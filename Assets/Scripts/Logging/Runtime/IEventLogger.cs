
namespace Logging
{
    // Event Logger is implemented using Unity's Debug class.
    // It provides category aware logging to support multiple log levels.
    // Essentially a wrapper for UnityEngine.Debug with log level filtering.
    public interface IEventLogger
    {
        // Enable or disable logging globally.
        // If false, no logs will be displayed.
        bool Enabled { get; set; }

        // Logs a diagnostic message at the Trace level.
        // param message - The message to log.
        // param category - The category of the log message.
        // param context - Optional context object for the log.
        void Trace(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs a debug message at the Debug level.
        // Used for development time debugging and tracing execution flow.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Debug(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs an informational message at the Info level.
        // Used for general state changes, events, and progress updates.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Info(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs a warning message at the Warn level.
        // Used for recoverable issues and potential problems.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Warn(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs an error using Unity's Debug.LogError.
        // Used for serious issues that require attention.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Error(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs an exception using Unity's Debug.LogException.
        // A hint can be provided for additional context.
        // param exception - The exception to log.
        // param hint - Optional hint message for context.
        // param context - Optional context object for the log.
        void Exception(System.Exception exception, LogCategory category = LogCategory.None, string message = null, object context = null);

        public void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None, object context = null);
    }
    // Defines the severity levels for logging output.
    // Used to filter log messages so that only messages
    // at or above the specified level are recorded.
    public enum LogLevel
    {
        // Most verbose logging level. Includes all messages.
        // Use for detailed tracing during development.
        Trace = 0,
        // Used for development time information and state changes
        // that are too verbose for production.
        Debug = 1,
        // General informational messages about the normal operation of the system.
        Info = 2,
        // Indicates potentially harmful situations or recoverable issues.
        Warn = 3,
        // Indicates serious errors that need attention.
        Error = 4,
        // All messages will be ignored.
        None = 5
    }

    // Defines categories for logging messages.
    // Multiple categories can be combined using bitwise "OR".
    [System.Flags]
    public enum LogCategory
    {
        // No category specified. If set, no logs will be shown.
        None = 0,
        // Core engine and systems logging.
        Core = 1 << 0,
        // Gamplay mechanics and rules logging
        Gameplay = 1 << 1,
        // User Interface related logging.
        UI = 1 << 2,
        // Economy and transactions logging.
        Economy = 1 << 3,
        // AI and bot behavior logging. If implemented.
        AI = 1 << 4,
        // Networking and multiplayer logging.
        Network = 1 << 5,
        // All categories enabled.
        All = ~0
    }
}
