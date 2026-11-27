using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Storage.Entities.Plugins;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Core.Tests.Storage.Repositories;

public class PluginRepositoryTests : IDisposable
{
    private readonly ServiceHarness _services = new();
    private readonly IPluginRepository _repository;

    public PluginRepositoryTests()
    {
        _repository = _services.Resolve<IPluginRepository>();
    }

    [Fact]
    public void SavePlugin_NewPlugin_IsStoredWithItsFeatures()
    {
        Guid pluginGuid = Guid.NewGuid();
        PluginEntity plugin = new() {PluginGuid = pluginGuid, IsEnabled = true};
        plugin.Features.Add(new PluginFeatureEntity {Type = "Artemis.Plugins.Test.Brush", IsEnabled = true});
        plugin.Features.Add(new PluginFeatureEntity {Type = "Artemis.Plugins.Test.Module", IsEnabled = false});

        _repository.SavePlugin(plugin);

        PluginEntity stored = _repository.GetPluginByPluginGuid(pluginGuid).ShouldNotBeNull();
        stored.Id.ShouldNotBe(Guid.Empty);
        stored.IsEnabled.ShouldBeTrue();
        stored.Features.Select(f => (f.Type, f.IsEnabled)).Order()
            .ShouldBe([("Artemis.Plugins.Test.Brush", true), ("Artemis.Plugins.Test.Module", false)]);
    }

    [Fact]
    public void GetPluginByPluginGuid_UnknownPlugin_ReturnsNull()
    {
        _repository.SavePlugin(new PluginEntity {PluginGuid = Guid.NewGuid()});

        _repository.GetPluginByPluginGuid(Guid.NewGuid()).ShouldBeNull();
    }

    [Fact]
    public void SavePlugin_StoredPlugin_StoresChangesAndNewFeatures()
    {
        Guid pluginGuid = Guid.NewGuid();
        PluginEntity plugin = new() {PluginGuid = pluginGuid, IsEnabled = true};
        plugin.Features.Add(new PluginFeatureEntity {Type = "Brush", IsEnabled = true});
        _repository.SavePlugin(plugin);

        plugin.IsEnabled = false;
        plugin.Features[0].IsEnabled = false;
        plugin.Features.Add(new PluginFeatureEntity {Type = "Effect", IsEnabled = true});
        _repository.SavePlugin(plugin);

        PluginEntity stored = _repository.GetPluginByPluginGuid(pluginGuid)!;
        stored.IsEnabled.ShouldBeFalse();
        stored.Features.Select(f => (f.Type, f.IsEnabled)).Order().ShouldBe([("Brush", false), ("Effect", true)]);
    }

    [Fact]
    public void SavePlugin_FeatureRemovedFromTheList_StaysStored()
    {
        Guid pluginGuid = Guid.NewGuid();
        PluginEntity plugin = new() {PluginGuid = pluginGuid};
        plugin.Features.Add(new PluginFeatureEntity {Type = "Brush"});
        plugin.Features.Add(new PluginFeatureEntity {Type = "Effect"});
        _repository.SavePlugin(plugin);

        // Saving only adds and updates, features that are no longer in the list aren't removed
        plugin.Features.RemoveAt(0);
        _repository.SavePlugin(plugin);

        _repository.GetPluginByPluginGuid(pluginGuid)!.Features.Select(f => f.Type).Order().ShouldBe(["Brush", "Effect"]);
    }

    [Fact]
    public void SavePlugin_SecondPluginWithTheSameGuid_Throws()
    {
        Guid pluginGuid = Guid.NewGuid();
        _repository.SavePlugin(new PluginEntity {PluginGuid = pluginGuid});

        Should.Throw<DbUpdateException>(() => _repository.SavePlugin(new PluginEntity {PluginGuid = pluginGuid}));
    }

    [Fact]
    public void SaveSetting_NewSetting_CanBeFoundByNameAndPlugin()
    {
        Guid pluginGuid = Guid.NewGuid();

        _repository.SaveSetting(new PluginSettingEntity {PluginGuid = pluginGuid, Name = "Brightness", Value = "50"});

        _repository.GetSettingByNameAndGuid("Brightness", pluginGuid).ShouldNotBeNull().Value.ShouldBe("50");
        _repository.GetSettingByNameAndGuid("Brightness", Guid.NewGuid()).ShouldBeNull();
        _repository.GetSettingByNameAndGuid("Speed", pluginGuid).ShouldBeNull();
    }

    [Fact]
    public void SaveSetting_StoredSetting_StoresTheNewValue()
    {
        Guid pluginGuid = Guid.NewGuid();
        PluginSettingEntity setting = new() {PluginGuid = pluginGuid, Name = "Brightness", Value = "50"};
        _repository.SaveSetting(setting);

        setting.Value = "75";
        _repository.SaveSetting(setting);

        _repository.GetSettingByNameAndGuid("Brightness", pluginGuid)!.Value.ShouldBe("75");
        using var context = _services.CreateDbContext();
        context.PluginSettings.Count().ShouldBe(1);
    }

    [Fact]
    public void SaveSetting_SecondSettingWithTheSameNameAndPlugin_Throws()
    {
        Guid pluginGuid = Guid.NewGuid();
        _repository.SaveSetting(new PluginSettingEntity {PluginGuid = pluginGuid, Name = "Brightness", Value = "50"});

        Should.Throw<DbUpdateException>(() => _repository.SaveSetting(new PluginSettingEntity {PluginGuid = pluginGuid, Name = "Brightness", Value = "75"}));
    }

    [Fact]
    public void SaveSetting_SameNameForAnotherPlugin_IsAllowed()
    {
        Guid first = Guid.NewGuid();
        Guid second = Guid.NewGuid();

        _repository.SaveSetting(new PluginSettingEntity {PluginGuid = first, Name = "Brightness", Value = "1"});
        _repository.SaveSetting(new PluginSettingEntity {PluginGuid = second, Name = "Brightness", Value = "2"});

        _repository.GetSettingByNameAndGuid("Brightness", first)!.Value.ShouldBe("1");
        _repository.GetSettingByNameAndGuid("Brightness", second)!.Value.ShouldBe("2");
    }

    [Fact]
    public void RemoveSettings_RemovesOnlyTheSettingsOfThatPlugin()
    {
        Guid removed = Guid.NewGuid();
        Guid kept = Guid.NewGuid();
        _repository.SaveSetting(new PluginSettingEntity {PluginGuid = removed, Name = "Brightness"});
        _repository.SaveSetting(new PluginSettingEntity {PluginGuid = removed, Name = "Speed"});
        _repository.SaveSetting(new PluginSettingEntity {PluginGuid = kept, Name = "Brightness"});

        _repository.RemoveSettings(removed);

        using var context = _services.CreateDbContext();
        context.PluginSettings.ShouldHaveSingleItem().PluginGuid.ShouldBe(kept);
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
