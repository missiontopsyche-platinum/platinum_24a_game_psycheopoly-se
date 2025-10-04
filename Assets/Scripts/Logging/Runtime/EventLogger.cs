using UnityEngine;

namespace Logging
{

    public class EventLogger : IEventLogger
    {
        public bool Enabled { get; set; } = true;
        public LogLevel Level { get; set; } = LogLevel.Trace;
        public LogCategory EnabledCategories { get; set; } = LogCategory.All;
        private readonly string _prefix;
        public EventLogger(string prefix = "PsycheOpoly")
        {
            _prefix = prefix;
        }
        public void Trace(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (IsWithinLevelAndEnabled(LogLevel.Trace, category))
            {
                UnityEngine.Debug.Log($"{_prefix} [TRACE] [{category}]{message}", context as UnityEngine.Object);
            }
        }
        public void Debug(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (IsWithinLevelAndEnabled(LogLevel.Debug, category))
            {
                UnityEngine.Debug.Log($"{_prefix} [DEBUG] {message}", context as UnityEngine.Object);
            }
        }
        public void Info(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (IsWithinLevelAndEnabled(LogLevel.Info, category))
            {
                UnityEngine.Debug.Log($"{_prefix} [INFO] {message}", context as UnityEngine.Object);
            }
        }
        public void Warn(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (IsWithinLevelAndEnabled(LogLevel.Warn, category))
            {
                UnityEngine.Debug.LogWarning($"{_prefix} [WARN] {message}", context as UnityEngine.Object);
            }
        }
        public void Error(string message, LogCategory category = LogCategory.None, object context = null)
        {
            if (IsWithinLevelAndEnabled(LogLevel.Error, category))
            {
                UnityEngine.Debug.LogError($"{_prefix} [ERROR] {message}", context as UnityEngine.Object);
            }
        }
        public void Exception(System.Exception exception, LogCategory category = LogCategory.None, string hint = null, object context = null)
        {
            if (!IsWithinLevelAndEnabled(LogLevel.Error, category)) return;
            if (string.IsNullOrEmpty(hint)) hint = "Exception";
            UnityEngine.Debug.LogException(new System.Exception($"{_prefix} [{category}] {hint}: {exception.Message}", exception), context as UnityEngine.Object);
        }
        private bool IsWithinLevelAndEnabled(LogLevel level, LogCategory category)
        {
            if (!Enabled) return false;
            if (Level > level) return false;
            if ((EnabledCategories & category) == 0) return false;
            return true;
        }
    }
}
