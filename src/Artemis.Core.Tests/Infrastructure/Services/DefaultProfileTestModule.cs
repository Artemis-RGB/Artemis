using Artemis.Core.Tests.Infrastructure.Plugins.Modules;

namespace Artemis.Core.Tests.Infrastructure.Services;

/// <summary>
///     A test module that ships a default profile, like game modules do
/// </summary>
public class DefaultProfileTestModule : TestModule
{
    /// <param name="category">The category the default profile goes in</param>
    /// <param name="profilePath">The full path of an exported profile</param>
    public DefaultProfileTestModule(DefaultCategoryName category, string profilePath)
    {
        // Before the module is enabled this is remembered, it is checked and added when the module is enabled
        AddDefaultProfile(category, profilePath);
    }
}
