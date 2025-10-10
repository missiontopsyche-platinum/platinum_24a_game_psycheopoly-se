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
  - _LoggingEnabled_: uncheck to silence everything at runtime. Defaults to
    On.
- **Enable by context**: This determines if logs are allowed in each context
  mentioned below:
  - _EnableInEditorPlaymode_: Enable the system during Editor and Play
    modes. Defaults to On.
  - _EnableInDevelopmentBuild_: Enable the system during development mode.
    Defaults to On.
  - _EnableInReleaseBuild_: Enable the system during production mode.
    Defults to Off.
- **Log Level & Categories**: Defines the severity levels for logging output.
  Used to filter log messages so that only messages at or above the specified
  level are recorded.
  - _MinLogLevel_: lowest severity you want to see.
    (Trace < Debug < Info < Warn < Error). - _Trace_: Use for detailed tracing during development. - _Debug_: Used for development time information and state changes that
    are too verbose for production. - _Info_: Use for informational messages about the normal operation of
    the system. - _Warn_: Use to indicates potentially harmful situations or recoverable
    issues. - _Error_: Indicates serious errors that need attention. - _None_: All messages will be ignored.
  - _EnabledCategories_: choose one or more (Core, Gameplay, UI, Economy, AI,
    Network). These are bitwise flags and can be combined (the Unity
    inspector shows them as a multi-select). **Setting one or multiple flags will only allow the log statements that were categorized as such**.
    - _None_: No category specified.
    - _Core_: Core engine and systems logging.
    - _Gameplay_: Gamplay mechanics and rules logging.
    - _UI_: User Interface related logging.
    - _Economy_: Economy and transactions logging.
    - _AI_: AI and bot behavior logging. If implemented.
    - _Network_: Networking and multiplayer logging.
    - _All_: All categories enabled.
  - _ErrorsAlwaysEnabled_: keeps Error visible even if runtime logging is
    disabled for the build/context.
    > Tip: Start with MinLogLevel = Debug in dev, and flip to Warn or Error for
          release builds.

## Code Guide

### Initialization:

```
var settings = LogSettings.Current();
Logger.Initialize(settings, prefix: "PsycheOpoly");
```

### Static API:

Write logs directly

```
Logger.Trace("pathfind", "Entering A*", LogCategory.AI, this);
Logger.Debug("spawn", "Created 4 tiles", LogCategory.Gameplay);
Logger.Info("session_start", "Player logged in", LogCategory.Core);
Logger.Warn("low_balance", "Under 10 coins", LogCategory.Economy);
Logger.Error("http_500", "Server error", LogCategory.Network);
Logger.Exception("save_fail", ex, LogCategory.Core, message: "While writing save file");
```

What it actually does:

- Applies the full isLoggable filter: master switches → environment gates → ErrorsAlwaysEnabled → min level → category mask.
- Formats messages uniformly: [Level:X] [Category:Y] [Event Name:Z] [Message:M] with your prefix.
- Sends to Debug.Log / LogWarning / LogError (or LogException for exceptions).

### Event‑based logging with LogEventChannel and LogHook:

#### Setup

1. Create a LogEventChannel asset (Create → PsycheOpoly → Events → Log Event Channel).
1. In a scene, add a LogHook component to any GameObject and assign that channel.

#### Emitting log events

- From gameplay scripts that have a reference to the channel:

```
public LogEventChannel Channel; // assign in inspector

    void OnPurchase()
    {
        Channel.RaiseEvent(new LogEvent(
            eventName: "purchase",
            level: LogLevel.Info,
            category: LogCategory.Economy,
            message: "Sword x1",
            context: this
        ));
        // More Logic...
    }
```

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
   >
   > ```
   > PsycheOpoly [Level: Info] [Category: Gameplay] [Event Name: Example] [Message: Spawning players...]
   > ```

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
