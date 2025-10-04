using UnityEngine;

namespace PsycheOpoly.Logging
{

    public class EventLogger : IEventLogger
    {
        public bool Enabled { get; set; } = true;
        public LogLevel Level { get; set; } = LogLevel.Trace;
        public void Trace(string message, object context = null)
        {
            if (Enabled && Level <= LogLevel.Trace)
            {
                UnityEngine.Debug.Log($"[TRACE] {message}", context as UnityEngine.Object);
            }
        }
        public void Debug(string message, object context = null)
        {
            if (Enabled && Level <= LogLevel.Debug)
            {
                UnityEngine.Debug.Log($"[DEBUG] {message}", context as UnityEngine.Object);
            }
        }
        public void Info(string message, object context = null)
        {
            if (Enabled && Level <= LogLevel.Info)
            {
                UnityEngine.Debug.Log($"[INFO] {message}", context as UnityEngine.Object);
            }
        }
        public void Warn(string message, object context = null)
        {
            if (Enabled && Level <= LogLevel.Warn)
            {
                UnityEngine.Debug.LogWarning($"[WARN] {message}", context as UnityEngine.Object);
            }
        }
        public void Error(string message, object context = null)
        {
            if (Enabled && Level <= LogLevel.Error)
            {
                UnityEngine.Debug.LogError($"[ERROR] {message}", context as UnityEngine.Object);
            }
        }
        public void Exception(System.Exception exception, string hint = null, object context = null)
        {
            if (Enabled || Level > LogLevel.Error) return;
            if (string.IsNullOrEmpty(hint)) hint = "Exception";
            UnityEngine.Debug.LogException(new System.Exception(hint, exception), context as UnityEngine.Object);
        }
    }
}
