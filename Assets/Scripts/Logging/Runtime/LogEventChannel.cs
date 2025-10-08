using UnityEngine;

namespace Logging
{
    [UnityEngine.CreateAssetMenu(fileName = "LogEventChannel", menuName = "PsycheOpoly/Events/Log Event Channel")]
    public class LogEventChannel : EventChannel<LogEvent> { }
}
