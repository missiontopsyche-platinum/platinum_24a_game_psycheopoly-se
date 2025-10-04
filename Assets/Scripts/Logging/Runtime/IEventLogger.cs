
namespace PsycheOpoly.Logging
{
    public interface IEventLogger
    {
        bool Enabled { get; set; }
        LogLevel Level { get; set; }
        void Trace(string message, object context = null);
        void Debug(string message, object context = null);
        void Info(string message, object context = null);
        void Warn(string message, object context = null);
        void Error(string message, object context = null);
        void Exception(System.Exception exception, string hint = null, object context = null);
    }

    public enum LogLevel
    {
        Trace = 0,
        Debug = 1,
        Info = 2,
        Warn = 3,
        Error = 4,
        None = 5
    }
}
