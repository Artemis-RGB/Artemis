using System.Linq;
using Artemis.Controls.Log;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace Artemis.Utilities
{
    public static class Logging
    {
        public static event LoggingEvent MemoryEvent;

        public static void SetupLogging(string logLevel)
        {
            SetupLogging(LogLevel.FromString(logLevel));
        }

        public static void SetupLogging(LogLevel logLevel)
        {
            if (logLevel == LogManager.Configuration?.LoggingRules?.FirstOrDefault()?.Levels.FirstOrDefault())
                return;

            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var debuggerTarget = new DebuggerTarget();
            config.AddTarget("debugger", debuggerTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            var memoryTarget = new MemoryEventTarget();
            memoryTarget.EventReceived += MemoryTargetOnEventReceived;
            config.AddTarget("memory", memoryTarget);

            // Step 3. Set target properties 
            debuggerTarget.Layout = @"${logger:shortName=True} - ${uppercase:${level}}: ${message}";
            fileTarget.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}";
            fileTarget.FileName = "${specialfolder:folder=CommonApplicationData}/Artemis/logs/log.txt";
            fileTarget.ArchiveFileName = "${specialfolder:folder=CommonApplicationData}/Artemis/logs/log-{#}.txt";
            fileTarget.ArchiveEvery = FileArchivePeriod.Day;
            fileTarget.ArchiveNumbering = ArchiveNumberingMode.Date;
            fileTarget.ArchiveOldFileOnStartup = true;
            fileTarget.MaxArchiveFiles = 7;

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", logLevel, debuggerTarget);
            config.LoggingRules.Add(rule1);
            var rule2 = new LoggingRule("*", logLevel, fileTarget);
            config.LoggingRules.Add(rule2);
            var rule3 = new LoggingRule("*", logLevel, memoryTarget);
            config.LoggingRules.Add(rule3);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            // Log as fatal so it always shows, add some spacing since this indicates the start of a new log
            var logger = LogManager.GetCurrentClassLogger();
            var logMsg = $"  INFO: Set log level to {logLevel}  ";
            logger.Fatal(new string('-', logMsg.Length));
            logger.Fatal(logMsg);
            logger.Fatal(new string('-', logMsg.Length));
        }

        private static void MemoryTargetOnEventReceived(LogEventInfo logEventInfo)
        {
            MemoryEvent?.Invoke(logEventInfo);
        }

        public static void ClearLoggingEvent()
        {
            MemoryEvent = delegate { };
        }
    }

    public delegate void LoggingEvent(LogEventInfo logEventInfo);
}