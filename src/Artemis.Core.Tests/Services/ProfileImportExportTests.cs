using System.IO.Compression;
using System.Text;
using Artemis.Core.LayerEffects;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerBrushes;
using Artemis.Core.Tests.Infrastructure.Plugins.LayerEffects;
using Artemis.Core.Tests.Infrastructure.Plugins.Modules;
using Artemis.Core.Tests.Infrastructure.Plugins.Nodes;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Core.Tests.Infrastructure.VisualScripting;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.Tests.Services;

/// <summary>
///     Exporting profiles to a file and importing them again through the profile service
/// </summary>
public class ProfileImportExportTests : ProfileServiceTestBase
{
    [Fact]
    public async Task ExportProfile_CreatesAnArchiveWithTheConfigurationAndProfile()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");

        await using Stream export = await ProfileService.ExportProfile(configuration);

        using ZipArchive archive = new(export, ZipArchiveMode.Read);
        archive.Entries.Select(e => e.Name).ShouldBe(["configuration.json", "profile.json"]);
        ReadEntry(archive, "configuration.json").ShouldContain("\"Test profile\"");
    }

    [Fact]
    public async Task ExportProfile_WithAnImageAsIcon_IncludesTheIcon()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        configuration.Icon.SetIconByStream(new MemoryStream([1, 2, 3]));

        await using Stream export = await ProfileService.ExportProfile(configuration);

        using ZipArchive archive = new(export, ZipArchiveMode.Read);
        archive.Entries.Select(e => e.Name).ShouldBe(["configuration.json", "profile.json", "icon.png"]);
    }

    [Fact]
    public async Task ExportProfile_ExportsWhatWasLastSaved()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Saved name", "TestTube");
        configuration.Name = "Unsaved name";

        await using Stream export = await ProfileService.ExportProfile(configuration);

        using ZipArchive archive = new(export, ZipArchiveMode.Read);
        ReadEntry(archive, "configuration.json").ShouldContain("\"Saved name\"");
    }

    [Fact]
    public async Task ImportProfile_MadeUnique_AddsACopyWithANewId()
    {
        ProfileConfiguration original = CreateSavedProfile("Test profile");
        ProfileCategory games = ProfileService.ProfileCategories.Single(c => c.Name == "Games");

        ProfileConfiguration imported = await ImportExport(original, games, true);

        imported.ShouldNotBeSameAs(original);
        imported.ProfileId.ShouldNotBe(original.ProfileId);
        imported.Name.ShouldBe("Test profile");
        imported.Category.ShouldBeSameAs(games);
        games.ProfileConfigurations.ShouldBe([imported]);
        imported.Profile.ShouldBeNull();

        ProfileConfiguration stored = CreateNewProfileService().ProfileCategories.Single(c => c.Name == "Games").ProfileConfigurations.Single();
        stored.ProfileId.ShouldBe(imported.ProfileId);
    }

    [Fact]
    public async Task ImportProfile_MadeUnique_KeepsTheContentsOfTheProfile()
    {
        TestLayerBrushProvider brushes = EnableFeature(new TestLayerBrushProvider());
        ProfileConfiguration original = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        Profile profile = ProfileService.ActivateProfile(original);
        AddLayer<SolidColorTestBrush>(profile, brushes, [], "Exported layer");
        ProfileService.SaveProfile(profile, true);

        ProfileConfiguration imported = await ImportExport(original, General, true);
        Profile importedProfile = ProfileService.ActivateProfile(imported);

        Layer layer = importedProfile.GetAllLayers().Single();
        layer.Name.ShouldBe("Exported layer");
        layer.LayerBrush.ShouldBeOfType<SolidColorTestBrush>();
        // The root folder moved along with the new ID
        importedProfile.GetRootFolder().ShouldNotBeNull();
    }

    [Fact]
    public async Task ImportProfile_WithANameAffix_AddsItToTheName()
    {
        ProfileConfiguration original = CreateSavedProfile("Test profile");

        // Like the editor's duplicate option
        ProfileConfiguration imported = await ImportExport(original, General, true, nameAffix: "copy");

        imported.Name.ShouldBe("Test profile - copy");
        imported.Entity.Profile.Name.ShouldBe("Test profile - copy");
    }

    [Fact]
    public async Task ImportProfile_ByDefault_GoesAtTheBottomOfTheCategory()
    {
        ProfileConfiguration original = CreateSavedProfile("First");
        CreateSavedProfile("Second");

        ProfileConfiguration imported = await ImportExport(original, General, true);

        General.ProfileConfigurations.Last().ShouldBeSameAs(imported);
        imported.Order.ShouldBe(2);
    }

    [Fact]
    public async Task ImportProfile_WithATarget_GoesInFrontOfTheTarget()
    {
        ProfileConfiguration original = CreateSavedProfile("First");
        ProfileConfiguration second = CreateSavedProfile("Second");
        // Listed as second, first

        ProfileConfiguration imported = await ImportExport(original, General, true, target: original);

        General.ProfileConfigurations.ShouldBe([second, imported, original]);
    }

    [Fact]
    public async Task ImportProfile_DoesNotRaiseProfileAdded()
    {
        ProfileConfiguration original = CreateSavedProfile("Test profile");
        int profileAdded = 0;
        int addedToCategory = 0;
        ProfileService.ProfileAdded += (_, _) => profileAdded++;
        General.ProfileConfigurationAdded += (_, _) => addedToCategory++;

        await ImportExport(original, General, true);

        // Pinned as-is: creating a profile raises ProfileAdded but importing one doesn't, only the category's own
        // ProfileConfigurationAdded event tells listeners about an imported profile
        profileAdded.ShouldBe(0);
        addedToCategory.ShouldBe(1);
    }

    [Fact]
    public async Task ImportProfile_NotMadeUnique_ReturnsTheExistingProfile()
    {
        ProfileConfiguration original = CreateSavedProfile("Test profile");

        ProfileConfiguration imported = await ImportExport(original, General, false);

        imported.ShouldBeSameAs(original);
        General.ProfileConfigurations.ShouldBe([original]);
    }

    [Fact]
    public async Task ImportProfile_NotMadeUnique_KeepsTheIdOfANewProfile()
    {
        ProfileConfiguration original = CreateSavedProfile("Test profile");
        await using Stream export = await ProfileService.ExportProfile(original);
        ProfileService.RemoveProfileConfiguration(original);

        ProfileConfiguration imported = await ProfileService.ImportProfile(export, General, false, false, null);

        imported.ProfileId.ShouldBe(original.ProfileId);
    }

    [Fact]
    public async Task ImportProfile_MarkedAsFreshImport_StaysFreshUntilSaved()
    {
        ProfileConfiguration original = CreateSavedProfile("Test profile");

        ProfileConfiguration imported = await ImportExport(original, General, true, markAsFreshImport: true);
        Profile profile = ProfileService.ActivateProfile(imported);

        // A fresh import is adapted to the devices when it is activated
        profile.IsFreshImport.ShouldBeTrue();
        ProfileService.SaveProfile(profile, true);
        profile.IsFreshImport.ShouldBeFalse();
        CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations
            .Single(c => c.ProfileId == imported.ProfileId).Entity.Profile.IsFreshImport.ShouldBeFalse();
    }

    [Fact]
    public async Task ImportProfile_WithAnImageAsIcon_ImportsTheIcon()
    {
        ProfileConfiguration original = CreateSavedProfile("Test profile");
        original.Icon.SetIconByStream(new MemoryStream([1, 2, 3]));
        ProfileService.SaveProfileCategory(General);

        ProfileConfiguration imported = await ImportExport(original, General, true);

        imported.Icon.IconType.ShouldBe(ProfileConfigurationIconType.BitmapImage);
        imported.Icon.IconBytes.ShouldBe([1, 2, 3]);
    }

    [Fact]
    public async Task ImportProfile_WithoutAConfiguration_Throws()
    {
        using MemoryStream archive = CreateArchive(("profile.json", "{}"));

        ArtemisCoreException exception = await Should.ThrowAsync<ArtemisCoreException>(() => ProfileService.ImportProfile(archive, General, true, false, null));
        exception.Message.ShouldContain("configuration.json missing");
    }

    [Fact]
    public async Task ImportProfile_WithoutAProfile_Throws()
    {
        using MemoryStream archive = CreateArchive(("configuration.json", "{}"));

        ArtemisCoreException exception = await Should.ThrowAsync<ArtemisCoreException>(() => ProfileService.ImportProfile(archive, General, true, false, null));
        exception.Message.ShouldContain("profile.json missing");
    }

    [Fact]
    public async Task ImportProfile_WithUnavailableBrushesAndEffects_KeepsThemForWhenTheyReturn()
    {
        TestLayerBrushProvider brushes = EnableFeature(new TestLayerBrushProvider());
        TestLayerEffectProvider effects = EnableFeature(new TestLayerEffectProvider());
        ProfileConfiguration original = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        Profile profile = ProfileService.ActivateProfile(original);
        Layer layer = AddLayer<SolidColorTestBrush>(profile, brushes, []);
        AddEffect<TintTestEffect>(layer, effects);
        ProfileService.SaveProfile(profile, true);
        await using Stream export = await ProfileService.ExportProfile(original);
        DisableFeature(brushes);
        DisableFeature(effects);

        ProfileConfiguration imported = await ProfileService.ImportProfile(export, General, true, false, null);
        Profile importedProfile = ProfileService.ActivateProfile(imported);
        Layer importedLayer = importedProfile.GetAllLayers().Single();

        // Without the plugins the layer has no brush and a placeholder effect
        importedLayer.LayerBrush.ShouldBeNull();
        importedLayer.BrokenState.ShouldNotBeNull();
        importedLayer.LayerEffects.Single().Descriptor.IsPlaceholder.ShouldBeTrue();

        // Saving in the meantime doesn't lose them
        ProfileService.SaveProfile(importedProfile, true);
        EnableFeature(new TestLayerBrushProvider());
        EnableFeature(new TestLayerEffectProvider());
        ProfileService.DeactivateProfile(imported);
        Layer reloadedLayer = ProfileService.ActivateProfile(imported).GetAllLayers().Single();
        reloadedLayer.LayerBrush.ShouldBeOfType<SolidColorTestBrush>();
        reloadedLayer.LayerEffects.Single().ShouldBeOfType<TintTestEffect>();
    }

    [Fact]
    public async Task ImportProfile_WithUnavailableNodes_KeepsThemInTheActivationCondition()
    {
        TestNodeProvider nodes = EnableFeature(new TestNodeProvider());
        ProfileConfiguration original = CreateSavedProfile("Test profile");
        UseActivationCondition(original, nodes, false);
        ProfileService.SaveProfileCategory(General);
        await using Stream export = await ProfileService.ExportProfile(original);
        DisableFeature(nodes);

        ProfileConfiguration imported = await ProfileService.ImportProfile(export, General, true, false, null);

        // The node can't be loaded without its plugin, but importing saves the category right away and that keeps it, so
        // enabling the plugin afterwards brings it back like missing brushes and effects
        imported.ActivationCondition.Nodes.OfType<TestBooleanNode>().ShouldBeEmpty();
        imported.Entity.ProfileConfiguration.ActivationCondition!.Nodes.ShouldContain(n => n.Type == nameof(TestBooleanNode));
        EnableFeature(new TestNodeProvider());
        ProfileConfiguration reloaded = CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations
            .Single(c => c.ProfileId == imported.ProfileId);
        reloaded.ActivationCondition.Nodes.OfType<TestBooleanNode>().ShouldHaveSingleItem();
        reloaded.ActivationCondition.ExitNodeConnected.ShouldBeTrue();
    }

    [Fact]
    public async Task ImportProfile_WithAnUnavailableModule_IsMissingItsModuleUntilItIsEnabled()
    {
        TestModule module = EnableFeature(new TestModule());
        ProfileConfiguration original = CreateSavedProfile("Test profile");
        original.Module = module;
        ProfileService.SaveProfileCategory(General);
        await using Stream export = await ProfileService.ExportProfile(original);
        DisableFeature(module);

        ProfileConfiguration imported = await ProfileService.ImportProfile(export, General, true, false, null);

        imported.IsMissingModule.ShouldBeTrue();
        imported.Entity.ProfileConfiguration.ModuleId.ShouldBe(module.Id);
        PluginManagement.EnablePluginFeature(module, false, false);
        imported.Module.ShouldBeSameAs(module);
    }

    [Fact]
    public void ProfilesThatWereNeverSavedInTheEditor_AreStoredAsVersionZero()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        GetStoredProfiles().Single().ProfileConfiguration.Version.ShouldBe(0);

        ProfileService.SaveProfile(ProfileService.ActivateProfile(configuration), true);

        GetStoredProfiles().Single().ProfileConfiguration.Version.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void ProfilesThatWereNeverSavedInTheEditor_FailToMigrateOnStartupButStillLoad()
    {
        ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");

        // Artemis migrates stored profiles once on startup, a failure is logged and the profile is left as it is
        Resolve<IProfileRepository>().MigrateProfiles();

        GetStoredProfiles().Single().ProfileConfiguration.Version.ShouldBe(0);
        CreateNewProfileService().ProfileCategories.Single(c => c.Name == "General").ProfileConfigurations.Single().Name.ShouldBe("Test profile");
    }

    [Fact]
    public async Task ImportProfile_OfAProfileThatWasNeverSavedInTheEditor_Fails()
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, "Test profile", "TestTube");
        await using Stream export = await ProfileService.ExportProfile(configuration);

        // Pinned as-is: saving the category (which is all creating a profile does) stores the profile without setting its
        // migration version, only SaveProfile sets it. The export says version 0, so importing runs every migration on
        // JSON that is already up to date and the first one fails on it. Duplicating a freshly created profile from the
        // sidebar or the editor (an export followed by an import) runs into this. Restarting Artemis runs into it too, but
        // there the failed migration is only logged and the profile loads normally, see the test above
        InvalidOperationException exception = await Should.ThrowAsync<InvalidOperationException>(() => ProfileService.ImportProfile(export, General, true, false, "copy"));
        exception.StackTrace!.ShouldContain("M0001NodeProviders");
        General.ProfileConfigurations.ShouldBe([configuration]);
    }

    /// <summary>
    ///     Creates a profile and saves it the way the editor does, which also stamps it with the latest migration version
    /// </summary>
    private ProfileConfiguration CreateSavedProfile(string name)
    {
        ProfileConfiguration configuration = ProfileService.CreateProfileConfiguration(General, name, "TestTube");
        ProfileService.SaveProfile(ProfileService.ActivateProfile(configuration), true);
        ProfileService.DeactivateProfile(configuration);
        return configuration;
    }

    private async Task<ProfileConfiguration> ImportExport(ProfileConfiguration configuration, ProfileCategory category, bool makeUnique,
        bool markAsFreshImport = false, string? nameAffix = null, ProfileConfiguration? target = null)
    {
        await using Stream export = await ProfileService.ExportProfile(configuration);
        return await ProfileService.ImportProfile(export, category, makeUnique, markAsFreshImport, nameAffix, target);
    }

    private static string ReadEntry(ZipArchive archive, string name)
    {
        using StreamReader reader = new(archive.GetEntry(name)!.Open());
        return reader.ReadToEnd();
    }

    private static MemoryStream CreateArchive(params (string Name, string Content)[] entries)
    {
        MemoryStream stream = new();
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, true))
        {
            foreach ((string name, string content) in entries)
            {
                using Stream entryStream = archive.CreateEntry(name).Open();
                entryStream.Write(Encoding.UTF8.GetBytes(content));
            }
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }
}
