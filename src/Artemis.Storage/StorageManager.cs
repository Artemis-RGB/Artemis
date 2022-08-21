using System;
using System.IO;
using System.Linq;
using LiteDB;

namespace Artemis.Storage;

public static class StorageManager
{
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

        string database = Path.Combine(dataFolder, "database.db");
        if (!File.Exists(database))
            return;

        string backupFolder = Path.Combine(dataFolder, "database backups");
        Directory.CreateDirectory(backupFolder);
        FileSystemInfo[] files = new DirectoryInfo(backupFolder).GetFileSystemInfos();
        if (files.Length >= 5)
        {
            FileSystemInfo newest = files.OrderByDescending(fi => fi.CreationTime).First();
            FileSystemInfo oldest = files.OrderBy(fi => fi.CreationTime).First();
            if (DateTime.Now - newest.CreationTime < TimeSpan.FromMinutes(10))
                return;

            oldest.Delete();
        }

        File.Copy(database, Path.Combine(backupFolder, $"database-{DateTime.Now:yyyy-dd-M--HH-mm-ss}.db"));
    }

    /// <summary>
    ///     Creates the LiteRepository that will be managed by dependency injection
    /// </summary>
    /// <param name="dataFolder">The Artemis data folder</param>
    public static LiteRepository CreateRepository(string dataFolder)
    {
        if (_inUse)
            throw new Exception("Storage is already in use, use dependency injection to get the repository.");

        try
        {
            _inUse = true;
            return new LiteRepository($"FileName={Path.Combine(dataFolder, "database.db")}");
        }
        catch (LiteException e)
        {
            // I don't like this way of error reporting, now I need to use reflection if I want a meaningful error message
            throw new Exception($"LiteDB threw error code {e.ErrorCode}. See inner exception for more details", e);
        }
    }
}