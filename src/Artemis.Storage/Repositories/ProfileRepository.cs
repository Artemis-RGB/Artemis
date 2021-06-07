using System;
using System.Collections.Generic;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Repositories.Interfaces;
using LiteDB;

namespace Artemis.Storage.Repositories
{
    internal class ProfileRepository : IProfileRepository
    {
        private readonly LiteRepository _repository;

        public ProfileRepository(LiteRepository repository)
        {
            _repository = repository;
            _repository.Database.GetCollection<ProfileEntity>().EnsureIndex(s => s.Name);
        }

        public void Add(ProfileEntity profileEntity)
        {
            _repository.Insert(profileEntity);
        }

        public void Remove(ProfileEntity profileEntity)
        {
            _repository.Delete<ProfileEntity>(profileEntity.Id);
        }

        public List<ProfileEntity> GetAll()
        {
            return _repository.Query<ProfileEntity>().ToList();
        }

        public ProfileEntity Get(Guid id)
        {
            return _repository.FirstOrDefault<ProfileEntity>(p => p.Id == id);
        }

        public void Save(ProfileEntity profileEntity)
        {
            _repository.Upsert(profileEntity);
        }
    }
}