using NLog;
using NLog.Config;
using NLog.Targets;

namespace Artemis.Utilities
{
    public static class Logging
    {
        public static void SetupLogging(string logLevel)
        {
            SetupLogging(LogLevel.FromString(logLevel));
        }

        public static void SetupLogging(LogLevel logLevel)
        {
            // Step 1. Create configuration object 
            var config = new LoggingConfiguration();

            // Step 2. Create targets and add them to the configuration 
            var debuggerTarget = new DebuggerTarget();
            config.AddTarget("debugger", debuggerTarget);

            var fileTarget = new FileTarget();
            config.AddTarget("file", fileTarget);

            // Step 3. Set target properties 
            debuggerTarget.Layout = @"${logger:shortName=True} - ${uppercase:${level}}: ${message}";
            fileTarget.FileName = "${specialfolder:folder=MyDocuments}/Artemis/logs/${shortdate}.txt";
            fileTarget.Layout = "${longdate}|${level:uppercase=true}|${logger}|${message} ${exception:format=tostring}";
            fileTarget.ArchiveEvery = FileArchivePeriod.Day;
            fileTarget.MaxArchiveFiles = 7;

            // Step 4. Define rules
            var rule1 = new LoggingRule("*", logLevel, debuggerTarget);
            config.LoggingRules.Add(rule1);
            var rule2 = new LoggingRule("*", logLevel, fileTarget);
            config.LoggingRules.Add(rule2);

            // Step 5. Activate the configuration
            LogManager.Configuration = config;

            // Log as fatal so it always shows
            var logger = LogManager.GetCurrentClassLogger();
            logger.Fatal("INFO: Set log level to {0}", logLevel);
        }
    }
}