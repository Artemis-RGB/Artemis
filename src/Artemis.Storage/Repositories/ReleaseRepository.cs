using System;
using System.Collections.Generic;
using System.Linq;
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
        _repository.Database.GetCollection<ReleaseEntity>().EnsureIndex(s => s.Status);
    }

    public string GetQueuedVersion()
    {
        return _repository.Query<ReleaseEntity>().Where(r => r.Status == ReleaseEntityStatus.Queued).FirstOrDefault()?.Version;
    }

    public string GetInstalledVersion()
    {
        return _repository.Query<ReleaseEntity>().Where(r => r.Status == ReleaseEntityStatus.Installed).FirstOrDefault()?.Version;
    }

    public string GetPreviousInstalledVersion()
    {
        return _repository.Query<ReleaseEntity>().Where(r => r.Status == ReleaseEntityStatus.Historical).OrderByDescending(r => r.InstalledAt).FirstOrDefault()?.Version;
    }
    
    public void QueueInstallation(string version)
    {
        // Mark release as queued and add if missing
        ReleaseEntity release = _repository.Query<ReleaseEntity>().Where(r => r.Version == version).FirstOrDefault() ?? new ReleaseEntity {Version = version};
        release.Status = ReleaseEntityStatus.Queued;
        _repository.Upsert(release);
    }

    public void FinishInstallation(string version)
    {
        // Mark release as installed and add if missing
        ReleaseEntity release = _repository.Query<ReleaseEntity>().Where(r => r.Version == version).FirstOrDefault() ?? new ReleaseEntity {Version = version};
        release.Status = ReleaseEntityStatus.Installed;
        release.InstalledAt = DateTimeOffset.UtcNow;
        _repository.Upsert(release);

        // Mark other releases as historical
        List<ReleaseEntity> oldReleases = _repository.Query<ReleaseEntity>().Where(r => r.Version != version && r.Status == ReleaseEntityStatus.Installed).ToList();
        if (!oldReleases.Any())
            return;
        
        foreach (ReleaseEntity oldRelease in oldReleases)
            oldRelease.Status = ReleaseEntityStatus.Historical;
        _repository.Update<ReleaseEntity>(oldReleases);
    }

    public void DequeueInstallation()
    {
        _repository.DeleteMany<ReleaseEntity>(r => r.Status == ReleaseEntityStatus.Queued);
    }
}

public interface IReleaseRepository : IRepository
{
    string GetQueuedVersion();
    string GetInstalledVersion();
    string GetPreviousInstalledVersion();
    void QueueInstallation(string version);
    void FinishInstallation(string version);
    void DequeueInstallation();
}