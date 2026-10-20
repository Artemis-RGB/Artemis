using System.Text.Json.Nodes;
using Artemis.Core.Services;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Core.Tests.Infrastructure.Storage;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Core.Tests.Storage.Repositories;

public class ProfileRepositoryTests : IDisposable
{
    private readonly RecordingLogger _log = new();
    private readonly ServiceHarness _services = new();
    private readonly ProfileRepository _repository;

    public ProfileRepositoryTests()
    {
        _repository = new ProfileRepository(_log.Logger, _services.CreateDbContext, StoredProfiles.AllMigrations(_log.Logger));
    }

    [Fact]
    public void Artemis_RegistersTheProfileRepository()
    {
        _services.Resolve<IProfileRepository>().ShouldBeOfType<ProfileRepository>();
    }

    [Fact]
    public void Add_StoresTheProfileAndItsNewCategory()
    {
        ProfileContainerEntity container = CreateContainer(new ProfileCategoryEntity {Name = "New category"});

        _repository.Add(container);

        using var context = _services.CreateDbContext();
        ProfileContainerEntity stored = context.ProfileContainers.Include(c => c.ProfileCategory).Single();
        stored.Profile.Name.ShouldBe("Profile");
        stored.ProfileCategory.Name.ShouldBe("New category");
        // Unlike Save, Add keeps the version as it is
        stored.ProfileConfiguration.Version.ShouldBe(0);
    }

    [Fact]
    public void Add_WithACategoryThatIsAlreadyStored_Throws()
    {
        // Add also adds everything the profile refers to, so the category is added a second time. Artemis adds profiles
        // by saving their category instead, see ProfileCategoryRepositoryTests.
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);

        Should.Throw<DbUpdateException>(() => _repository.Add(CreateContainer(category)));
    }

    [Fact]
    public void Save_StoresTheProfileWithTheLatestVersion()
    {
        ProfileContainerEntity container = CreateContainer(StoredProfiles.AddCategory(_services.CreateDbContext));

        _repository.Save(container);

        container.ProfileConfiguration.Version.ShouldBe(4);
        using var context = _services.CreateDbContext();
        context.ProfileContainers.Single().ProfileConfiguration.Version.ShouldBe(4);
    }

    [Fact]
    public void Save_UpdatesAStoredProfile()
    {
        ProfileContainerEntity container = CreateContainer(StoredProfiles.AddCategory(_services.CreateDbContext));
        _repository.Save(container);

        container.Profile.Name = "Renamed";
        container.ProfileConfiguration.Name = "Renamed configuration";
        _repository.Save(container);

        using var context = _services.CreateDbContext();
        ProfileContainerEntity stored = context.ProfileContainers.Single();
        stored.Profile.Name.ShouldBe("Renamed");
        stored.ProfileConfiguration.Name.ShouldBe("Renamed configuration");
    }

    [Fact]
    public void SaveRange_StoresAllProfilesWithTheLatestVersion()
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        List<ProfileContainerEntity> containers = [CreateContainer(category, "First"), CreateContainer(category, "Second")];

        _repository.SaveRange(containers);

        using var context = _services.CreateDbContext();
        context.ProfileContainers.AsEnumerable().Select(c => (c.Profile.Name, c.ProfileConfiguration.Version)).Order()
            .ShouldBe([("First", 4), ("Second", 4)]);
    }

    [Fact]
    public void Remove_DeletesTheProfile()
    {
        ProfileContainerEntity container = CreateContainer(StoredProfiles.AddCategory(_services.CreateDbContext));
        _repository.Save(container);

        _repository.Remove(container);

        using var context = _services.CreateDbContext();
        context.ProfileContainers.ShouldBeEmpty();
        context.ProfileCategories.ShouldHaveSingleItem();
    }

    [Fact]
    public void MigrateProfiles_MigratesOldProfilesInTheDatabase()
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        Guid id = StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, StoredProfiles.Version0ConfigurationWithVersion(), OldProfileJson.Version0Profile);

        _repository.MigrateProfiles();

        (string configuration, string profile) = StoredProfiles.ReadRaw(_services.CreateDbContext, id);
        JsonNode.Parse(configuration).ShouldBeJson(OldProfileJson.MigratedConfiguration);
        JsonNode.Parse(profile).ShouldBeJson(OldProfileJson.MigratedProfile);
        _log.Errors.ShouldBeEmpty();

        // Entity Framework can read it now
        using var context = _services.CreateDbContext();
        ProfileContainerEntity stored = context.ProfileContainers.Single();
        stored.Profile.Layers.ShouldHaveSingleItem().AdaptionHints.Count.ShouldBe(2);
    }

    [Fact]
    public void MigrateProfiles_LeavesProfilesAtTheLatestVersionAlone()
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        // Indented JSON, migrating would write it again in a different layout
        Guid id = StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, OldProfileJson.MigratedConfiguration, OldProfileJson.MigratedProfile);

        _repository.MigrateProfiles();

        StoredProfiles.ReadRaw(_services.CreateDbContext, id).ShouldBe((OldProfileJson.MigratedConfiguration, OldProfileJson.MigratedProfile));
    }

    [Fact]
    public void MigrateProfiles_ProfileWithoutVersion_IsMigratedFromVersion0()
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        Guid id = StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, OldProfileJson.Version0Configuration, OldProfileJson.Version0Profile);

        _repository.MigrateProfiles();

        // Like MigrateProfile does, a missing version counts as version 0
        (string configuration, string profile) = StoredProfiles.ReadRaw(_services.CreateDbContext, id);
        JsonNode.Parse(configuration)!["Version"]!.GetValue<int>().ShouldBeGreaterThan(0);
        profile.ShouldNotBe(OldProfileJson.Version0Profile);
    }

    [Theory]
    [InlineData("{broken", "Failed to migrate profile container {Id}")]
    [InlineData("null", "Failed to parse profile or profile configuration of profile container {Id}")]
    [InlineData("""{"Folders": 5}""", "Failed to migrate profile container {Id}")]
    public void MigrateProfiles_ProfileThatCantBeMigrated_IsLoggedAndSkipped(string brokenProfile, string expectedError)
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        Guid broken = StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, StoredProfiles.Version0ConfigurationWithVersion(), brokenProfile);
        Guid working = StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, StoredProfiles.Version0ConfigurationWithVersion(), OldProfileJson.Version0Profile);

        _repository.MigrateProfiles();

        _log.Errors.ShouldHaveSingleItem().MessageTemplate.Text.ShouldBe(expectedError);
        StoredProfiles.ReadRaw(_services.CreateDbContext, broken).Profile.ShouldBe(brokenProfile);
        JsonNode.Parse(StoredProfiles.ReadRaw(_services.CreateDbContext, working).Profile).ShouldBeJson(OldProfileJson.MigratedProfile);
    }

    [Fact]
    public void MigrateProfiles_ConfigurationThatIsNotJson_IsLoggedAndSkipped()
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, "{broken", OldProfileJson.Version0Profile);
        Guid working = StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, StoredProfiles.Version0ConfigurationWithVersion(), OldProfileJson.Version0Profile);

        _repository.MigrateProfiles();

        _log.Errors.ShouldHaveSingleItem().MessageTemplate.Text.ShouldBe("Failed to migrate profile container {Id}");
        StoredProfiles.ReadRaw(_services.CreateDbContext, working).Profile.ShouldNotBe(OldProfileJson.Version0Profile);
    }

    [Fact]
    public void MigrateProfiles_ProfileCreatedInArtemis_IsStoredAtTheCurrentVersion()
    {
        IProfileService profileService = _services.Resolve<IProfileService>();
        ProfileCategory category = profileService.CreateProfileCategory("Created");
        profileService.CreateProfileConfiguration(category, "New profile", "Keyboard");

        int version;
        using (var context = _services.CreateDbContext())
            version = context.ProfileContainers.Single().ProfileConfiguration.Version;
        version.ShouldBeGreaterThan(0);

        _repository.MigrateProfiles();

        _log.Errors.ShouldBeEmpty();
        using (var context = _services.CreateDbContext())
            context.ProfileContainers.Single().ProfileConfiguration.Version.ShouldBe(version);
    }

    private static ProfileContainerEntity CreateContainer(ProfileCategoryEntity category, string name = "Profile")
    {
        Guid profileId = Guid.NewGuid();
        return new ProfileContainerEntity
        {
            ProfileCategory = category,
            ProfileConfiguration = new ProfileConfigurationEntity {Name = name, ProfileCategoryId = category.Id, ProfileId = profileId},
            Profile = new ProfileEntity {Id = profileId, Name = name}
        };
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
