using UnityEngine;

namespace Logging
{
    /// <summary>
    /// Represents a single log entry transmitted via EventChannel.
    /// value-based type for safe broadcast.
    /// </summary>
    public class LogEvent
    {
        public LogLevel Level { get; }
        public LogCategory Category { get; }
        public string Message { get; }
        public Object Context { get; }
        public LogEvent(LogLevel level, LogCategory category, string message, Object context = null)
        {
            Level = level;
            Category = category;
            Message = message;
            Context = context;
        }
    }
}
