using System.Diagnostics.CodeAnalysis;
using Artemis.Core;
using Artemis.Storage.Entities.Workshop;

namespace Artemis.WebClient.Workshop.Services;

public class InstalledEntry
{
    private Dictionary<string, object> _metadata = new();

    internal InstalledEntry(EntryEntity entity)
    {
        Entity = entity;
        Load();
    }

    public InstalledEntry(IGetEntryById_Entry entry)
    {
        Entity = new EntryEntity();

        EntryId = entry.Id;
        EntryType = entry.EntryType;

        Author = entry.Author;
        Name = entry.Name;
    }

    public long EntryId { get; set; }
    public EntryType EntryType { get; set; }

    public string Author { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public long ReleaseId { get; set; }
    public string ReleaseVersion { get; set; } = string.Empty;
    public DateTimeOffset InstalledAt { get; set; }

    internal EntryEntity Entity { get; }

    internal void Load()
    {
        EntryId = Entity.EntryId;
        EntryType = (EntryType) Entity.EntryType;

        Author = Entity.Author;
        Name = Entity.Name;

        ReleaseId = Entity.ReleaseId;
        ReleaseVersion = Entity.ReleaseVersion;
        InstalledAt = Entity.InstalledAt;

        _metadata = new Dictionary<string, object>(Entity.Metadata);
    }

    internal void Save()
    {
        Entity.EntryId = EntryId;
        Entity.EntryType = (int) EntryType;

        Entity.Author = Author;
        Entity.Name = Name;

        Entity.ReleaseId = ReleaseId;
        Entity.ReleaseVersion = ReleaseVersion;
        Entity.InstalledAt = InstalledAt;

        Entity.Metadata = new Dictionary<string, object>(_metadata);
    }

    /// <summary>
    /// Gets the metadata value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">When this method returns, contains the value associated with the specified key, if the key is found and of type <typeparamref name="T"/>;
    /// otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns><see langword="true"/> if the metadata contains an element with the specified key; otherwise, <see langword="false"/>.</returns>
    public bool TryGetMetadata<T>(string key, [NotNullWhen(true)] out T? value)
    {
        if (!_metadata.TryGetValue(key, out object? objectValue) || objectValue is not T result)
        {
            value = default;
            return false;
        }

        value = result;
        return true;
    }

    /// <summary>
    /// Sets metadata with the provided key to the provided value.
    /// </summary>
    /// <param name="key">The key of the value to set</param>
    /// <param name="value">The value to set.</param>
    public void SetMetadata(string key, object value)
    {
        _metadata.Add(key, value);
    }

    /// <summary>
    /// Removes metadata with the provided key.
    /// </summary>
    /// <param name="key">The key of the metadata to remove</param>
    /// <returns><see langword="true"/> if the element is successfully found and removed; otherwise, <see langword="false"/>.</returns>
    public bool RemoveMetadata(string key)
    {
        return _metadata.Remove(key);
    }

    /// <summary>
    /// Returns the directory info of the entry, where any files would be stored if applicable.
    /// </summary>
    /// <param name="rootDirectory">A value indicating whether or not to return the root directory of the entry, and not the version.</param>
    /// <returns>The directory info of the entry, where any files would be stored if applicable.</returns>
    public DirectoryInfo GetDirectory(bool rootDirectory = false)
    {
        if (rootDirectory)
            return new DirectoryInfo(Path.Combine(Constants.WorkshopFolder, EntryId.ToString()));
        
        string safeVersion = Path.GetInvalidFileNameChars().Aggregate(ReleaseVersion, (current, c) => current.Replace(c, '-'));
        return new DirectoryInfo(Path.Combine(Constants.WorkshopFolder, EntryId.ToString(), safeVersion));
    }
}