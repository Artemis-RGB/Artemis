using System;

namespace Artemis.Core;

/// <summary>
///     An exception thrown when a plugin feature-related error occurs
/// </summary>
public class ArtemisPluginFeatureException : Exception
{
    internal ArtemisPluginFeatureException(PluginFeature pluginFeature)
    {
        PluginFeature = pluginFeature;
    }

    internal ArtemisPluginFeatureException(PluginFeature pluginFeature, string message) : base(message)
    {
        PluginFeature = pluginFeature;
    }

    internal ArtemisPluginFeatureException(PluginFeature pluginFeature, string message, Exception inner) : base(message, inner)
    {
        PluginFeature = pluginFeature;
    }

    /// <summary>
    ///     Gets the plugin feature the error is related to
    /// </summary>
    public PluginFeature PluginFeature { get; }
}