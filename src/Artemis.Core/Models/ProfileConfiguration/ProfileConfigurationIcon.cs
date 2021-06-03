using System.ComponentModel;
using System.IO;
using Artemis.Storage.Entities.Profile;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents the icon of a <see cref="ProfileConfiguration" />
    /// </summary>
    public class ProfileConfigurationIcon : CorePropertyChanged, IStorageModel
    {
        private readonly ProfileConfigurationEntity _entity;
        private Stream? _fileIcon;
        private ProfileConfigurationIconType _iconType;
        private string? _materialIcon;

        internal ProfileConfigurationIcon(ProfileConfigurationEntity entity)
        {
            _entity = entity;
        }

        /// <summary>
        ///     Gets or sets the type of icon this profile configuration uses
        /// </summary>
        public ProfileConfigurationIconType IconType
        {
            get => _iconType;
            set => SetAndNotify(ref _iconType, value);
        }

        /// <summary>
        ///     Gets or sets the icon if it is a Material icon
        /// </summary>
        public string? MaterialIcon
        {
            get => _materialIcon;
            set => SetAndNotify(ref _materialIcon, value);
        }

        /// <summary>
        ///     Gets or sets a stream containing the icon if it is bitmap or SVG
        /// </summary>
        /// <returns></returns>
        public Stream? FileIcon
        {
            get => _fileIcon;
            set => SetAndNotify(ref _fileIcon, value);
        }

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            IconType = (ProfileConfigurationIconType) _entity.IconType;
            MaterialIcon = _entity.MaterialIcon;
        }

        /// <inheritdoc />
        public void Save()
        {
            _entity.IconType = (int) IconType;
            _entity.MaterialIcon = MaterialIcon;
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
        [Description("Bitmap Image")] BitmapImage,

        /// <summary>
        ///     An SVG image icon
        /// </summary>
        [Description("SVG Image")] SvgImage
    }
}