using System;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Artemis.Storage;

public static class StorageManager
{
    private static bool _ranMigrations;
    private static bool _inUse;

    /// <summary>
    ///     Creates a backup of the database if the last backup is older than 10 minutes
    ///     Removes the oldest backup if there are more than 5 backups present
    /// </summary>
    /// <param name="dataFolder">The Artemis data folder</param>
    public static void CreateBackup(string dataFolder)
    {
        if (_inUse)
            throw new Exception("Storage is already in use, can't backup now.");

        string database = Path.Combine(dataFolder, "artemis.db");
        if (!File.Exists(database))
            return;

        string backupFolder = Path.Combine(dataFolder, "database backups");
        Directory.CreateDirectory(backupFolder);
        FileSystemInfo[] files = new DirectoryInfo(backupFolder).GetFileSystemInfos();
        if (files.Length >= 5)
        {
            FileSystemInfo newest = files.OrderByDescending(fi => fi.CreationTime).First();
            FileSystemInfo oldest = files.OrderBy(fi => fi.CreationTime).First();
            if (DateTime.Now - newest.CreationTime < TimeSpan.FromHours(12))
                return;

            oldest.Delete();
        }

        File.Copy(database, Path.Combine(backupFolder, $"artemis-{DateTime.Now:yyyy-dd-M--HH-mm-ss}.db"));
    }
    
    public static ArtemisDbContext CreateDbContext(string dataFolder)
    {
        _inUse = true;

        ArtemisDbContext dbContext = new() {DataFolder = dataFolder};
        if (_ranMigrations)
            return dbContext;

        dbContext.Database.Migrate();
        dbContext.Database.ExecuteSqlRaw("PRAGMA optimize");
        _ranMigrations = true;

        return dbContext;
    }
}