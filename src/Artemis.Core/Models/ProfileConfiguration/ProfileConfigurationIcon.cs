using System;
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
        private string? _iconName;
        private Stream? _iconStream;
        private ProfileConfigurationIconType _iconType;
        private string? _originalFileName;

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
        ///     Gets the original file name of the icon (if applicable)
        /// </summary>
        public string? OriginalFileName
        {
            get => _originalFileName;
            private set => SetAndNotify(ref _originalFileName, value);
        }

        /// <summary>
        ///     Updates the <see cref="IconName" /> to the provided value and changes the <see cref="IconType" /> is
        ///     <see cref="ProfileConfigurationIconType.MaterialIcon" />
        /// </summary>
        /// <param name="iconName">The name of the icon</param>
        public void SetIconByName(string iconName)
        {
            IconName = iconName ?? throw new ArgumentNullException(nameof(iconName));
            OriginalFileName = null;
            IconType = ProfileConfigurationIconType.MaterialIcon;

            _iconStream?.Dispose();
        }

        /// <summary>
        ///     Updates the stream returned by <see cref="GetIconStream" /> to the provided stream
        /// </summary>
        /// <param name="originalFileName">The original file name backing the stream, should include the extension</param>
        /// <param name="stream">The stream to copy</param>
        public void SetIconByStream(string originalFileName, Stream stream)
        {
            if (originalFileName == null) throw new ArgumentNullException(nameof(originalFileName));
            if (stream == null) throw new ArgumentNullException(nameof(stream));

            _iconStream?.Dispose();
            _iconStream = new MemoryStream();
            stream.Seek(0, SeekOrigin.Begin);
            stream.CopyTo(_iconStream);
            _iconStream.Seek(0, SeekOrigin.Begin);

            IconName = null;
            OriginalFileName = originalFileName;
            IconType = OriginalFileName.EndsWith(".svg") ? ProfileConfigurationIconType.SvgImage : ProfileConfigurationIconType.BitmapImage;
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

        #region Implementation of IStorageModel

        /// <inheritdoc />
        public void Load()
        {
            IconType = (ProfileConfigurationIconType) _entity.IconType;
            if (IconType == ProfileConfigurationIconType.MaterialIcon)
                IconName = _entity.MaterialIcon;
        }

        /// <inheritdoc />
        public void Save()
        {
            _entity.IconType = (int) IconType;
            _entity.MaterialIcon = IconType == ProfileConfigurationIconType.MaterialIcon ? IconName : null;
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