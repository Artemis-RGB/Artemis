using System.Collections.Generic;
using System.Linq;

namespace Artemis.Core
{
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
        ///     Determines whether the prerequisites of this plugin are met
        /// </summary>
        bool ArePrerequisitesMet();
    }
}