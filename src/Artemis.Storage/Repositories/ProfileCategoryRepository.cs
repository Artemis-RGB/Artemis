using System;
using System.Collections.Generic;
using System.IO;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories;

internal class ProfileCategoryRepository : IProfileCategoryRepository
{
    private readonly ILiteStorage<Guid> _profileIcons;
    private readonly LiteRepository _repository;

    public ProfileCategoryRepository(LiteRepository repository)
    {
        _repository = repository;
        _repository.Database.GetCollection<ProfileCategoryEntity>().EnsureIndex(s => s.Name, true);
        _profileIcons = _repository.Database.GetStorage<Guid>("profileIcons");
    }

    public void Add(ProfileCategoryEntity profileCategoryEntity)
    {
        _repository.Insert(profileCategoryEntity);
    }

    public void Remove(ProfileCategoryEntity profileCategoryEntity)
    {
        _repository.Delete<ProfileCategoryEntity>(profileCategoryEntity.Id);
    }

    public List<ProfileCategoryEntity> GetAll()
    {
        return _repository.Query<ProfileCategoryEntity>().ToList();
    }

    public ProfileCategoryEntity Get(Guid id)
    {
        return _repository.FirstOrDefault<ProfileCategoryEntity>(p => p.Id == id);
    }

    public ProfileCategoryEntity IsUnique(string name, Guid? id)
    {
        name = name.Trim();
        if (id == null)
            return _repository.FirstOrDefault<ProfileCategoryEntity>(p => p.Name == name);
        return _repository.FirstOrDefault<ProfileCategoryEntity>(p => p.Name == name && p.Id != id.Value);
    }

    public void Save(ProfileCategoryEntity profileCategoryEntity)
    {
        _repository.Upsert(profileCategoryEntity);
    }

    public Stream GetProfileIconStream(Guid id)
    {
        if (!_profileIcons.Exists(id))
            return null;

        MemoryStream stream = new();
        _profileIcons.Download(id, stream);
        return stream;
    }

    public void SaveProfileIconStream(ProfileConfigurationEntity profileConfigurationEntity, Stream stream)
    {
        if (profileConfigurationEntity.FileIconId == Guid.Empty)
            profileConfigurationEntity.FileIconId = Guid.NewGuid();

        if (stream == null && _profileIcons.Exists(profileConfigurationEntity.FileIconId))
            _profileIcons.Delete(profileConfigurationEntity.FileIconId);

        _profileIcons.Upload(profileConfigurationEntity.FileIconId, profileConfigurationEntity.IconOriginalFileName, stream);
    }
}