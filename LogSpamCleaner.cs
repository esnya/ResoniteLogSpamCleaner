using FrooxEngine;
using HarmonyLib;
using ResoniteModLoader;
using System;
using Elements.Core;
using System.Collections.Generic;
using System.Reflection;

namespace LogSpamCleaner
{
    public class LogSpamCleaner : ResoniteMod
    {
        public override string Name => "LogSpamCleaner";
        public override string Author => "esnya";
        public override string Version => "1.0.0";
        public override string Link => "https://github.com/esnya/ResoniteLogSpamCleaner";

        private static ModConfiguration config;

        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> allowStackTraceOnError = new ModConfigurationKey<bool>("AllowStackTraceOnError", "Allow stack trace on error", () => true);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> allowStackTraceOnWarning = new ModConfigurationKey<bool>("AllowStackTraceOnWarning", "Allow stack trace on warning", () => false);
        [AutoRegisterConfigKey]
        public static readonly ModConfigurationKey<bool> allowStackTraceOnLog = new ModConfigurationKey<bool>("AllowStackTraceOnLog", "Allow stack trace on log", () => false);

        public override void OnEngineInit()
        {
            config = GetConfiguration();
            config.Save(true);

            var harmony = new Harmony("com.nekometer.esnya.LogSpamCleaner");
            harmony.PatchAll();
        }

        private static bool IsForbidden(string name)
        {
            switch (name)
            {
                case "Log":
                    return !config.GetValue<bool>(allowStackTraceOnLog);
                case "Warning":
                    return !config.GetValue<bool>(allowStackTraceOnWarning);
                case "Error":
                    return !config.GetValue<bool>(allowStackTraceOnError);
                default:
                    return true;
            }
        }

        [HarmonyPatch]
        class PatchBase
        {
            static IEnumerable<MethodBase> TargetMethods()
            {
                var uniLogType = typeof(UniLog);
                var args = new Type[] { typeof(string), typeof(bool) };

                yield return uniLogType.GetMethod("Log", args);
                yield return uniLogType.GetMethod("Warning", args);
                yield return uniLogType.GetMethod("Error", args);
            }

            static void Prefix(MethodBase __originalMethod, string message, ref bool stackTrace)
            {
                if (stackTrace && IsForbidden(__originalMethod.Name))
                {
                    stackTrace = false;
                }
            }
        }
    }
}

