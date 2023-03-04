using System;
using System.Collections.Generic;
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

    public ReleaseEntity GetQueuedVersion()
    {
        return _repository.Query<ReleaseEntity>().Where(r => r.Status == ReleaseEntityStatus.Queued).FirstOrDefault();
    }

    public ReleaseEntity GetInstalledVersion()
    {
        return _repository.Query<ReleaseEntity>().Where(r => r.Status == ReleaseEntityStatus.Installed).FirstOrDefault();
    }

    public ReleaseEntity GetPreviousInstalledVersion()
    {
        return _repository.Query<ReleaseEntity>().Where(r => r.Status == ReleaseEntityStatus.Historical).OrderByDescending(r => r.InstalledAt).FirstOrDefault();
    }

    public void QueueInstallation(string version, string releaseId)
    {
        // Mark release as queued and add if missing
        ReleaseEntity release = _repository.Query<ReleaseEntity>().Where(r => r.Version == version).FirstOrDefault() ?? new ReleaseEntity {Version = version, ReleaseId = releaseId};
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
        List<ReleaseEntity> oldReleases = _repository.Query<ReleaseEntity>().Where(r => r.Version != version && r.Status != ReleaseEntityStatus.Historical).ToList();
        foreach (ReleaseEntity oldRelease in oldReleases)
            oldRelease.Status = ReleaseEntityStatus.Historical;
        _repository.Update<ReleaseEntity>(oldReleases);
    }

    public void DequeueInstallation()
    {
        // Mark all queued releases as unknown, until FinishInstallation is called we don't know the status
        List<ReleaseEntity> queuedReleases = _repository.Query<ReleaseEntity>().Where(r => r.Status == ReleaseEntityStatus.Queued).ToList();
        foreach (ReleaseEntity queuedRelease in queuedReleases)
            queuedRelease.Status = ReleaseEntityStatus.Unknown;
        _repository.Update<ReleaseEntity>(queuedReleases);
    }
}

public interface IReleaseRepository : IRepository
{
    ReleaseEntity GetQueuedVersion();
    ReleaseEntity GetInstalledVersion();
    ReleaseEntity GetPreviousInstalledVersion();
    void QueueInstallation(string version, string releaseId);
    void FinishInstallation(string version);
    void DequeueInstallation();
}