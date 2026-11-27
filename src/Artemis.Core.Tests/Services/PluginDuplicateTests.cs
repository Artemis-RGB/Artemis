using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Services;

namespace Artemis.Core.Tests.Services;

public class PluginDuplicateTests : ServiceTestBase
{
    [Fact]
    public void StartupSkipsDuplicateButExplicitLoadStillRejectsIt()
    {
        string path = Path.Combine(Services.DataFolder, "duplicate");
        Directory.CreateDirectory(path);
        File.WriteAllText(Path.Combine(path, "plugin.json"), $$"""
            {"Guid":"{{TestPlugin.Plugin.Guid}}","Name":"Duplicate","Version":"1.0.0","Main":"duplicate.dll"}
            """);
        DirectoryInfo directory = new(path);
        PluginManagementService service = (PluginManagementService) PluginManagement;

        Assert.Null(service.LoadPlugin(directory, skipDuplicate: true));
        Assert.Same(TestPlugin.Plugin, Assert.Single(service.GetAllPlugins()));
        Assert.Throws<ArtemisCoreException>(() => service.LoadPlugin(directory));
    }
}
