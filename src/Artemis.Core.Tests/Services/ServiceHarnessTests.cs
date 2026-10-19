using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.Tests.Services;

public class ServiceHarnessTests : IDisposable
{
    private readonly ServiceHarness _services = new();

    [Fact]
    public void Settings_AreStoredInTheTemporaryDatabase()
    {
        PluginSetting<int> setting = _services.Resolve<ISettingsService>().GetSetting("Tests.Number", 1);
        setting.Value = 5;
        setting.Save();

        // A fresh settings instance reads the value back from the database
        SettingsService reloaded = new(_services.Resolve<IPluginRepository>());
        reloaded.GetSetting("Tests.Number", 1).Value.ShouldBe(5);
        using var context = _services.CreateDbContext();
        context.PluginSettings.ShouldContain(s => s.Name == "Tests.Number");
    }

    [Fact]
    public void ProfileService_StoresCategoriesInTheTemporaryDatabase()
    {
        IProfileService profileService = _services.Resolve<IProfileService>();

        profileService.CreateProfileCategory("Test category");

        using var context = _services.CreateDbContext();
        context.ProfileCategories.ShouldContain(c => c.Name == "Test category");
    }

    [Fact]
    public void Harness_DoesNotTouchTheRealDataFolder()
    {
        _services.DataFolder.ShouldNotStartWith(Constants.DataFolder);
        _services.Resolve<ISettingsService>().GetSetting("Tests.Number", 1).Save();

        File.Exists(Path.Combine(_services.DataFolder, "artemis.db")).ShouldBeTrue();
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
