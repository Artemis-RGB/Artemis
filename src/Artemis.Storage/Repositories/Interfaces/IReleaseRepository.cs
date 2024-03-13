using Artemis.Storage.Entities.General;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IReleaseRepository : IRepository
{
    bool SaveVersionInstallDate(string version);
    ReleaseEntity? GetPreviousInstalledVersion();
}