using System;
using System.Collections.Generic;
using LiteDB;

namespace Artemis.Storage.Entities.Profile
{
    public class FolderEntity
    {
        public Guid Id { get; set; }
        public Guid ParentId { get; set; }

        public int Order { get; set; }
        public string Name { get; set; }

        public List<ProfileConditionEntity> Conditions { get; set; }

        [BsonRef("ProfileEntity")]
        public ProfileEntity Profile { get; set; }

        public Guid ProfileId { get; set; }
    }
}