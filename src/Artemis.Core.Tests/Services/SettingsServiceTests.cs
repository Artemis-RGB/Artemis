using System.ComponentModel;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.Tests.Services;

public class SettingsServiceTests : IDisposable
{
    private readonly ServiceHarness _services = new();

    private ISettingsService Settings => _services.Resolve<ISettingsService>();

    /// <summary>
    ///     A settings service on the same database, like starting Artemis again
    /// </summary>
    private SettingsService CreateNewSettingsService()
    {
        return new SettingsService(_services.Resolve<IPluginRepository>());
    }

    [Fact]
    public void GetSetting_ANewSetting_HasTheDefaultValue()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 42);

        setting.Name.ShouldBe("Tests.Number");
        setting.Value.ShouldBe(42);
        setting.HasChanged.ShouldBeFalse();
    }

    [Fact]
    public void GetSetting_WithoutADefault_UsesTheDefaultOfTheType()
    {
        Settings.GetSetting<int>("Tests.Number").Value.ShouldBe(0);
        Settings.GetSetting<string>("Tests.Text").Value.ShouldBeNull();
    }

    [Fact]
    public void GetSetting_ANewSetting_IsStoredRightAway()
    {
        Settings.GetSetting("Tests.Number", 42);

        // Without saving, the default value is already in the database
        CreateNewSettingsService().GetSetting("Tests.Number", 1).Value.ShouldBe(42);
    }

    [Fact]
    public void GetSetting_ASecondTime_ReturnsTheSameSetting()
    {
        PluginSetting<int> first = Settings.GetSetting("Tests.Number", 1);
        PluginSetting<int> second = Settings.GetSetting("Tests.Number", 2);

        second.ShouldBeSameAs(first);
        second.Value.ShouldBe(1);
    }

    [Fact]
    public void GetSetting_ASecondTimeWithAnotherType_Throws()
    {
        Settings.GetSetting("Tests.Number", 1);

        Should.Throw<InvalidCastException>(() => Settings.GetSetting("Tests.Number", "text"));
    }

    [Fact]
    public void GetSetting_AStoredSetting_IgnoresTheDefaultValue()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);
        setting.Value = 5;
        setting.Save();

        CreateNewSettingsService().GetSetting("Tests.Number", 1).Value.ShouldBe(5);
    }

    [Fact]
    public void GetSetting_AStoredValueThatDoesNotFitTheType_UsesTheDefaultValuePassedIn()
    {
        Settings.GetSetting("Tests.Value", "not a number");

        PluginSetting<int> setting = CreateNewSettingsService().GetSetting("Tests.Value", 7);

        setting.Value.ShouldBe(7);
        setting.HasChanged.ShouldBeTrue();
    }

    [Fact]
    public void GetSetting_ANameLongerThan128Characters_Throws()
    {
        Should.Throw<ArtemisCoreException>(() => Settings.GetSetting(new string('a', 129), 1));
        Should.NotThrow(() => Settings.GetSetting(new string('a', 128), 1));
    }

    [Fact]
    public void ChangingTheValue_RaisesSettingChangedAndPropertyChanged()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);
        int settingChanged = 0;
        List<string?> propertiesChanged = [];
        setting.SettingChanged += (_, _) => settingChanged++;
        ((INotifyPropertyChanged) setting).PropertyChanged += (_, e) => propertiesChanged.Add(e.PropertyName);

        setting.Value = 2;

        settingChanged.ShouldBe(1);
        propertiesChanged.ShouldBe(["Value"]);
        setting.HasChanged.ShouldBeTrue();
    }

    [Fact]
    public void SettingTheSameValue_RaisesNothing()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);
        int settingChanged = 0;
        setting.SettingChanged += (_, _) => settingChanged++;

        setting.Value = 1;

        settingChanged.ShouldBe(0);
    }

    [Fact]
    public void ChangingTheValue_IsNotStoredUntilSaved()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);

        setting.Value = 2;

        CreateNewSettingsService().GetSetting("Tests.Number", 1).Value.ShouldBe(1);
    }

    [Fact]
    public void Save_StoresTheValueAndRaisesSettingSaved()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);
        int saved = 0;
        setting.SettingSaved += (_, _) => saved++;

        setting.Value = 2;
        setting.Save();

        saved.ShouldBe(1);
        setting.HasChanged.ShouldBeFalse();
        CreateNewSettingsService().GetSetting("Tests.Number", 1).Value.ShouldBe(2);
    }

    [Fact]
    public void Save_WithoutChanges_DoesNothing()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);
        int saved = 0;
        setting.SettingSaved += (_, _) => saved++;

        setting.Save();

        saved.ShouldBe(0);
    }

    [Fact]
    public void Save_AfterChangingAListInPlace_StoresTheChange()
    {
        PluginSetting<List<int>> setting = Settings.GetSetting("Tests.Numbers", new List<int> {1});

        // Changes are detected by comparing the stored JSON, so changing the list itself counts as a change
        setting.Value!.Add(2);
        setting.HasChanged.ShouldBeTrue();
        setting.Save();

        CreateNewSettingsService().GetSetting<List<int>>("Tests.Numbers").Value.ShouldBe([1, 2]);
    }

    [Fact]
    public void AutoSave_StoresEveryChange()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);
        int saved = 0;
        setting.SettingSaved += (_, _) => saved++;
        setting.AutoSave = true;

        setting.Value = 2;
        setting.Value = 3;

        saved.ShouldBe(2);
        setting.HasChanged.ShouldBeFalse();
        CreateNewSettingsService().GetSetting("Tests.Number", 1).Value.ShouldBe(3);
    }

    [Fact]
    public void RejectChanges_GoesBackToTheLastSavedValue()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);
        setting.Value = 2;
        setting.Save();
        setting.Value = 3;
        int settingChanged = 0;
        setting.SettingChanged += (_, _) => settingChanged++;

        setting.RejectChanges();

        setting.Value.ShouldBe(2);
        setting.HasChanged.ShouldBeFalse();
        settingChanged.ShouldBe(1);
    }

    [Fact]
    public void SaveAllSettings_SavesEveryChangedSetting()
    {
        PluginSetting<int> number = Settings.GetSetting("Tests.Number", 1);
        PluginSetting<string> text = Settings.GetSetting("Tests.Text", "a");
        PluginSetting<bool> unchanged = Settings.GetSetting("Tests.Boolean", true);
        int unchangedSaved = 0;
        unchanged.SettingSaved += (_, _) => unchangedSaved++;
        number.Value = 2;
        text.Value = "b";

        Settings.SaveAllSettings();

        SettingsService reloaded = CreateNewSettingsService();
        reloaded.GetSetting("Tests.Number", 1).Value.ShouldBe(2);
        reloaded.GetSetting("Tests.Text", "a").Value.ShouldBe("b");
        unchangedSaved.ShouldBe(0);
    }

    [Fact]
    public void ToString_DescribesTheSetting()
    {
        PluginSetting<int> setting = Settings.GetSetting("Tests.Number", 1);
        setting.Value = 2;

        setting.ToString().ShouldBe("Name: Tests.Number, Value: 2, HasChanged: True");
    }

    [Fact]
    public void PluginSettings_AreKeptApartPerPlugin()
    {
        using TestPlugin first = new();
        using TestPlugin second = new();
        IPluginRepository repository = _services.Resolve<IPluginRepository>();
        PluginSettings firstSettings = new(first.Plugin, repository);
        PluginSettings secondSettings = new(second.Plugin, repository);

        PluginSetting<int> setting = firstSettings.GetSetting("Number", 1);
        setting.Value = 5;
        setting.Save();

        secondSettings.GetSetting("Number", 1).Value.ShouldBe(1);
        new PluginSettings(first.Plugin, repository).GetSetting("Number", 1).Value.ShouldBe(5);
        // Core settings belong to their own plugin
        Settings.GetSetting("Number", 1).Value.ShouldBe(1);
    }

    [Fact]
    public void PluginSettings_BelongToThePlugin()
    {
        using TestPlugin plugin = new();

        PluginSettings settings = new(plugin.Plugin, _services.Resolve<IPluginRepository>());

        settings.Plugin.ShouldBeSameAs(plugin.Plugin);
        plugin.Plugin.Settings.ShouldBeSameAs(settings);
    }

    [Fact]
    public void PluginSettings_ClearSettings_ForgetsLoadedSettings()
    {
        using TestPlugin plugin = new();
        PluginSettings settings = new(plugin.Plugin, _services.Resolve<IPluginRepository>());
        PluginSetting<int> before = settings.GetSetting("Number", 1);
        before.Value = 2;

        settings.ClearSettings();
        PluginSetting<int> after = settings.GetSetting("Number", 1);

        // A new setting is read from the database, the unsaved change is gone
        after.ShouldNotBeSameAs(before);
        after.Value.ShouldBe(1);
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
