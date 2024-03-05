using System;
using System.Linq;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Storage.Repositories;

public class ReleaseRepository : IReleaseRepository
{
    private readonly ArtemisDbContext _dbContext;

    public ReleaseRepository(ArtemisDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public bool SaveVersionInstallDate(string version)
    {
        ReleaseEntity? release = _dbContext.Releases.FirstOrDefault(r => r.Version == version);
        if (release != null)
            return false;

        _dbContext.Releases.Add(new ReleaseEntity {Version = version, InstalledAt = DateTimeOffset.UtcNow});
        _dbContext.SaveChanges();
        return true;
    }

    public ReleaseEntity? GetPreviousInstalledVersion()
    {
        return _dbContext.Releases.OrderByDescending(r => r.InstalledAt).Skip(1).FirstOrDefault();
    }
}

public interface IReleaseRepository : IRepository
{
    bool SaveVersionInstallDate(string version);
    ReleaseEntity? GetPreviousInstalledVersion();
}