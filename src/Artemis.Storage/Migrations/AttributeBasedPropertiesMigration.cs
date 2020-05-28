using System;
using System.Collections.Generic;
using System.Text;
using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;

namespace Artemis.Storage.Migrations
{
    public class AttributeBasedPropertiesMigration : IStorageMigration
    {
        public int UserVersion => 1;
        public void Apply(LiteRepository repository)
        {
            if (repository.Database.CollectionExists("ProfileEntity"))
                repository.Database.DropCollection("ProfileEntity");
        }
    }
}
