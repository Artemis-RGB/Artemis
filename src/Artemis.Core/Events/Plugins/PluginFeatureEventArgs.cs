using System;

namespace Artemis.Core;

/// <summary>
///     Provides data about plugin feature related events
/// </summary>
public class PluginFeatureEventArgs : EventArgs
{
    internal PluginFeatureEventArgs(PluginFeature pluginFeature)
    {
        PluginFeature = pluginFeature;
    }

    /// <summary>
    ///     Gets the plugin feature this event is related to
    /// </summary>
    public PluginFeature PluginFeature { get; }
}

/// <summary>
///     Provides data about plugin feature info related events
/// </summary>
public class PluginFeatureInfoEventArgs : EventArgs
{
    internal PluginFeatureInfoEventArgs(PluginFeatureInfo pluginFeatureInfo)
    {
        PluginFeatureInfo = pluginFeatureInfo;
    }

    /// <summary>
    ///     Gets the plugin feature this event is related to
    /// </summary>
    public PluginFeatureInfo PluginFeatureInfo { get; }
}