
namespace Logging
{
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
        // No category specified.
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

    // Event Logger is implemented using Unity's Debug class.
    // It provides category aware logging to support multiple log levels.
    // Essentially a wrapper for UnityEngine.Debug with log level filtering.
    public interface IEventLogger
    {
        // Enable or disable logging globally. If false, no logs
        // will be displayed. Intended use case for the editor.
        bool Enabled { get; set; }
        // Logs a message with a specific log level. This is the core logging method 
        // invoked by higher-level helpers (Warn, Error, etc.). It applies filtering
        // before writing to the Unity Console.
        // param message - The message to log. Defaults to null.
        // param level - The severity level of the log message. Defaults to LogLevel.Info.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        public void Log(string message, LogLevel level = LogLevel.Info, LogCategory category = LogCategory.None, object context = null);
        // Logs a diagnostic message at the Trace level.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        void Trace(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs a debug message at the Debug level.
        // Used for development time debugging and tracing execution flow.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        void Debug(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs an informational message at the Info level.
        // Used for general state changes, events, and progress updates.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        void Info(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs a warning message at the Warn level.
        // Used for recoverable issues and potential problems.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        void Warn(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs an error using Unity's Debug.LogError.
        // Used for serious issues that require attention.
        // param message - The message to log. Defaults to null.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param context - Optional context object for the log. Defaults to null.
        void Error(string message, LogCategory category = LogCategory.None, object context = null);
        // Logs an exception using Unity's Debug.LogException.
        // A message can be provided for additional context.
        // param exception - The exception object to be logged.
        // param category - The category of the log message. Defaults to LogCategory.None.
        // param message - The message to log. Defaults to null.
        // param context - Optional context object for the log. Defaults to null.
        void Exception(System.Exception exception, LogCategory category = LogCategory.None, string message = null, object context = null);
    }
}
