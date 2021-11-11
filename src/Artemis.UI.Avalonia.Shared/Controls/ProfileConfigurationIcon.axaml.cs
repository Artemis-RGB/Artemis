using System;
using System.ComponentModel;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Artemis.UI.Avalonia.Shared.Controls
{
    public class ProfileConfigurationIcon : UserControl
    {
        #region Properties

        /// <summary>
        ///     Gets or sets the <see cref="Core.ProfileConfigurationIcon" /> to display
        /// </summary>
        public static readonly StyledProperty<Core.ProfileConfigurationIcon?> ConfigurationIconProperty =
            AvaloniaProperty.Register<ProfileConfigurationIcon, Core.ProfileConfigurationIcon?>(nameof(ConfigurationIcon));

        /// <summary>
        ///     Gets or sets the <see cref="Core.ProfileConfigurationIcon" /> to display
        /// </summary>
        public Core.ProfileConfigurationIcon? ConfigurationIcon
        {
            get => GetValue(ConfigurationIconProperty);
            set => SetValue(ConfigurationIconProperty, value);
        }

        #endregion

        public ProfileConfigurationIcon()
        {
            InitializeComponent();
            DetachedFromLogicalTree += OnDetachedFromLogicalTree;
            PropertyChanged += OnPropertyChanged;
        }


        private void Update()
        {
            if (ConfigurationIcon == null)
                return;

            try
            {
                if (ConfigurationIcon.IconType == ProfileConfigurationIconType.SvgImage && ConfigurationIcon.FileIcon != null)
                {
                    SvgSource source = new();
                    source.Load(ConfigurationIcon.FileIcon);
                    Content = new SvgImage {Source = source};
                }
                else if (ConfigurationIcon.IconType == ProfileConfigurationIconType.MaterialIcon && ConfigurationIcon.MaterialIcon != null)
                {
                    Content = Enum.TryParse(ConfigurationIcon.MaterialIcon, true, out MaterialIconKind parsedIcon)
                        ? new MaterialIcon {Kind = parsedIcon!}
                        : new MaterialIcon {Kind = MaterialIconKind.QuestionMark};
                }
                else if (ConfigurationIcon.IconType == ProfileConfigurationIconType.BitmapImage && ConfigurationIcon.FileIcon != null)
                    Content = new Image {Source = new Bitmap(ConfigurationIcon.FileIcon)};
                else
                    Content = new MaterialIcon {Kind = MaterialIconKind.QuestionMark};
            }
            catch
            {
                Content = new MaterialIcon {Kind = MaterialIconKind.QuestionMark};
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Event handlers

        private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            if (ConfigurationIcon != null)
                ConfigurationIcon.PropertyChanged -= IconOnPropertyChanged;

            if (Content is SvgImage svgImage)
                svgImage.Source?.Dispose();
            else if (Content is Image image)
                ((Bitmap) image.Source).Dispose();
        }

        private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
        {
            if (e.Property == ConfigurationIconProperty)
            {
                if (e.OldValue is Core.ProfileConfigurationIcon oldIcon)
                    oldIcon.PropertyChanged -= IconOnPropertyChanged;
                if (e.NewValue is Core.ProfileConfigurationIcon newIcon)
                    newIcon.PropertyChanged += IconOnPropertyChanged;
                Update();
            }
        }

        private void IconOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Update();
        }

        #endregion
    }
}