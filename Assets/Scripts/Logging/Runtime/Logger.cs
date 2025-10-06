using JetBrains.Annotations;
using Logging;
using UnityEngine;

namespace Logging
{
    public class Logger
    {
        public static IEventLogger EventLogger { get; private set; }

        public static void Initialize(LogSettings settings, string prefix = "PsycheOpoly")
        {
            EventLogger = new EventLogger(settings, prefix);
        }

        public static void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                UnityEngine.Debug.LogWarning("Logger not initialized. Call Logger.Initialize(LogSetting setting, string prefix) first.");
                return;
            }
            EventLogger.Log(message, level, category, context);
        }
        public static void Trace(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                UnityEngine.Debug.LogWarning("Logger not initialized. Call Logger.Initialize(LogSetting setting, string prefix) first.");
                return;
            }
            EventLogger.Trace(message, category, context);
        }
        public static void Debug(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                UnityEngine.Debug.LogWarning("Logger not initialized. Call Logger.Initialize(LogSetting setting, string prefix) first.");
                return;
            }
            EventLogger.Debug(message, category, context);
        }
        public static void Info(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                UnityEngine.Debug.LogWarning("Logger not initialized. Call Logger.Initialize(LogSetting setting, string prefix) first.");
                return;
            }
            EventLogger.Info(message, category, context);
        }
        public static void Warn(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                UnityEngine.Debug.LogWarning("Logger not initialized. Call Logger.Initialize(LogSetting setting, string prefix) first.");
                return;
            }
            EventLogger.Warn(message, category, context);
        }
        public static void Error(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                UnityEngine.Debug.LogWarning("Logger not initialized. Call Logger.Initialize(LogSetting setting, string prefix) first.");
                return;
            }
            EventLogger.Error(message, category, context);
        }
        public static void Exception(System.Exception exception, LogCategory category = LogCategory.None, string message = null, object context = null)
        {
            if (EventLogger == null)
            {
                UnityEngine.Debug.LogWarning("Logger not initialized. Call Logger.Initialize(LogSetting setting, string prefix) first.");
                return;
            }
            EventLogger.Exception(exception, category, message, context);
        }
    }
}
