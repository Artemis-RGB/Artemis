using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Artemis.Core;
using Artemis.Storage.Entities.Workshop;

namespace Artemis.WebClient.Workshop.Models;

public class InstalledEntry
{
    private Dictionary<string, JsonNode> _metadata = new();

    internal InstalledEntry(EntryEntity entity)
    {
        Entity = entity;
        Load();
    }

    public InstalledEntry(IEntrySummary entry, IRelease release)
    {
        Entity = new EntryEntity();

        EntryId = entry.Id;
        EntryType = entry.EntryType;

        Author = entry.Author;
        Name = entry.Name;
        InstalledAt = DateTimeOffset.Now;
        ReleaseId = release.Id;
        ReleaseVersion = release.Version;
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

        _metadata = Entity.Metadata != null ? new Dictionary<string, JsonNode>(Entity.Metadata) : new Dictionary<string, JsonNode>();
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

        Entity.Metadata = new Dictionary<string, JsonNode>(_metadata);
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
        if (!_metadata.TryGetValue(key, out JsonNode? element))
        {
            value = default;
            return false;
        }

        value = element.GetValue<T>();
        return value != null;
    }

    /// <summary>
    /// Sets metadata with the provided key to the provided value.
    /// </summary>
    /// <param name="key">The key of the value to set</param>
    /// <param name="value">The value to set.</param>
    public void SetMetadata(string key, object value)
    {
        _metadata[key] = JsonSerializer.SerializeToNode(value) ?? throw new InvalidOperationException();
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
    /// <returns>The directory info of the directory.</returns>
    public DirectoryInfo GetDirectory()
    {
        return new DirectoryInfo(Path.Combine(Constants.WorkshopFolder, $"{EntryId}-{StringUtilities.UrlFriendly(Name)}"));
    }

    /// <summary>
    /// Returns the directory info of a release of this entry, where any files would be stored if applicable.
    /// </summary>
    /// <param name="release">The release to use, if none provided the current release is used.</param>
    /// <returns>The directory info of the directory.</returns>
    public DirectoryInfo GetReleaseDirectory(IRelease? release = null)
    {
        return new DirectoryInfo(Path.Combine(GetDirectory().FullName, StringUtilities.UrlFriendly(release?.Version ?? ReleaseVersion)));
    }

    /// <summary>
    /// Applies the provided release to the installed entry.
    /// </summary>
    /// <param name="release">The release to apply.</param>
    public void ApplyRelease(IRelease release)
    {
        ReleaseId = release.Id;
        ReleaseVersion = release.Version;
        InstalledAt = DateTimeOffset.UtcNow;
    }
}