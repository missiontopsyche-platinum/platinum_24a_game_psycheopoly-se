using System;
using UnityEngine;

namespace Logging
{

    public class EventLogger : IEventLogger
    {
        public bool Enabled { get; set; } = true;
        private LogSettings _settings;
        private readonly string _prefix;
        public EventLogger(LogSettings settings, string prefix = "PsycheOpoly")
        {
            _settings = settings;
            _prefix = prefix;
        }

        public void Trace(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            Log(eventName, message, LogLevel.Trace, category, context);
        }
        public void Debug(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            Log(eventName, message, LogLevel.Debug, category, context);
        }
        public void Info(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            Log(eventName, message, LogLevel.Info, category, context);
        }
        public void Warn(string eventName, string message, LogCategory category = LogCategory.None, object context = null)
        {
            Log(eventName, message, LogLevel.Warn, category, context);
        }
        public void Error(string eventName, string message = null, LogCategory category = LogCategory.None, object context = null)
        {
            Log(eventName, message, LogLevel.Error, category, context);
        }
        public void Exception(System.Exception exception, string eventName, LogCategory category = LogCategory.None, string message = null, object context = null)
        {
            if (!isLoggable(LogLevel.Error, category)) return;

            UnityEngine.Debug.LogException(new System.Exception(FormatMessage(eventName, LogLevel.Error, category, message) + $"Exception: \n{exception.Message}", exception), context as UnityEngine.Object);
        }

        public void Log(string eventName, string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None, object context = null)
        {

            if (!isLoggable(level, category)) return;

            switch (level)
            {
                case LogLevel.Trace:
                    UnityEngine.Debug.Log(FormatMessage(eventName, level, category, message), context as UnityEngine.Object);
                    break;
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(FormatMessage(eventName, level, category, message), context as UnityEngine.Object);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(FormatMessage(eventName, level, category, message), context as UnityEngine.Object);
                    break;
                case LogLevel.Warn:
                    UnityEngine.Debug.LogWarning(FormatMessage(eventName, level, category, message), context as UnityEngine.Object);
                    break;
                case LogLevel.Error:
                    UnityEngine.Debug.LogError(FormatMessage(eventName, level, category, message), context as UnityEngine.Object);
                    break;
                default:
                    UnityEngine.Debug.Log(FormatMessage(eventName, level, category, message), context as UnityEngine.Object);
                    break;
            }
        }

        private bool isLoggable(LogLevel level, LogCategory category)
        {
            if (!Enabled) return false;

            if (!_settings.LoggingEnabled) return false;

            if (!(_settings.isRunTimeLoggingEnabled() && _settings.isCategoryEnabled(category)))
            {
                if (!(_settings.ErrorsAlwaysEnabled && level == LogLevel.Error))
                    return false;
            }

            if (level < _settings.MinLogLevel) return false;

            if (category == LogCategory.None)
                return _settings.EnabledCategories != LogCategory.None;

            return (_settings.EnabledCategories & category) != 0;
        }

        private string FormatMessage(string eventName, LogLevel level, LogCategory category, string message = null)
        {
            string currentTimeString = DateTime.Now.ToString("HH:mm:ss:ff");
            if (string.IsNullOrEmpty(message))
                message = "None";
            return $"[{currentTimeString}] [Level: {level}] [Category: {category}] [Event Name: {eventName}] [Message: {message}]";
        }
    }
}
