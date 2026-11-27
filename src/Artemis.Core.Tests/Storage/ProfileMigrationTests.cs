using System.IO.Compression;
using System.Text;
using System.Text.Json.Nodes;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Core.Tests.Infrastructure.Storage;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Entities.Profile.AdaptionHints;
using Artemis.Storage.Entities.Profile.Conditions;
using Artemis.Storage.Migrations;
using Artemis.Storage.Repositories;
using Artemis.Storage.Repositories.Interfaces;
using DryIoc;

namespace Artemis.Core.Tests.Storage;

/// <summary>
///     Tests how <see cref="ProfileRepository.MigrateProfile" /> applies the profile migrations, with stand-in migrations
///     and with the real ones registered by Artemis.Core
/// </summary>
public class ProfileMigrationTests : IDisposable
{
    private readonly RecordingLogger _log = new();
    private readonly ServiceHarness _services = new();
    private readonly List<string> _calls = [];

    [Fact]
    public void MigrateProfile_RunsMigrationsInVersionOrder()
    {
        ProfileRepository repository = CreateRepository(3, 1, 2);
        JsonObject configuration = new() {["Version"] = 0};

        repository.MigrateProfile(configuration, new JsonObject());

        _calls.ShouldBe(["1 at version 0", "2 at version 1", "3 at version 2"]);
        configuration["Version"]!.GetValue<int>().ShouldBe(3);
    }

    [Theory]
    [InlineData(1, new[] {"2 at version 1", "3 at version 2"})]
    [InlineData(2, new[] {"3 at version 2"})]
    [InlineData(3, new string[0])]
    [InlineData(10, new string[0])]
    public void MigrateProfile_SkipsMigrationsTheProfileAlreadyHad(int version, string[] expectedCalls)
    {
        ProfileRepository repository = CreateRepository(1, 2, 3);
        JsonObject configuration = new() {["Version"] = version};

        repository.MigrateProfile(configuration, new JsonObject());

        _calls.ShouldBe(expectedCalls);
        configuration["Version"]!.GetValue<int>().ShouldBe(Math.Max(version, 3));
    }

    [Fact]
    public void MigrateProfile_ConfigurationWithoutVersion_StartsAtVersionZero()
    {
        ProfileRepository repository = CreateRepository(1, 2);
        JsonObject configuration = new() {["Name"] = "No version"};

        repository.MigrateProfile(configuration, new JsonObject());

        _calls.ShouldBe(["1 at version 0", "2 at version 1"]);
        configuration["Version"]!.GetValue<int>().ShouldBe(2);
    }

    [Fact]
    public void MigrateProfile_MissingConfigurationOrProfile_DoesNothing()
    {
        ProfileRepository repository = CreateRepository(1);
        JsonObject configuration = new() {["Name"] = "No profile"};

        repository.MigrateProfile(configuration, null);
        repository.MigrateProfile(null, new JsonObject());

        _calls.ShouldBeEmpty();
        configuration.ShouldBeJson("""{"Name": "No profile"}""");
    }

    [Fact]
    public void MigrateProfile_MigrationThrows_StopsWithTheVersionOfTheLastMigrationThatWorked()
    {
        ProfileRepository repository = new(_log.Logger, () => throw new InvalidOperationException("No database needed"),
            [new RecordingMigration(1, _calls), new FailingMigration(2), new RecordingMigration(3, _calls)]);
        JsonObject configuration = new() {["Version"] = 0};

        Should.Throw<InvalidOperationException>(() => repository.MigrateProfile(configuration, new JsonObject()));

        _calls.ShouldBe(["1 at version 0"]);
        configuration["Version"]!.GetValue<int>().ShouldBe(1);
    }

    [Fact]
    public void Artemis_RegistersAllFourMigrations()
    {
        List<IProfileMigration> migrations = _services.Container.Resolve<IEnumerable<IProfileMigration>>().ToList();

        migrations.Select(m => m.Version).Order().ShouldBe([1, 2, 3, 4]);
    }

    [Fact]
    public void MigrateProfile_Version0Profile_EndsUpInTheCurrentFormat()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.Version0Configuration);
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);

        _services.Resolve<IProfileRepository>().MigrateProfile(configuration, profile);

        configuration.ShouldBeJson(OldProfileJson.MigratedConfiguration);
        profile.ShouldBeJson(OldProfileJson.MigratedProfile);
    }

    [Fact]
    public void MigrateProfile_Version2Profile_OnlyGetsTheLaterMigrations()
    {
        // The JSON is still in the old format but claims the nodes were already moved to the general nodes plugin
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.Version0Configuration);
        configuration["Version"] = 2;
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.Version0Profile);

        _services.Resolve<IProfileRepository>().MigrateProfile(configuration, profile);

        configuration["Version"]!.GetValue<int>().ShouldBe(4);
        profile["Folders"].ShouldBeOfType<JsonArray>();
        JsonObject node = profile.FindNode(OldProfileJson.LayerBrushNodeId);
        node["Type"]!.GetValue<string>().ShouldBe("Artemis.VisualScripting.Nodes.Color.ColorGradientNode");
        node.ContainsKey("ProviderId").ShouldBeFalse();
        node["Storage"]!.GetValue<string>().ShouldBe("""[{"Color":"#ffff0000","Position":0.0},{"Color":"#ff0000ff","Position":1.0}]""");
    }

    [Fact]
    public void MigrateProfile_CurrentProfile_IsLeftAlone()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.MigratedConfiguration);
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.MigratedProfile);

        _services.Resolve<IProfileRepository>().MigrateProfile(configuration, profile);

        configuration.ShouldBeJson(OldProfileJson.MigratedConfiguration);
        profile.ShouldBeJson(OldProfileJson.MigratedProfile);
    }

    [Fact]
    public void MigrateProfile_CurrentProfileWithoutVersion_Throws()
    {
        JsonObject configuration = OldProfileJson.Parse(OldProfileJson.MigratedConfiguration);
        configuration.Remove("Version");
        JsonObject profile = OldProfileJson.Parse(OldProfileJson.MigratedProfile);

        // Without a version the profile is taken to be from before version 1, the version was added to tell those apart.
        // M0001 fails on the current format, so a profile in the current format without a version can't be imported
        Should.Throw<InvalidOperationException>(() => _services.Resolve<IProfileRepository>().MigrateProfile(configuration, profile));
    }

    [Fact]
    public async Task ImportProfile_MigratesAnOldExport()
    {
        IProfileService profileService = _services.Resolve<IProfileService>();
        ProfileCategory category = profileService.CreateProfileCategory("Imported");
        using MemoryStream archive = CreateExport(OldProfileJson.Version0Configuration, OldProfileJson.Version0Profile);

        ProfileConfiguration imported = await profileService.ImportProfile(archive, category, true, true, null);

        imported.Name.ShouldBe("Old profile");
        using var context = _services.CreateDbContext();
        ProfileContainerEntity stored = context.ProfileContainers.Single();
        stored.ProfileConfiguration.Version.ShouldBe(4);
        stored.ProfileConfiguration.ProfileId.ShouldBe(stored.Profile.Id);
        stored.Profile.Id.ShouldNotBe(Guid.Parse(OldProfileJson.ProfileId));
        stored.Profile.IsFreshImport.ShouldBeTrue();

        FolderEntity folder = stored.Profile.Folders.ShouldHaveSingleItem();
        folder.ParentId.ShouldBe(stored.Profile.Id);
        StaticConditionEntity folderCondition = folder.DisplayCondition.ShouldBeOfType<StaticConditionEntity>();
        folderCondition.Script!.Nodes.ShouldHaveSingleItem().ProviderId.ShouldBe(OldProfileJson.GeneralNodesProviderId);

        LayerEntity layer = stored.Profile.Layers.ShouldHaveSingleItem();
        layer.DisplayCondition.ShouldBeOfType<EventConditionEntity>().EventPath!.Path.ShouldBe("Player.Died");
        layer.AdaptionHints.Select(h => h.GetType()).ShouldBe([typeof(CategoryAdaptionHintEntity), typeof(KeyboardSectionAdaptionHintEntity)]);
        layer.LayerBrush!.PropertyGroup!.PropertyGroups.ShouldHaveSingleItem().Properties.ShouldHaveSingleItem()
            .DataBinding!.NodeScript!.Nodes.ShouldHaveSingleItem().Storage
            .ShouldBe("""[{"Color":"#ffff0000","Position":0.0},{"Color":"#ff0000ff","Position":1.0}]""");
    }

    private ProfileRepository CreateRepository(params int[] versions)
    {
        // MigrateProfile doesn't use the database
        return new ProfileRepository(_log.Logger, () => throw new InvalidOperationException("No database needed"),
            versions.Select(v => (IProfileMigration) new RecordingMigration(v, _calls)).ToList());
    }

    private static MemoryStream CreateExport(string configurationJson, string profileJson)
    {
        MemoryStream stream = new();
        using (ZipArchive archive = new(stream, ZipArchiveMode.Create, true))
        {
            using (StreamWriter writer = new(archive.CreateEntry("configuration.json").Open(), Encoding.UTF8))
                writer.Write(configurationJson);
            using (StreamWriter writer = new(archive.CreateEntry("profile.json").Open(), Encoding.UTF8))
                writer.Write(profileJson);
        }

        stream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    public void Dispose()
    {
        _services.Dispose();
    }

    private sealed class RecordingMigration(int version, List<string> calls) : IProfileMigration
    {
        public int Version { get; } = version;

        public void Migrate(JsonObject configurationJson, JsonObject profileJson)
        {
            calls.Add($"{Version} at version {configurationJson["Version"]}");
        }
    }

    private sealed class FailingMigration(int version) : IProfileMigration
    {
        public int Version { get; } = version;

        public void Migrate(JsonObject configurationJson, JsonObject profileJson)
        {
            throw new InvalidOperationException("Migration failed");
        }
    }
}
