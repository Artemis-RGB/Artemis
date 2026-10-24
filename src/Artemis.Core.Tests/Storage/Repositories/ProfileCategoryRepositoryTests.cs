using System.Text.Json.Nodes;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Core.Tests.Infrastructure.Storage;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Core.Tests.Storage.Repositories;

public class ProfileCategoryRepositoryTests : IDisposable
{
    private readonly RecordingLogger _log = new();
    private readonly ServiceHarness _services = new();
    private readonly ProfileCategoryRepository _repository;

    public ProfileCategoryRepositoryTests()
    {
        _repository = CreateRepository();
    }

    [Fact]
    public void Artemis_RegistersTheProfileCategoryRepository()
    {
        _services.Resolve<IProfileCategoryRepository>().ShouldBeOfType<ProfileCategoryRepository>();
    }

    [Fact]
    public void Add_ThenGet_ReturnsTheCategory()
    {
        ProfileCategoryEntity category = new() {Name = "Games", IsCollapsed = true, IsSuspended = true, Order = 3};

        _repository.Add(category);

        ProfileCategoryEntity stored = _repository.Get(category.Id).ShouldNotBeNull();
        stored.ShouldNotBeSameAs(category);
        (stored.Name, stored.IsCollapsed, stored.IsSuspended, stored.Order).ShouldBe(("Games", true, true, 3));
    }

    [Fact]
    public void Add_CategoryWithANameThatIsTaken_Throws()
    {
        _repository.Add(new ProfileCategoryEntity {Name = "Games"});

        Should.Throw<DbUpdateException>(() => _repository.Add(new ProfileCategoryEntity {Name = "Games"}));
    }

    [Fact]
    public void Get_UnknownCategory_ReturnsNull()
    {
        _repository.Get(Guid.NewGuid()).ShouldBeNull();
    }

    [Fact]
    public void GetAll_ReturnsAllCategoriesWithTheirProfiles()
    {
        ProfileCategoryEntity games = new() {Name = "Games"};
        games.ProfileConfigurations.Add(CreateContainer("Racing"));
        games.ProfileConfigurations.Add(CreateContainer("Shooter"));
        _repository.Add(games);
        _repository.Add(new ProfileCategoryEntity {Name = "Empty"});

        List<ProfileCategoryEntity> categories = _repository.GetAll();

        categories.Select(c => c.Name).Order().ShouldBe(["Empty", "Games"]);
        categories.Single(c => c.Name == "Games").ProfileConfigurations.Select(p => p.Profile.Name).Order().ShouldBe(["Racing", "Shooter"]);
        categories.Single(c => c.Name == "Empty").ProfileConfigurations.ShouldBeEmpty();
    }

    [Fact]
    public void Save_StoresChangesAndNewProfiles()
    {
        ProfileCategoryEntity category = new() {Name = "Games"};
        _repository.Add(category);

        category.Name = "Renamed";
        category.Order = 2;
        category.ProfileConfigurations.Add(CreateContainer("New profile"));
        _repository.Save(category);

        ProfileCategoryEntity stored = _repository.Get(category.Id)!;
        stored.Name.ShouldBe("Renamed");
        stored.Order.ShouldBe(2);
        ProfileContainerEntity profile = stored.ProfileConfigurations.ShouldHaveSingleItem();
        profile.Profile.Name.ShouldBe("New profile");
        // Unlike ProfileRepository.Save this doesn't set the version, see ProfileRepositoryTests
        profile.ProfileConfiguration.Version.ShouldBe(0);
    }

    [Fact]
    public void SaveRange_StoresAllCategories()
    {
        ProfileCategoryEntity first = new() {Name = "First"};
        ProfileCategoryEntity second = new() {Name = "Second"};
        _repository.Add(first);
        _repository.Add(second);

        first.Order = 2;
        second.Order = 1;
        _repository.SaveRange([first, second]);

        _repository.GetAll().OrderBy(c => c.Order).Select(c => c.Name).ShouldBe(["Second", "First"]);
    }

    [Fact]
    public void Remove_DeletesTheCategoryAndItsProfiles()
    {
        ProfileCategoryEntity category = new() {Name = "Games"};
        category.ProfileConfigurations.Add(CreateContainer("Racing"));
        _repository.Add(category);

        _repository.Remove(category);

        using var context = _services.CreateDbContext();
        context.ProfileCategories.ShouldBeEmpty();
        context.ProfileContainers.ShouldBeEmpty();
    }

    [Fact]
    public void IsUnique_ReturnsWhetherTheNameIsFree()
    {
        ProfileCategoryEntity games = new() {Name = "Games"};
        _repository.Add(games);

        _repository.IsUnique("Games", null).ShouldBeFalse();
        _repository.IsUnique("Other", null).ShouldBeTrue();
    }

    [Fact]
    public void IsUnique_IgnoresTheCategoryWithTheGivenId()
    {
        ProfileCategoryEntity games = new() {Name = "Games"};
        _repository.Add(games);

        _repository.IsUnique("Games", games.Id).ShouldBeTrue();
        _repository.IsUnique("Games", Guid.NewGuid()).ShouldBeFalse();
    }

    [Fact]
    public void IsUnique_IgnoresSpacesAroundTheName()
    {
        _repository.Add(new ProfileCategoryEntity {Name = "Games"});

        _repository.IsUnique("  Games ", null).ShouldBeFalse();
        _repository.IsUnique("games", null).ShouldBeTrue();
    }

    [Fact]
    public void GetAll_MigratesOldProfilesTheFirstTime()
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, StoredProfiles.Version0ConfigurationWithVersion(), OldProfileJson.Version0Profile);

        ProfileContainerEntity profile = _repository.GetAll().Single().ProfileConfigurations.Single();

        profile.ProfileConfiguration.Version.ShouldBe(4);
        profile.Profile.Layers.ShouldHaveSingleItem().AdaptionHints.Count.ShouldBe(2);
    }

    [Fact]
    public void Get_MigratesOldProfilesTheFirstTime()
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, StoredProfiles.Version0ConfigurationWithVersion(), OldProfileJson.Version0Profile);

        _repository.Get(category.Id)!.ProfileConfigurations.Single().ProfileConfiguration.Version.ShouldBe(4);
    }

    [Fact]
    public void GetAll_MigratesOnlyOncePerRepository()
    {
        ProfileCategoryEntity category = StoredProfiles.AddCategory(_services.CreateDbContext);
        _repository.GetAll();
        Guid id = StoredProfiles.InsertRaw(_services.CreateDbContext, category.Id, StoredProfiles.Version0ConfigurationWithVersion(), OldProfileJson.Version0Profile);

        // The repository is a singleton, profiles are migrated once when Artemis starts
        _repository.Get(Guid.NewGuid());
        JsonNode.Parse(StoredProfiles.ReadRaw(_services.CreateDbContext, id).Configuration)!["Version"]!.GetValue<int>().ShouldBe(0);

        // A new repository does migrate it
        CreateRepository().GetAll().Single().ProfileConfigurations.Single().ProfileConfiguration.Version.ShouldBe(4);
    }

    private ProfileCategoryRepository CreateRepository()
    {
        return new ProfileCategoryRepository(_services.CreateDbContext, new ProfileRepository(_log.Logger, _services.CreateDbContext, StoredProfiles.AllMigrations(_log.Logger)));
    }

    private static ProfileContainerEntity CreateContainer(string name)
    {
        Guid profileId = Guid.NewGuid();
        return new ProfileContainerEntity
        {
            ProfileConfiguration = new ProfileConfigurationEntity {Name = name, ProfileId = profileId},
            Profile = new ProfileEntity {Id = profileId, Name = name}
        };
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
