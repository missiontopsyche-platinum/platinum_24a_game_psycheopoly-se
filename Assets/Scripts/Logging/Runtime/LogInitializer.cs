using UnityEngine;

namespace Logging
{
    public class LogInitializer : MonoBehaviour
    {
        [SerializeField]
        private LogSettings logSettings;

        private void Awake()
        {
            Logging.Logger.Initialize(logSettings);
        }

        private void OnDestroy()
        {
            Logging.Logger.Reset();
        }
    }
}

