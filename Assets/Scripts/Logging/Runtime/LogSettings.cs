using UnityEngine;

namespace Logging
{
    [CreateAssetMenu(fileName = "LogSettings", menuName = "Logging/Log Settings")]
    public class LogSettings : ScriptableObject
    {
        [Header("Master Switch")]
        [Tooltip("Completely disable all runtime logging when unchecked.")]
        public bool LoggingEnabled = true;

        [Header("Enable by context")]
        public bool EnableInEditorPlaymode = true;
        public bool EnableInDevelopmentBuild = true;
        public bool EnableInReleaseBuild = false;

        [Header("Log Level and Categories")]
        public LogLevel MinLogLevel = LogLevel.Trace;
        public LogCategory EnabledCategories = LogCategory.All;
        public bool ErrorsAlwaysEnabled = true;

        private static LogSettings _instance;
        public static LogSettings Current()
        {
            return _instance??= Resources.Load<LogSettings>("Logging/LogSettings");
        }

        public bool isRunTimeLoggingEnabled()
        {
            if (Application.isEditor && Application.isPlaying)
            {
                return EnableInEditorPlaymode;
            }

            if (Debug.isDebugBuild)
            {
                return EnableInDevelopmentBuild;
            }

            return EnableInReleaseBuild;
        }

        public bool isCategoryEnabled(LogCategory category)
        {
            return (EnabledCategories & category) != 0;
        }
    }
}
