using System;
using System.ComponentModel;
using System.IO;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core;

/// <summary>
///     Represents the icon of a <see cref="ProfileConfiguration" />
/// </summary>
public class ProfileConfigurationIcon : CorePropertyChanged, IStorageModel
{
    private readonly ProfileConfigurationEntity _entity;
    private bool _fill;
    private string? _iconName;
    private Stream? _iconStream;
    private ProfileConfigurationIconType _iconType;

    internal ProfileConfigurationIcon(ProfileConfigurationEntity entity)
    {
        _entity = entity;
    }

    /// <summary>
    ///     Gets the type of icon this profile configuration uses
    /// </summary>
    public ProfileConfigurationIconType IconType
    {
        get => _iconType;
        private set => SetAndNotify(ref _iconType, value);
    }

    /// <summary>
    ///     Gets the name of the icon if <see cref="IconType" /> is <see cref="ProfileConfigurationIconType.MaterialIcon" />
    /// </summary>
    public string? IconName
    {
        get => _iconName;
        private set => SetAndNotify(ref _iconName, value);
    }

    /// <summary>
    ///     Gets or sets a boolean indicating whether or not this icon should be filled.
    /// </summary>
    public bool Fill
    {
        get => _fill;
        set => SetAndNotify(ref _fill, value);
    }

    /// <summary>
    ///     Updates the <see cref="IconName" /> to the provided value and changes the <see cref="IconType" /> is
    ///     <see cref="ProfileConfigurationIconType.MaterialIcon" />
    /// </summary>
    /// <param name="iconName">The name of the icon</param>
    public void SetIconByName(string iconName)
    {
        if (iconName == null) throw new ArgumentNullException(nameof(iconName));

        _iconStream?.Dispose();
        IconName = iconName;
        IconType = ProfileConfigurationIconType.MaterialIcon;

        OnIconUpdated();
    }

    /// <summary>
    ///     Updates the stream returned by <see cref="GetIconStream" /> to the provided stream
    /// </summary>
    /// <param name="stream">The stream to copy</param>
    public void SetIconByStream(Stream stream)
    {
        if (stream == null) throw new ArgumentNullException(nameof(stream));

        _iconStream?.Dispose();
        _iconStream = new MemoryStream();
        stream.Seek(0, SeekOrigin.Begin);
        stream.CopyTo(_iconStream);
        _iconStream.Seek(0, SeekOrigin.Begin);

        IconName = null;
        IconType = ProfileConfigurationIconType.BitmapImage;
        OnIconUpdated();
    }

    /// <summary>
    ///     Creates a copy of the stream containing the icon
    /// </summary>
    /// <returns>A stream containing the icon</returns>
    public Stream? GetIconStream()
    {
        if (_iconStream == null)
            return null;

        MemoryStream stream = new();
        _iconStream.CopyTo(stream);

        stream.Seek(0, SeekOrigin.Begin);
        _iconStream.Seek(0, SeekOrigin.Begin);
        return stream;
    }

    /// <summary>
    ///     Occurs when the icon was updated
    /// </summary>
    public event EventHandler? IconUpdated;

    /// <summary>
    ///     Invokes the <see cref="IconUpdated" /> event
    /// </summary>
    protected virtual void OnIconUpdated()
    {
        IconUpdated?.Invoke(this, EventArgs.Empty);
    }

    #region Implementation of IStorageModel

    /// <inheritdoc />
    public void Load()
    {
        IconType = (ProfileConfigurationIconType) _entity.IconType;
        Fill = _entity.IconFill;
        if (IconType != ProfileConfigurationIconType.MaterialIcon)
            return;

        IconName = _entity.MaterialIcon;
        OnIconUpdated();
    }

    /// <inheritdoc />
    public void Save()
    {
        _entity.IconType = (int) IconType;
        _entity.MaterialIcon = IconType == ProfileConfigurationIconType.MaterialIcon ? IconName : null;
        _entity.IconFill = Fill;
    }

    #endregion
}

/// <summary>
///     Represents a type of profile icon
/// </summary>
public enum ProfileConfigurationIconType
{
    /// <summary>
    ///     An icon picked from the Material Design Icons collection
    /// </summary>
    [Description("Material Design Icon")] MaterialIcon,

    /// <summary>
    ///     A bitmap image icon
    /// </summary>
    [Description("Bitmap Image")] BitmapImage
}