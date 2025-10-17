using JetBrains.Annotations;
using Logging;
using UnityEngine;

namespace Logging
{
    public static class Logger
    {
        public static IEventLogger EventLogger { get; private set; }

        // Initializes the global "Logger" system with the specified settings and log prefix.
        // This method must be called before using "Logger.Log(string, LogLevel, LogCategory, object)" 
        // or any helper methods (e.g., Warn, Error). If not called, logging attempts will
        // issue a Unity warning indicating that the logger has not been initialized.
        // param settings - The LogSettings instance that controls logging behavior, such as enabled state,
        // minimum log level, and category filters.
        // param prefix - An optional prefix string to prepend to all log messages. Defaults to "PsycheOpoly".
        // Use cases can include game module, testing, etc.
        public static void Initialize(LogSettings settings, string prefix = "PsycheOpoly")
        {
            if (EventLogger != null)
            {
                return;
            }

            EventLogger = new EventLogger(settings, prefix);
            EventLogger.Info("Logger.Initialize",
                    "Logger is now initialized.",
                    LogCategory.Core);
        }
        // Logs a message with a specific log level. This is the core logging method 
        // invoked by higher-level helpers (Warn, Error, etc.). It applies filtering
        // before writing to the Unity Console.
        // param message - The message to log. Defaults to null.
        // param level - The severity level of the log message. Defaults to LogLevel.Info.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        public static void Log(LogEvent logEvent)
        {
            if (EventLogger == null)
            {
                logIsNullMessage();
                return;
            }
            EventLogger.Log(logEvent.EventName, logEvent.Message, logEvent.Level, logEvent.Category, logEvent.Context);
        }
        // Logs a diagnostic message at the Trace level.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        public static void Trace(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                logIsNullMessage();
                return;
            }
            EventLogger.Trace(eventName, message, category, context);
        }
        // Logs a debug message at the Debug level.
        // Used for development time debugging and tracing execution flow.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        public static void Debug(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                logIsNullMessage();
                return;
            }
            EventLogger.Debug(eventName, message, category, context);
        }
        // Logs an informational message at the Info level.
        // Used for general state changes, events, and progress updates.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        public static void Info(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                logIsNullMessage();
                return;
            }
            EventLogger.Info(eventName, message, category, context);
        }
        // Logs a warning message at the Warn level.
        // Used for recoverable issues and potential problems.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        public static void Warn(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                logIsNullMessage();
                return;
            }
            EventLogger.Warn(eventName, message, category, context);
        }
        // Logs an error using Unity's Debug.LogError.
        // Used for serious issues that require attention.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        public static void Error(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (EventLogger == null)
            {
                logIsNullMessage();
                return;
            }
            EventLogger.Error(eventName, message, category, context);
        }
        // Logs an exception using Unity's Debug.LogException.
        // A message can be provided for additional context.
        // param exception - The exception object to be logged.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param message - The message to log. Defaults to null.
        // param context - Optional context object for the log. Defaults to null.
        public static void Exception(string eventName, System.Exception exception, LogCategory category = LogCategory.None, string message = null, object context = null)
        {
            if (EventLogger == null)
            {
                logIsNullMessage();
                return;
            }
            EventLogger.Exception(exception, eventName, category, message, context);
        }
        private static void logIsNullMessage()
        {
            UnityEngine.Debug.LogWarning("Logger not initialized. Call Logger.Initialize(LogSetting setting, string prefix) first.");
        }
    }
}
