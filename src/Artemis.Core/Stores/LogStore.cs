using System;
using System.Collections.Generic;
using System.Linq;
using Serilog.Events;

namespace Artemis.Core;

/// <summary>
///     A static store containing the last 500 logging events
/// </summary>
public static class LogStore
{
    private static readonly object _lock = new();
    
    private static readonly LinkedList<LogEvent> LinkedList = [];

    /// <summary>
    ///     Gets a list containing the last 500 log events.
    /// </summary>
    public static List<LogEvent> Events
    {
        get
        {
            List<LogEvent> events;
            
            lock (_lock)
                events = LinkedList.ToList();
            
            return events;
        }
    }

    /// <summary>
    ///     Occurs when a new <see cref="LogEvent" /> was received.
    /// </summary>
    public static event EventHandler<LogEventEventArgs>? EventAdded;

    internal static void Emit(LogEvent logEvent)
    {
        lock (_lock)
        {
            LinkedList.AddLast(logEvent);
            while (LinkedList.Count > 500)
                LinkedList.RemoveFirst();
        }


        OnEventAdded(new LogEventEventArgs(logEvent));
    }

    private static void OnEventAdded(LogEventEventArgs e)
    {
        EventAdded?.Invoke(null, e);
    }
}

/// <summary>
///     Contains log event related data
/// </summary>
public class LogEventEventArgs : EventArgs
{
    internal LogEventEventArgs(LogEvent logEvent)
    {
        LogEvent = logEvent;
    }

    /// <summary>
    ///     Gets the log event
    /// </summary>
    public LogEvent LogEvent { get; }
}