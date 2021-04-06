﻿using Artemis.Storage.Entities.Profile;
using Artemis.Storage.Migrations.Interfaces;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Artemis.Storage.Migrations
{
    public class M0004ProfileSegmentsMigration : IStorageMigration
    {
        public int UserVersion => 4;

        public void Apply(LiteRepository repository)
        {
            // Lesson for next time: Use BsonDocuments in migrations
            // List<ProfileEntity> profiles = repository.Query<ProfileEntity>().ToList();
            // foreach (ProfileEntity profileEntity in profiles)
            // {
            //     foreach (FolderEntity folder in profileEntity.Folders.Where(f => f.MainSegmentLength == TimeSpan.Zero))
            //     {
            //         if (folder.PropertyEntities.Any(p => p.KeyframeEntities.Any()))
            //             folder.MainSegmentLength = folder.PropertyEntities.Where(p => p.KeyframeEntities.Any()).Max(p => p.KeyframeEntities.Max(k => k.Position));
            //         if (folder.MainSegmentLength == TimeSpan.Zero)
            //             folder.MainSegmentLength = TimeSpan.FromSeconds(5);
            //
            //         folder.PlayMode = 0;
            //     }
            //
            //     foreach (LayerEntity layer in profileEntity.Layers.Where(l => l.MainSegmentLength == TimeSpan.Zero))
            //     {
            //         if (layer.PropertyEntities.Any(p => p.KeyframeEntities.Any()))
            //             layer.MainSegmentLength = layer.PropertyEntities.Where(p => p.KeyframeEntities.Any()).Max(p => p.KeyframeEntities.Max(k => k.Position));
            //         if (layer.MainSegmentLength == TimeSpan.Zero)
            //             layer.MainSegmentLength = TimeSpan.FromSeconds(5);
            //
            //         layer.PlayMode = 0;
            //     }
            //
            //     repository.Update(profileEntity);
            // }
        }
    }
}