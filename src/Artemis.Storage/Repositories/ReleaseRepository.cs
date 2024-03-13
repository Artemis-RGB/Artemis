using System;
using System.Linq;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Repositories.Interfaces;

namespace Artemis.Storage.Repositories;

public class ReleaseRepository(Func<ArtemisDbContext> getContext) : IReleaseRepository
{
    public bool SaveVersionInstallDate(string version)
    {
        using ArtemisDbContext dbContext = getContext();
        
        ReleaseEntity? release = dbContext.Releases.FirstOrDefault(r => r.Version == version);
        if (release != null)
            return false;

        dbContext.Releases.Add(new ReleaseEntity {Version = version, InstalledAt = DateTimeOffset.UtcNow});
        dbContext.SaveChanges();
        return true;
    }

    public ReleaseEntity? GetPreviousInstalledVersion()
    {
        using ArtemisDbContext dbContext = getContext();
        return dbContext.Releases.AsEnumerable().OrderByDescending(r => r.InstalledAt).Skip(1).FirstOrDefault();
    }
}