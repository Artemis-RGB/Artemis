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
    private readonly ProfileContainerEntity _entity;
    private bool _fill;
    private string? _iconName;
    private byte[]? _iconBytes;
    private ProfileConfigurationIconType _iconType;

    internal ProfileConfigurationIcon(ProfileContainerEntity entity)
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
    ///   Gets or sets the icon bytes if <see cref="IconType" /> is <see cref="ProfileConfigurationIconType.BitmapImage" />
    /// </summary>
    public byte[]? IconBytes
    {
        get => _iconBytes;
        private set => SetAndNotify(ref _iconBytes, value);
    }

    /// <summary>
    ///     Updates the <see cref="IconName" /> to the provided value and changes the <see cref="IconType" /> is
    ///     <see cref="ProfileConfigurationIconType.MaterialIcon" />
    /// </summary>
    /// <param name="iconName">The name of the icon</param>
    public void SetIconByName(string iconName)
    {
        ArgumentNullException.ThrowIfNull(iconName);

        IconBytes = null;
        IconName = iconName;
        IconType = ProfileConfigurationIconType.MaterialIcon;

        OnIconUpdated();
    }

    /// <summary>
    ///     Updates the <see cref="IconBytes" /> to the provided value and changes the <see cref="IconType" /> is
    /// </summary>
    /// <param name="stream">The stream to copy</param>
    public void SetIconByStream(Stream stream)
    {
        ArgumentNullException.ThrowIfNull(stream);

        if (stream.CanSeek)
            stream.Seek(0, SeekOrigin.Begin);

        using (MemoryStream ms = new())
        {
            stream.CopyTo(ms);
            IconBytes = ms.ToArray();
        }

        IconName = null;
        IconType = ProfileConfigurationIconType.BitmapImage;
        OnIconUpdated();
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
        IconType = (ProfileConfigurationIconType) _entity.ProfileConfiguration.IconType;
        Fill = _entity.ProfileConfiguration.IconFill;
        if (IconType != ProfileConfigurationIconType.MaterialIcon)
            return;

        IconName = _entity.ProfileConfiguration.MaterialIcon;
        IconBytes = IconType == ProfileConfigurationIconType.BitmapImage ? _entity.Icon : null;

        OnIconUpdated();
    }

    /// <inheritdoc />
    public void Save()
    {
        _entity.ProfileConfiguration.IconType = (int) IconType;
        _entity.ProfileConfiguration.MaterialIcon = IconType == ProfileConfigurationIconType.MaterialIcon ? IconName : null;
        _entity.ProfileConfiguration.IconFill = Fill;
        _entity.Icon = IconBytes ?? Array.Empty<byte>();
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