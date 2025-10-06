# Logger: Basic Usage Guide
Implemented using Unity's Debug class. It provides category aware logging to 
support multiple log levels. Essentially, this class is a wrapper for 
UnityEngine.Debug with log level filtering.
## UI Guide: Editor Setup
### A) Create the settings asset
1. In Unity: Assets → Create → Logging → Log Settings.
2. Name it LogSettings and place it under Assets/Resources/Logging so it can be 
    auto-loaded at runtime.
### B) Configure the behavior
 Open the created LogSettings asset and set:
 - **Master Switch**:
    - *LoggingEnabled*: uncheck to silence everything at runtime. Defaults to 
        On.
- **Enable by context**: This determines if logs are allowed in each context 
    mentioned below:
    - *EnableInEditorPlaymode*: Enable the system during Editor and Play
        modes. Defaults to On.
    - *EnableInDevelopmentBuild*: Enable the system during development mode.
        Defaults to On.
    - *EnableInReleaseBuild*: Enable the system during production mode.
        Defults to Off.
- **Log Level & Categories**: Defines the severity levels for logging output. 
    Used to filter log messages so that only messages at or above the specified 
    level are recorded. 
    - *MinLogLevel*: lowest severity you want to see. 
    (Trace < Debug < Info < Warn < Error).
        - *Trace*: Use for detailed tracing during development.
        - *Debug*: Used for development time information and state changes that 
            are too verbose for production.
        - *Info*: Use for informational messages about the normal operation of 
            the system.
        - *Warn*: Use to indicates potentially harmful situations or recoverable 
            issues.
        - *Error*: Indicates serious errors that need attention.
        - *None*: All messages will be ignored.
    - *EnabledCategories*: choose one or more (Core, Gameplay, UI, Economy, AI, 
        Network). These are bitwise flags and can be combined (the Unity 
        inspector shows them as a multi-select). **Setting one or multiple flags will only allow the log statements that were categorized as such**.
        - *None*: No category specified.
        - *Core*: Core engine and systems logging.
        - *Gameplay*: Gamplay mechanics and rules logging.
        - *UI*: User Interface related logging.
        - *Economy*: Economy and transactions logging.
        - *AI*: AI and bot behavior logging. If implemented.
        - *Network*: Networking and multiplayer logging.
        - *All*: All categories enabled.
    - *ErrorsAlwaysEnabled*: keeps Error visible even if runtime logging is 
        disabled for the build/context.
        > Tip: Start with MinLogLevel = Debug in dev, and flip to Warn or Error for 
            release builds.
## Code Guide
### A) Initialize once (e.g., in a bootstrap MonoBehavior)
~~~
using Logging;
using UnityEngine;

public class LoggingBootstrap : MonoBehaviour
{
    void Awake()
    {
        // Auto-loads Resources/LogSettings
        var settings = LogSettings.Current();
        Logger.Initialize(settings, prefix: "PsycheOpoly"); // optional prefix
    }
}
~~~
- **Logger.Initialize** wires up the static façade to the concrete EventLogger. 
    If you forget this, Logger methods warn you at runtime.
    - *LogSettings*: the settings you've configured.
    - *prefix*: appears in each formatted message (e.g., “PsycheOpoly [Level: …]
        [Category: …]”).
### B) Log with the static façade
~~~
using Logging;
using UnityEngine;

public class PlayerSpawner : MonoBehaviour
{
    void Start()
    {
        Logger.Info("Spawning players...", LogCategory.Gameplay, this);
        Logger.Debug("Seed=42", LogCategory.Core, this);
        Logger.Warn("No default avatar set", LogCategory.UI, this);
    }

    void OnError(System.Exception ex)
    {
        Logger.Exception(ex, LogCategory.Core, "Spawn failed", this);
    }
}
~~~
- Use categories to slice up the display (Core, Gameplay, UI, Economy, AI, 
    Network). They’re Flags, so you can combine them with OR if needed, such as 
    the example below:
    ~~~
    LogLevel level = LogLevel.UI | LogLevel.Gameplay;
    ~~~
- Context can be a UnityEngine.Object so Unity links the log to a scene/object.
    All level-specific methods forward to the concrete logger.
### C) Use the concrete IEventLogger (EventLogger)
If you need multiple loggers (e.g., different prefixes or settings), you can
create a new instance of it:
~~~
IEventLogger custom = new EventLogger(LogSettings.Current(), "AI");
custom.Info("Planner started", LogCategory.AI, this);
~~~
- The implementation filters by: global enable, runtime context, 
    ErrorsAlwaysEnabled, MinLogLevel, and category flags—before sending to 
    Unity’s console via UnityEngine.Debug (Log/Warning/Error/Exception).
## Minimal Example
1. Create Resources/LogSettings and set:
    - LoggingEnabled = true
    - EnableInEditorPlaymode = true
    - MinLogLevel = Debug
    - EnabledCategories = Gameplay | Core
1. Add the component with Logger.Initialize(LogSettings.Current(), 
    "PsycheOpoly")
1. Use example:
    ~~~
    Logger.Info("Round started", LogCategory.Gameplay, this);
    Logger.Warn("Missing avatar", LogCategory.UI, this);
    Logger.Exception(new System.Exception("Boom"), LogCategory.Core, "While spawning", this);
    ~~~
    Or you can use the more verbose option:
    ~~~
    Logger.Log("Round started", LogLevel.Info, LogCategory.Gameplay, this)
    Logger.Log("Missing avatar", LogLevel.Warn, LogCategory.UI, this)
    ~~~
    > Note: Exception is not supported within the Log method. Therefore, when
    > logging exception, you must use Logger.Exception() method.
## Filtering
This is how the filtering actually works. A message is emitted only if all of
    the following are true.
1. Enabled property on the logger instance and LogSettings.LoggingEnabled are 
    set to true.
1. Runtime context is permitted (EnableInEditorPlaymode, or dev build, or 
    release as configured).
1. If runtime logging is not permitted, ErrorsAlwaysEnabled still allows Error 
    logs through.
1. level >= MinLogLevel.
1. Category is included in EnabledCategories (unless you pass None, which 
    behaves like “no category filter,” but still requires that some categories 
    are enabled globally).
    > Format example produced by the logger:
    > ~~~
    > PsycheOpoly [Level: Info] [Category: Gameplay] [Message: Spawning players...]
    > ~~~
## Components
- **LogSettings**: this is a ScriptableObject asset that controls when/what to 
    log (master switch, build-type toggles, min level, categories). 
- **IEventLogger**: this is an interface that defines the API 
    (Trace/Debug/Info/Warn/Error/Exception + generic Log). Includes LogLevel and 
    Flags LogCategory. 
- **EventLogger**:this is the concrete Unity-backed implementation with 
    filtering and formatted output.
- **Logger** (static façade): one-line initialization and static helper methods 
    (Logger.Debug/Info/etc.).