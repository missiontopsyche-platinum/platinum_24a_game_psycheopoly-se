using NUnit.Framework;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using Logging;

namespace Tests.Logging
{
    public class EventLoggerTests
    {

        private EventLogger _logger;

        [SetUp]
        public void SetUp()
        {
            _logger = new EventLogger();

            _logger.Enabled = true;
            _logger.Level = LogLevel.Info;
            _logger.EnabledCategories = LogCategory.All;
        }

        [TearDown]
        public void TearDown()
        {
            LogAssert.ignoreFailingMessages = false;
        }

        [Test]
        public void DisabledLogger_DoesNotEmitAnything()
        {
            _logger.Enabled = false;
            SafeLog(LogLevel.Info, "should not appear", LogCategory.Core);
            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LevelFiltering_InfoLevel_AllowsInfoWarnError_BlocksDebugTrace()
        {
            _logger.Level = LogLevel.Info;

            ExpectInfo("info visible");
            SafeLog(LogLevel.Info, "info visible", LogCategory.Core);

            ExpectWarning("warn visible");
            SafeLog(LogLevel.Warn, "warn visible", LogCategory.Core);

            ExpectError("error visible");
            SafeLog(LogLevel.Error, "error visible", LogCategory.Core);

            ExpectException("exception visible");
            SafeLog(null, "exception visible", LogCategory.Core, new Exception("exception visible"));

            SafeLog(LogLevel.Debug, "debug hidden", LogCategory.Core);
            SafeLog(LogLevel.Trace, "trace hidden", LogCategory.Core);

            LogAssert.NoUnexpectedReceived();
        }

        [Test]
        public void LevelFiltering_WarnLevel_AllowsWarnError_BlocksInfoAndBelow()
        {
            _logger.Level = LogLevel.Warn;

            ExpectWarning("warn visible");
            SafeLog(LogLevel.Warn, "warn visible", LogCategory.Gameplay);

            ExpectError("error visible");
            SafeLog(LogLevel.Error, "error visible", LogCategory.Gameplay);

            ExpectException("exception visible");
            SafeLog(null, "exception visible", LogCategory.Core, new Exception("exception visible"));

            SafeLog(LogLevel.Info, "info hidden", LogCategory.Gameplay);
            SafeLog(LogLevel.Debug, "debug hidden", LogCategory.Gameplay);
            SafeLog(LogLevel.Trace, "trace hidden", LogCategory.Gameplay);

            LogAssert.NoUnexpectedReceived();
        }

        private void SafeLog(LogLevel? level, string msg, LogCategory cat, Exception ex = null)
        {
            switch (level)
            {
                case LogLevel.Trace:
                    _logger.Trace(msg, cat, this);
                    break;
                case LogLevel.Debug:
                    _logger.Debug(msg, cat, this);
                    break;
                case LogLevel.Info:
                    _logger.Info(msg, cat, this);
                    break;
                case LogLevel.Warn:
                    _logger.Warn(msg, cat, this);
                    break;
                case LogLevel.Error:
                    _logger.Error(msg, cat, this);
                    break;
                default:
                    _logger.Exception(ex, cat, msg, this);
                    break;
            }
        }
        private void SafeLogFormat(LogLevel level, string fmt, LogCategory cat, params object[] args)
        {
            var msg = string.Format(fmt ?? string.Empty, args);
            SafeLog(level, msg, cat);
        }

        private static void ExpectInfo(string contains)
        {
            LogAssert.Expect(LogType.Log, new System.Text.RegularExpressions.Regex(RegexEscape(contains)));
        }

        private static void ExpectWarning(string contains)
        {
            LogAssert.Expect(LogType.Warning, new System.Text.RegularExpressions.Regex(RegexEscape(contains)));
        }

        private static void ExpectError(string contains)
        {
            LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex(RegexEscape(contains)));
        }

        void ExpectException(string contains)
        {
            LogAssert.Expect(LogType.Exception, new System.Text.RegularExpressions.Regex(RegexEscape(contains)));
        }

        private static string RegexEscape(string s) => System.Text.RegularExpressions.Regex.Escape(s ?? string.Empty);
    }
}
