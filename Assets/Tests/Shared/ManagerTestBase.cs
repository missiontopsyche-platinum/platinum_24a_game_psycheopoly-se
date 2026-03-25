using Logging;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using TestLogger = Logging.Logger; // Alias to avoid namespace clash. Not sure how to resolve without an asmdef file.

namespace Tests.EditMode
{
    public class ManagerTestBase
    {
        protected List<ScriptableObject> eventChannels = new();
        protected List<ScriptableObject> scriptableObjects = new();
        protected IEventLogger logger;

        protected T CreateChannel<T>() where T : ScriptableObject
        {
            T channel = ScriptableObject.CreateInstance<T>();
            eventChannels.Add(channel);
            return channel;
        }

        protected T TrackScriptableObject<T>(T so) where T : ScriptableObject
        {
            scriptableObjects.Add(so);
            return so;
        }
        
        protected T CreateAndAttachComponent<T>(string componentName, GameObject parent) where T : Component
        {
            var component = new GameObject(componentName);
            component.transform.SetParent(parent.transform);
            return component.AddComponent<T>();
        }
        protected void DestroyTestObjects(params UnityEngine.Object[] objects)
        {
            // destroy all passed objects
            foreach (var obj in objects)
                if (obj != null)
                    Object.DestroyImmediate(obj);
        
            // destroy all event channels
            foreach (var channel in eventChannels)
                if (channel != null)
                    Object.DestroyImmediate(channel);
            
            // destroy all tracked scriptable objects
            foreach (var so in scriptableObjects)
                if (so != null)
                    Object.DestroyImmediate(so);
        
            eventChannels.Clear();
            scriptableObjects.Clear();

            // Destroy test logger
            TestLogger.Reset();
        }
        // These are to make sure that each test has a fresh logger with known settings
        protected void InitializeTestLogger()
        {
            LogSettings settings = ScriptableObject.CreateInstance<LogSettings>();
            settings.LoggingEnabled = true;
            settings.MinLogLevel = LogLevel.Trace;
            settings.EnabledCategories = LogCategory.All;
            TestLogger.Initialize(settings, "Test", true);
            logger = TestLogger.EventLogger;
        }

        // This is to ignore the timestamps from the log statements, so it does not fail unit tests.
        protected Regex CreateRegexLogPattern(string logLevel, string category, string eventName, string message)
        {
            return new Regex(
                        $@"^\[.+\] Test \[Level: {logLevel}\] " +
                        $@"\[Category: {category}\] " +
                        $@"\[Event Name: {eventName}\] " +
                        $@"\[Message: {message}\]$");
        }
    }
}
