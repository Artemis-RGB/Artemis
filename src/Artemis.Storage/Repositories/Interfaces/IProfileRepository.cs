using System.Collections.Generic;
using System.Text.Json.Nodes;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Storage.Repositories.Interfaces;

public interface IProfileRepository : IRepository
{
    void Add(ProfileContainerEntity profileContainerEntity);
    void Remove(ProfileContainerEntity profileContainerEntity);
    void Save(ProfileContainerEntity profileContainerEntity);
    void SaveRange(List<ProfileContainerEntity> profileContainerEntities);
    void MigrateProfiles();
    void MigrateProfile(JsonObject? configurationJson, JsonObject? profileJson);
}