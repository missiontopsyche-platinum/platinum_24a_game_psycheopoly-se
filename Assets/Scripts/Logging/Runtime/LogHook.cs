using Logging;
using UnityEngine;

public class LogHook : MonoBehaviour
{
    [SerializeField]
    private LogLevel minLogLevel = LogLevel.Info;
    [SerializeField]
    private LogCategory logCategory = LogCategory.All;
    [SerializeField]
    private EventChannel<LogEvent> logEventChannel = null;

    private void OnEnable()
    {
        logEventChannel?.Subscribe(OnLogEvent);
    }
    private void OnDisable()
    {
        logEventChannel?.Unsubscribe(OnLogEvent);
    }
    private void OnLogEvent(LogEvent logEvent)
    {
        if (logEvent.Level < minLogLevel) return;
        if (logCategory != LogCategory.All &&
            (logEvent.Category == LogCategory.None || (logCategory & logEvent.Category) == 0))
            return;

        Logging.Logger.Log(logEvent.Message, logEvent.Level, logEvent.Category, logEvent.Context);
    }
}
