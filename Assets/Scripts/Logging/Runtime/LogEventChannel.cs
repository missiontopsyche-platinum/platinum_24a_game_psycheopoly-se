using UnityEngine;

namespace Logging
{
    [UnityEngine.CreateAssetMenu(fileName = "LogEventChannel", menuName = "Logging/Log Event Channel")]
    public class LogEventChannel : EventChannel<LogEvent> { }
}
