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

        public void Trace(string message = null, LogCategory category = LogCategory.None, object context = null)
        {
            Log(message, LogLevel.Trace, category, context);
        }
        public void Debug(string message = null, LogCategory category = LogCategory.None, object context = null)
        {
            Log(message, LogLevel.Debug, category, context);
        }
        public void Info(string message = null, LogCategory category = LogCategory.None, object context = null)
        {
            Log(message, LogLevel.Info, category, context);
        }
        public void Warn(string message = null, LogCategory category = LogCategory.None, object context = null)
        {
            Log(message, LogLevel.Warn, category, context);
        }
        public void Error(string message = null, LogCategory category = LogCategory.None, object context = null)
        {
            Error(message, category, context, null);
        }
        public void Exception(System.Exception exception, LogCategory category = LogCategory.None, string message = null, object context = null)
        {
            Error(message ?? "Exception occurred", category, context, exception);
        }

        public void Log(string message = null, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None, object context = null)
        {

            if (!isLoggable(level, category)) return;

            if (string.IsNullOrEmpty(message))
                message = "None";

            switch (level)
            {
                case LogLevel.Trace:
                    UnityEngine.Debug.Log(FormatMessage(message, level, category), context as UnityEngine.Object);
                    break;
                case LogLevel.Debug:
                    UnityEngine.Debug.Log(FormatMessage(message, level, category), context as UnityEngine.Object);
                    break;
                case LogLevel.Info:
                    UnityEngine.Debug.Log(FormatMessage(message, level, category), context as UnityEngine.Object);
                    break;
                case LogLevel.Warn:
                    UnityEngine.Debug.LogWarning(FormatMessage(message, level, category), context as UnityEngine.Object);
                    break;
                default:
                    UnityEngine.Debug.Log(FormatMessage(message, level, category), context as UnityEngine.Object);
                    break;
            }
        }

        public void Error(string message = null, LogCategory category = LogCategory.None, object context = null, System.Exception exception = null)
        {
            var level = LogLevel.Error;

            if (!isLoggable(level, category)) return;

            if (string.IsNullOrEmpty(message))
                message = "None";

            if (exception != null)
            {
                UnityEngine.Debug.LogException(new System.Exception(FormatMessage(message, level, category) + $": { exception.Message}", exception), context as UnityEngine.Object);
                return;
            }
            UnityEngine.Debug.LogError($"{_prefix} [ERROR] [{category}]{message}", context as UnityEngine.Object);
        }

        private bool isLoggable(LogLevel level, LogCategory category)
        {
            if (!Enabled) return false;

            if (!_settings.isRunTimeLoggingEnabled())
            {
                if (!(_settings.ErrorsAlwaysEnabled && level == LogLevel.Error))
                    return false;
            }

            if (level < _settings.MinLogLevel) return false;

            if (category == LogCategory.None)
                return _settings.EnabledCategories != LogCategory.None;

            return (_settings.EnabledCategories & category) != 0;
        }

        private string FormatMessage(string message, LogLevel level, LogCategory category)
        {
            return $"{_prefix} [Level: {level}] [Category: {category}] [Message: {message}]";
        }
    }
}
