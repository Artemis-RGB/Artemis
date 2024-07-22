using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Nodes;
using Artemis.Core;
using Artemis.Storage.Entities.Workshop;

namespace Artemis.WebClient.Workshop.Models;

public class InstalledEntry : CorePropertyChanged
{
    private string _author;
    private bool _autoUpdate;
    private IReadOnlyList<InstalledEntryCategory> _categories;
    private DateTimeOffset _createdAt;
    private long _downloads;
    private EntryType _entryType;
    private long _id;
    private bool _isOfficial;
    private long? _latestReleaseId;
    private Dictionary<string, JsonNode> _metadata = new();
    private string _name;
    private long _releaseId;
    private string _releaseVersion = string.Empty;
    private string _summary;

    internal InstalledEntry(EntryEntity entity)
    {
        Entity = entity;
        Load();
    }

    public InstalledEntry(IEntrySummary entry, IRelease release)
    {
        Entity = new EntryEntity();

        ApplyEntrySummary(entry);
        InstalledAt = DateTimeOffset.Now;
        ReleaseId = release.Id;
        ReleaseVersion = release.Version;
        AutoUpdate = true;
    }

    public DateTimeOffset InstalledAt { get; set; }

    public long ReleaseId
    {
        get => _releaseId;
        set => SetAndNotify(ref _releaseId, value);
    }

    public string ReleaseVersion
    {
        get => _releaseVersion;
        set => SetAndNotify(ref _releaseVersion, value);
    }

    public bool AutoUpdate
    {
        get => _autoUpdate;
        set => SetAndNotify(ref _autoUpdate, value);
    }

    public long Id
    {
        get => _id;
        private set => SetAndNotify(ref _id, value);
    }

    public string Author
    {
        get => _author;
        private set => SetAndNotify(ref _author, value);
    }

    public bool IsOfficial
    {
        get => _isOfficial;
        private set => SetAndNotify(ref _isOfficial, value);
    }

    public string Name
    {
        get => _name;
        private set => SetAndNotify(ref _name, value);
    }

    public string Summary
    {
        get => _summary;
        private set => SetAndNotify(ref _summary, value);
    }

    public EntryType EntryType
    {
        get => _entryType;
        private set => SetAndNotify(ref _entryType, value);
    }

    public long Downloads
    {
        get => _downloads;
        private set => SetAndNotify(ref _downloads, value);
    }

    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        private set => SetAndNotify(ref _createdAt, value);
    }

    public long? LatestReleaseId
    {
        get => _latestReleaseId;
        private set => SetAndNotify(ref _latestReleaseId, value);
    }

    public IReadOnlyList<InstalledEntryCategory> Categories
    {
        get => _categories;
        private set => SetAndNotify(ref _categories, value);
    }

    internal EntryEntity Entity { get; }

    /// <summary>
    ///     Gets the metadata value associated with the specified key.
    /// </summary>
    /// <param name="key">The key of the value to get.</param>
    /// <param name="value">
    ///     When this method returns, contains the value associated with the specified key, if the key is found and of type
    ///     <typeparamref name="T" />;
    ///     otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.
    /// </param>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>
    ///     <see langword="true" /> if the metadata contains an element with the specified key; otherwise,
    ///     <see langword="false" />.
    /// </returns>
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
    ///     Sets metadata with the provided key to the provided value.
    /// </summary>
    /// <param name="key">The key of the value to set</param>
    /// <param name="value">The value to set.</param>
    public void SetMetadata(string key, object value)
    {
        _metadata[key] = JsonSerializer.SerializeToNode(value) ?? throw new InvalidOperationException();
    }

    /// <summary>
    ///     Removes metadata with the provided key.
    /// </summary>
    /// <param name="key">The key of the metadata to remove</param>
    /// <returns><see langword="true" /> if the element is successfully found and removed; otherwise, <see langword="false" />.</returns>
    public bool RemoveMetadata(string key)
    {
        return _metadata.Remove(key);
    }

    /// <summary>
    ///     Returns the directory info of the entry, where any files would be stored if applicable.
    /// </summary>
    /// <returns>The directory info of the directory.</returns>
    public DirectoryInfo GetDirectory()
    {
        return new DirectoryInfo(Path.Combine(Constants.WorkshopFolder, $"{Id}-{StringUtilities.UrlFriendly(Name)}"));
    }

    /// <summary>
    ///     Returns the directory info of a release of this entry, where any files would be stored if applicable.
    /// </summary>
    /// <param name="release">The release to use, if none provided the current release is used.</param>
    /// <returns>The directory info of the directory.</returns>
    public DirectoryInfo GetReleaseDirectory(IRelease? release = null)
    {
        return new DirectoryInfo(Path.Combine(GetDirectory().FullName, StringUtilities.UrlFriendly(release?.Version ?? ReleaseVersion)));
    }

    /// <summary>
    ///     Applies the provided release to the installed entry.
    /// </summary>
    /// <param name="release">The release to apply.</param>
    public void ApplyRelease(IRelease release)
    {
        ReleaseId = release.Id;
        ReleaseVersion = release.Version;
        InstalledAt = DateTimeOffset.UtcNow;
    }

    public void ApplyEntrySummary(IEntrySummary entry)
    {
        Id = entry.Id;
        Author = entry.Author;
        IsOfficial = entry.IsOfficial;
        Name = entry.Name;
        Summary = entry.Summary;
        EntryType = entry.EntryType;
        Downloads = entry.Downloads;
        CreatedAt = entry.CreatedAt;
        LatestReleaseId = entry.LatestReleaseId;
        Categories = entry.Categories.Select(c => new InstalledEntryCategory(c.Name, c.Icon)).ToList();
    }


    public override string ToString()
    {
        return $"[{EntryType}] {Id} - {Name}";
    }

    internal void Load()
    {
        Id = Entity.EntryId;
        Author = Entity.Author;
        IsOfficial = Entity.IsOfficial;
        Name = Entity.Name;
        Summary = Entity.Summary;
        EntryType = (EntryType) Entity.EntryType;
        Downloads = Entity.Downloads;
        CreatedAt = Entity.CreatedAt;
        LatestReleaseId = Entity.LatestReleaseId;
        Categories = Entity.Categories?.Select(c => new InstalledEntryCategory(c.Name, c.Icon)).ToList() ?? [];

        ReleaseId = Entity.ReleaseId;
        ReleaseVersion = Entity.ReleaseVersion;
        InstalledAt = Entity.InstalledAt;
        AutoUpdate = Entity.AutoUpdate;

        _metadata = Entity.Metadata != null ? new Dictionary<string, JsonNode>(Entity.Metadata) : [];
    }

    internal void Save()
    {
        Entity.EntryId = Id;
        Entity.EntryType = (int) EntryType;

        Entity.Author = Author;
        Entity.IsOfficial = IsOfficial;
        Entity.Name = Name;
        Entity.Summary = Summary;
        Entity.Downloads = Downloads;
        Entity.CreatedAt = CreatedAt;
        Entity.LatestReleaseId = LatestReleaseId;
        Entity.Categories = Categories.Select(c => new EntryCategoryEntity(c.Name, c.Icon)).ToList();

        Entity.ReleaseId = ReleaseId;
        Entity.ReleaseVersion = ReleaseVersion;
        Entity.InstalledAt = InstalledAt;
        Entity.AutoUpdate = AutoUpdate;

        Entity.Metadata = new Dictionary<string, JsonNode>(_metadata);
    }
}