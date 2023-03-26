using System;
using Artemis.Storage.Entities.General;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories;

public class ReleaseRepository : IReleaseRepository
{
    private readonly LiteRepository _repository;

    public ReleaseRepository(LiteRepository repository)
    {
        _repository = repository;
        _repository.Database.GetCollection<ReleaseEntity>().EnsureIndex(s => s.Version, true);
    }

    public bool SaveVersionInstallDate(string version)
    {
        ReleaseEntity release = _repository.Query<ReleaseEntity>().Where(r => r.Version == version).FirstOrDefault();
        if (release != null)
            return false;

        _repository.Insert(new ReleaseEntity {Version = version, InstalledAt = DateTimeOffset.UtcNow});
        return true;
    }

    public ReleaseEntity GetPreviousInstalledVersion()
    {
        return _repository.Query<ReleaseEntity>().OrderByDescending(r => r.InstalledAt).Skip(1).FirstOrDefault();
    }
}

public interface IReleaseRepository : IRepository
{
    bool SaveVersionInstallDate(string version);
    ReleaseEntity GetPreviousInstalledVersion();
}