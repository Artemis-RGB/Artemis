using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Core.Tests.Infrastructure.Services;

namespace Artemis.Core.Tests.Services;

public class ModuleServiceTests : ProfileServiceTestBase
{
    private readonly List<Module> _activated = [];
    private readonly List<Module> _deactivated = [];

    private IModuleService Modules
    {
        get
        {
            IModuleService moduleService = ResolveModuleService();
            moduleService.ModuleActivated -= OnModuleActivated;
            moduleService.ModuleActivated += OnModuleActivated;
            moduleService.ModuleDeactivated -= OnModuleDeactivated;
            moduleService.ModuleDeactivated += OnModuleDeactivated;
            return moduleService;
        }
    }

    [Fact]
    public void UpdateModuleActivation_ActivatesModulesWithoutRequirementsQuietly()
    {
        TestModule module = EnableFeature(new TestModule());

        Modules.UpdateModuleActivation();

        module.IsActivated.ShouldBeTrue();
        module.IsActivatedOverride.ShouldBeFalse();
        // Unlike modules with requirements, no ModuleActivated event is raised for them
        _activated.ShouldBeEmpty();
    }

    [Fact]
    public void UpdateModuleActivation_FollowsTheRequirements()
    {
        BooleanActivationRequirement requirement = new();
        TestModule module = EnableFeature(new TestModule {Requirements = [requirement]});

        Modules.UpdateModuleActivation();
        module.IsActivated.ShouldBeFalse();

        requirement.ActivationMet = true;
        Modules.UpdateModuleActivation();
        module.IsActivated.ShouldBeTrue();
        _activated.ShouldBe([module]);

        requirement.ActivationMet = false;
        Modules.UpdateModuleActivation();
        module.IsActivated.ShouldBeFalse();
        _deactivated.ShouldBe([module]);
    }

    [Theory]
    [InlineData(ActivationRequirementType.Any, true, false, true)]
    [InlineData(ActivationRequirementType.Any, false, false, false)]
    [InlineData(ActivationRequirementType.All, true, false, false)]
    [InlineData(ActivationRequirementType.All, true, true, true)]
    public void UpdateModuleActivation_CombinesRequirementsByTheirMode(ActivationRequirementType mode, bool first, bool second, bool activated)
    {
        TestModule module = EnableFeature(new TestModule
        {
            Requirements = [new BooleanActivationRequirement {ActivationMet = first}, new BooleanActivationRequirement {ActivationMet = second}],
            ActivationRequirementMode = mode
        });

        Modules.UpdateModuleActivation();

        module.IsActivated.ShouldBe(activated);
    }

    [Fact]
    public void ModulesEnabledLater_ArePickedUp()
    {
        IModuleService modules = Modules;

        TestModule module = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement {ActivationMet = true}]});
        modules.UpdateModuleActivation();

        module.IsActivated.ShouldBeTrue();
    }

    [Fact]
    public void DisablingAModule_DeactivatesItAndStopsUpdatingIt()
    {
        BooleanActivationRequirement requirement = new() {ActivationMet = true};
        TestModule module = EnableFeature(new TestModule {Requirements = [requirement]});
        Modules.UpdateModuleActivation();

        DisableFeature(module);
        Modules.UpdateModuleActivation();
        Modules.UpdateActiveModules(1);

        module.IsActivated.ShouldBeFalse();
        module.UpdatedFor.ShouldBe(0);
    }

    [Fact]
    public void UpdateActiveModules_OnlyUpdatesActivatedModules()
    {
        TestModule active = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement {ActivationMet = true}]});
        TestModule inactive = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement {ActivationMet = false}]});
        Modules.UpdateModuleActivation();

        Modules.UpdateActiveModules(0.5);
        Modules.UpdateActiveModules(0.25);

        active.UpdatedFor.ShouldBe(0.75);
        inactive.UpdatedFor.ShouldBe(0);
    }

    [Fact]
    public void SetActivationOverride_ActivatesAModuleWhoseRequirementsAreNotMet()
    {
        TestModule module = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement()]});

        // The profile editor does this to preview a profile of a module that isn't running
        Modules.SetActivationOverride(module);

        module.IsActivated.ShouldBeTrue();
        module.IsActivatedOverride.ShouldBeTrue();
        _activated.ShouldBeEmpty();

        // The regular activation update leaves it alone
        Modules.UpdateModuleActivation();
        module.IsActivated.ShouldBeTrue();

        Modules.SetActivationOverride(null);
        module.IsActivated.ShouldBeFalse();
    }

    [Fact]
    public void SetActivationOverride_ModulesActivatedByAnOverride_DoNotUpdate()
    {
        TestModule module = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement()]});
        Modules.SetActivationOverride(module);

        Modules.UpdateActiveModules(1);

        module.IsUpdateAllowed.ShouldBeFalse();
        module.UpdatedFor.ShouldBe(0);
    }

    [Fact]
    public void SetActivationOverride_OfAModuleWhoseRequirementsAreMet_IsARegularActivation()
    {
        TestModule module = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement {ActivationMet = true}]});

        Modules.SetActivationOverride(module);
        module.IsActivated.ShouldBeTrue();
        module.IsActivatedOverride.ShouldBeFalse();

        // Clearing the override leaves it active since it should be active anyway
        Modules.SetActivationOverride(null);
        module.IsActivated.ShouldBeTrue();
    }

    [Fact]
    public void SetActivationOverride_ToAnotherModule_DeactivatesThePreviousOne()
    {
        TestModule first = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement()]});
        TestModule second = EnableFeature(new TestModule {Requirements = [new BooleanActivationRequirement()]});

        Modules.SetActivationOverride(first);
        Modules.SetActivationOverride(second);

        first.IsActivated.ShouldBeFalse();
        second.IsActivated.ShouldBeTrue();
        second.IsActivatedOverride.ShouldBeTrue();
    }

    [Fact]
    public async Task DefaultProfiles_OfModulesEnabledLater_AreImported()
    {
        string profilePath = await ExportSavedProfile("Default profile");
        IModuleService _ = Modules;

        EnableFeature(new DefaultProfileTestModule(DefaultCategoryName.Games, profilePath));

        ProfileConfiguration imported = await WaitForProfile("Games", "Default profile");
        imported.Entity.Profile.IsFreshImport.ShouldBeTrue();
    }

    [Fact]
    public async Task DefaultProfiles_OfModulesEnabledBeforeTheServiceStarts_AreImported()
    {
        string profilePath = await ExportSavedProfile("Default profile");
        EnableFeature(new DefaultProfileTestModule(DefaultCategoryName.Applications, profilePath));

        IModuleService _ = Modules;

        await WaitForProfile("Applications", "Default profile");
    }

    [Fact]
    public async Task DefaultProfiles_RecreateTheirCategoryWhenItWasDeleted()
    {
        string profilePath = await ExportSavedProfile("Default profile");
        ProfileService.DeleteProfileCategory(ProfileService.ProfileCategories.Single(c => c.Name == "Games"));
        IModuleService _ = Modules;

        EnableFeature(new DefaultProfileTestModule(DefaultCategoryName.Games, profilePath));

        await WaitForProfile("Games", "Default profile");
    }

    [Fact]
    public async Task DefaultProfiles_AreNotImportedTwice()
    {
        string profilePath = await ExportSavedProfile("Default profile");
        IModuleService _ = Modules;
        DefaultProfileTestModule module = EnableFeature(new DefaultProfileTestModule(DefaultCategoryName.Games, profilePath));
        await WaitForProfile("Games", "Default profile");

        // Enabling the module again imports the profile again, which finds the existing profile
        DisableFeature(module);
        PluginManagement.EnablePluginFeature(module, false, false);
        await Task.Delay(500, TestContext.Current.CancellationToken);

        ProfileService.ProfileCategories.Single(c => c.Name == "Games").ProfileConfigurations.Count.ShouldBe(1);
    }

    [Fact]
    public void DefaultProfiles_ThatDoNotExist_FailWhenTheModuleIsEnabled()
    {
        string missing = Path.Combine(Services.DataFolder, "missing.zip");

        DefaultProfileTestModule module = new(DefaultCategoryName.Games, missing);

        // Features are enabled on a background task, so the error arrives wrapped
        AggregateException exception = Should.Throw<AggregateException>(() => EnableFeature(module));
        exception.InnerException.ShouldBeOfType<ArtemisPluginFeatureException>();
        module.IsEnabled.ShouldBeFalse();
    }

    /// <summary>
    ///     Exports a profile into a file in the temporary data folder and removes the profile, the way a module ships one
    /// </summary>
    private async Task<string> ExportSavedProfile(string name)
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, name, "TestTube");
        // Saving the profile stamps it with the latest migration version, see ProfileImportExportTests
        ProfileService.SaveProfile(ProfileService.ActivateProfile(configuration), true);
        string path = Path.Combine(Services.DataFolder, $"{name}.zip");
        await using (Stream export = await ProfileService.ExportProfile(configuration))
        await using (FileStream file = File.Create(path))
        {
            await export.CopyToAsync(file);
        }

        ProfileService.RemoveProfileConfiguration(configuration);
        return path;
    }

    /// <summary>
    ///     Default profiles are imported in the background, waits for the profile to show up
    /// </summary>
    private async Task<ProfileConfiguration> WaitForProfile(string categoryName, string profileName)
    {
        for (int i = 0; i < 1000; i++)
        {
            ProfileConfiguration? profile = ProfileService.ProfileCategories.FirstOrDefault(c => c.Name == categoryName)?.ProfileConfigurations.FirstOrDefault(p => p.Name == profileName);
            if (profile != null)
                return profile;
            await Task.Delay(5, TestContext.Current.CancellationToken);
        }

        throw new TimeoutException($"Profile {profileName} was not imported into {categoryName}");
    }

    private void OnModuleActivated(object? sender, ModuleEventArgs e)
    {
        _activated.Add(e.Module);
    }

    private void OnModuleDeactivated(object? sender, ModuleEventArgs e)
    {
        _deactivated.Add(e.Module);
    }
}
