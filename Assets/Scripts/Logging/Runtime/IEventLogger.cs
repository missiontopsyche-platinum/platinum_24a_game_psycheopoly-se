
namespace PsycheOpoly.Logging
{
    // Event Logger is implemented using Unity's Debug class.
    // It provides category aware logging to support multiple log levels.
    // Essentially a wrapper for UnityEngine.Debug with log level filtering.
    public interface IEventLogger
    {
        // Enable or disable logging globally.
        // If false, no logs will be displayed.
        bool Enabled { get; set; }
        // Minimum log level that will be displayed.
        // Logs below this level will be ignored.
        LogLevel Level { get; set; }

        // Logs a diagnostic message at the Trace level.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Trace(string message, object context = null);
        // Logs a debug message at the Debug level.
        // Used for development time debugging and tracing execution flow.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Debug(string message, object context = null);
        // Logs an informational message at the Info level.
        // Used for general state changes, events, and progress updates.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Info(string message, object context = null);
        // Logs a warning message at the Warn level.
        // Used for recoverable issues and potential problems.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Warn(string message, object context = null);
        // Logs an error message at the Error level.
        // Used for serious issues that require attention.
        // param message - The message to log.
        // param context - Optional context object for the log.
        void Error(string message, object context = null);
        // Logs an exception using Unity's Debug.LogException.
        // A hint can be provided for additional context.
        // param exception - The exception to log.
        // param hint - Optional hint message for context.
        // param context - Optional context object for the log.
        void Exception(System.Exception exception, string hint = null, object context = null);
    }
    // Defines the severity levels for logging output.
    // Used to filter log messages so that only messages
    // at or above the specified level are recorded.
    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        None = 5
    }

    [System.Flags]
    public enum LogCategory
    {
        None = 0,
        Core = 1 << 0,
        Gameplay = 1 << 1,
        UI = 1 << 2,
        Economy = 1 << 3,
        AI = 1 << 4,
        Network = 1 << 5,
        All = ~0
    }
}
