﻿using System;
using Ninject.Activation;
using Serilog;
using Serilog.Core;
using Serilog.Events;

namespace Artemis.Core.Ninject
{
    internal class LoggerProvider : Provider<ILogger>
    {
        internal static readonly LoggingLevelSwitch LoggingLevelSwitch = new(LogEventLevel.Verbose);

        private static readonly ILogger Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .Enrich.WithDemystifiedStackTraces()
            .WriteTo.File(Constants.DataFolder + "logs/Artemis log-.log",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level:u3}] [{SourceContext}] {Message:lj}{NewLine}{Exception}")
            .WriteTo.Debug()
            .WriteTo.Sink<ArtemisSink>()
            .MinimumLevel.ControlledBy(LoggingLevelSwitch)
            .CreateLogger();

        protected override ILogger CreateInstance(IContext context)
        {
            Type? requestingType = context.Request.ParentContext?.Plan?.Type;
            if (requestingType != null)
                return Logger.ForContext(requestingType);
            return Logger;
        }
    }

    internal class ArtemisSink : ILogEventSink
    {
        /// <inheritdoc />
        public void Emit(LogEvent logEvent)
        {
            LogStore.Emit(logEvent);
        }
    }
}