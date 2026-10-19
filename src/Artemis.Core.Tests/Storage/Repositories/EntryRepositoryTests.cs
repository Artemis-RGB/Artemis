using System.Text.Json.Nodes;
using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Storage.Entities.Workshop;
using Artemis.Storage.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Core.Tests.Storage.Repositories;

public class EntryRepositoryTests : IDisposable
{
    private readonly ServiceHarness _services = new();
    private readonly IEntryRepository _repository;

    public EntryRepositoryTests()
    {
        _repository = _services.Resolve<IEntryRepository>();
    }

    [Fact]
    public void Add_ThenGet_ReturnsEverythingThatWasStored()
    {
        DateTimeOffset createdAt = new(2024, 5, 1, 12, 0, 0, TimeSpan.FromHours(2));
        DateTimeOffset installedAt = new(2024, 6, 1, 8, 30, 0, TimeSpan.Zero);
        EntryEntity entry = new()
        {
            EntryId = 42,
            EntryType = 2,
            Author = "Someone",
            IsOfficial = true,
            Name = "Rainbow profile",
            Summary = "All the colors",
            Downloads = 1000,
            CreatedAt = createdAt,
            LatestReleaseId = 7,
            ReleaseId = 6,
            ReleaseVersion = "1.2.0",
            InstalledAt = installedAt,
            AutoUpdate = true,
            Metadata = new Dictionary<string, JsonNode> {["ProfileId"] = JsonValue.Create("abc"), ["Settings"] = new JsonObject {["Speed"] = 5}},
            Categories = [new EntryCategoryEntity("Games", "Gamepad")]
        };

        _repository.Add(entry);

        EntryEntity stored = _repository.Get(entry.Id).ShouldNotBeNull();
        stored.ShouldNotBeSameAs(entry);
        (stored.EntryId, stored.EntryType, stored.Author, stored.IsOfficial).ShouldBe((42L, 2, "Someone", true));
        (stored.Name, stored.Summary, stored.Downloads).ShouldBe(("Rainbow profile", "All the colors", 1000L));
        (stored.CreatedAt, stored.InstalledAt).ShouldBe((createdAt, installedAt));
        (stored.LatestReleaseId, stored.ReleaseId, stored.ReleaseVersion, stored.AutoUpdate).ShouldBe((7L, 6L, "1.2.0", true));
        stored.Metadata!["ProfileId"].GetValue<string>().ShouldBe("abc");
        stored.Metadata["Settings"]["Speed"]!.GetValue<int>().ShouldBe(5);
        stored.Categories.ShouldBe([new EntryCategoryEntity("Games", "Gamepad")]);
    }

    [Fact]
    public void Add_EntryWithoutMetadataOrCategories_ReadsBackWithout()
    {
        EntryEntity entry = new() {EntryId = 1, Metadata = null, Categories = null};

        _repository.Add(entry);

        // Null skips the JSON conversion and is stored as an empty column
        EntryEntity stored = _repository.Get(entry.Id)!;
        stored.Metadata.ShouldBeNull();
        stored.Categories.ShouldBeNull();
    }

    [Fact]
    public void Get_EntryWithNullJson_ReturnsEmptyMetadataAndCategories()
    {
        EntryEntity entry = new() {EntryId = 1};
        _repository.Add(entry);
        using (var context = _services.CreateDbContext())
            context.Database.ExecuteSqlRaw("UPDATE Entries SET Metadata = 'null', Categories = 'null'");

        EntryEntity stored = _repository.Get(entry.Id)!;

        stored.Metadata.ShouldNotBeNull().ShouldBeEmpty();
        stored.Categories.ShouldNotBeNull().ShouldBeEmpty();
    }

    [Fact]
    public void GetByEntryId_FindsTheEntryByItsWorkshopId()
    {
        _repository.Add(new EntryEntity {EntryId = 1, Name = "First"});
        _repository.Add(new EntryEntity {EntryId = 2, Name = "Second"});

        _repository.GetByEntryId(2).ShouldNotBeNull().Name.ShouldBe("Second");
        _repository.GetByEntryId(3).ShouldBeNull();
    }

    [Fact]
    public void Get_UnknownEntry_ReturnsNull()
    {
        _repository.Add(new EntryEntity {EntryId = 1});

        _repository.Get(Guid.NewGuid()).ShouldBeNull();
    }

    [Fact]
    public void Add_SecondEntryWithTheSameWorkshopId_Throws()
    {
        _repository.Add(new EntryEntity {EntryId = 1});

        Should.Throw<DbUpdateException>(() => _repository.Add(new EntryEntity {EntryId = 1}));
    }

    [Fact]
    public void GetAll_ReturnsAllEntries()
    {
        _repository.Add(new EntryEntity {EntryId = 1});
        _repository.Add(new EntryEntity {EntryId = 2});

        _repository.GetAll().Select(e => e.EntryId).Order().ShouldBe([1L, 2L]);
    }

    [Fact]
    public void Save_StoresTheChanges()
    {
        EntryEntity entry = new() {EntryId = 1, ReleaseVersion = "1.0.0"};
        _repository.Add(entry);

        entry.ReleaseVersion = "1.1.0";
        entry.Metadata = new Dictionary<string, JsonNode> {["Key"] = JsonValue.Create(true)};
        _repository.Save(entry);

        EntryEntity stored = _repository.Get(entry.Id)!;
        stored.ReleaseVersion.ShouldBe("1.1.0");
        stored.Metadata!["Key"].GetValue<bool>().ShouldBeTrue();
    }

    [Fact]
    public void Remove_DeletesTheEntry()
    {
        EntryEntity entry = new() {EntryId = 1};
        _repository.Add(entry);
        _repository.Add(new EntryEntity {EntryId = 2});

        _repository.Remove(entry);

        _repository.GetAll().ShouldHaveSingleItem().EntryId.ShouldBe(2);
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
