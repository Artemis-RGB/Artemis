using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories
{
    internal class ProfileCategoryRepository : IProfileCategoryRepository
    {
        private readonly LiteRepository _repository;

        public ProfileCategoryRepository(LiteRepository repository)
        {
            _repository = repository;
            _repository.Database.GetCollection<ProfileCategoryEntity>().EnsureIndex(s => s.Name, true);
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
        
        public void Save(ProfileCategoryEntity profileCategoryEntity)
        {
            _repository.Upsert(profileCategoryEntity);
        }
    }
}