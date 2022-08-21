using System.Collections.Generic;

namespace Artemis.Core;

/// <summary>
///     Represents a type that has prerequisites
/// </summary>
public interface IPrerequisitesSubject
{
    /// <summary>
    ///     Gets a list of prerequisites for this plugin
    /// </summary>
    List<PluginPrerequisite> Prerequisites { get; }

    /// <summary>
    ///     Gets a list of prerequisites of the current platform for this plugin
    /// </summary>
    IEnumerable<PluginPrerequisite> PlatformPrerequisites { get; }

    /// <summary>
    ///     Determines whether the prerequisites of this plugin are met
    /// </summary>
    bool ArePrerequisitesMet();
}