using Artemis.Core.Tests.Infrastructure.Services;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Core.Tests.Storage.Repositories;

public class ReleaseRepositoryTests : IDisposable
{
    private readonly ServiceHarness _services = new();
    private readonly IReleaseRepository _repository;

    public ReleaseRepositoryTests()
    {
        _repository = _services.Resolve<IReleaseRepository>();
    }

    [Fact]
    public void SaveVersionInstallDate_NewVersion_IsStoredWithTheCurrentTime()
    {
        DateTimeOffset before = DateTimeOffset.UtcNow;

        _repository.SaveVersionInstallDate("1.0.0").ShouldBeTrue();

        using var context = _services.CreateDbContext();
        ReleaseEntity release = context.Releases.ShouldHaveSingleItem();
        release.Version.ShouldBe("1.0.0");
        release.InstalledAt.ShouldNotBeNull().ShouldBeInRange(before, DateTimeOffset.UtcNow);
    }

    [Fact]
    public void SaveVersionInstallDate_VersionThatIsAlreadyStored_KeepsTheFirstDate()
    {
        DateTimeOffset installedAt = new(2024, 1, 1, 0, 0, 0, TimeSpan.Zero);
        AddRelease("1.0.0", installedAt);

        _repository.SaveVersionInstallDate("1.0.0").ShouldBeFalse();

        using var context = _services.CreateDbContext();
        context.Releases.ShouldHaveSingleItem().InstalledAt.ShouldBe(installedAt);
    }

    [Fact]
    public void GetPreviousInstalledVersion_NothingOrOnlyOneVersionInstalled_ReturnsNull()
    {
        _repository.GetPreviousInstalledVersion().ShouldBeNull();

        _repository.SaveVersionInstallDate("1.0.0");

        _repository.GetPreviousInstalledVersion().ShouldBeNull();
    }

    [Fact]
    public void GetPreviousInstalledVersion_ReturnsTheVersionInstalledBeforeTheLatest()
    {
        // Stored out of order, the install date decides
        AddRelease("1.1.0", new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero));
        AddRelease("1.2.0", new DateTimeOffset(2024, 3, 1, 0, 0, 0, TimeSpan.Zero));
        AddRelease("1.0.0", new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        _repository.GetPreviousInstalledVersion().ShouldNotBeNull().Version.ShouldBe("1.1.0");
    }

    [Fact]
    public void GetPreviousInstalledVersion_ComparesTheMomentNotTheTimeZone()
    {
        // 10:00 at +05:00 is 05:00 UTC, so it was installed before 08:00 UTC
        AddRelease("1.0.0", new DateTimeOffset(2024, 1, 1, 10, 0, 0, TimeSpan.FromHours(5)));
        AddRelease("1.1.0", new DateTimeOffset(2024, 1, 1, 8, 0, 0, TimeSpan.Zero));

        _repository.GetPreviousInstalledVersion()!.Version.ShouldBe("1.0.0");
    }

    [Fact]
    public void GetPreviousInstalledVersion_VersionWithoutInstallDate_CountsAsTheOldest()
    {
        AddRelease("0.9.0", null);
        AddRelease("1.0.0", new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero));

        _repository.GetPreviousInstalledVersion()!.Version.ShouldBe("0.9.0");

        AddRelease("1.1.0", new DateTimeOffset(2024, 2, 1, 0, 0, 0, TimeSpan.Zero));
        _repository.GetPreviousInstalledVersion()!.Version.ShouldBe("1.0.0");
    }

    private void AddRelease(string version, DateTimeOffset? installedAt)
    {
        using var context = _services.CreateDbContext();
        context.Releases.Add(new ReleaseEntity {Version = version, InstalledAt = installedAt});
        context.SaveChanges();
    }

    public void Dispose()
    {
        _services.Dispose();
    }
}
