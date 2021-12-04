using System;
using System.ComponentModel;
using System.IO;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Artemis.UI.Shared.Controls
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
                if (ConfigurationIcon.IconType == ProfileConfigurationIconType.MaterialIcon)
                {
                    Content = Enum.TryParse(ConfigurationIcon.IconName, true, out MaterialIconKind parsedIcon)
                        ? new MaterialIcon {Kind = parsedIcon!}
                        : new MaterialIcon {Kind = MaterialIconKind.QuestionMark};
                    return;
                }

                Stream? stream = ConfigurationIcon.GetIconStream();
                if (stream == null)
                {
                    Content = new MaterialIcon {Kind = MaterialIconKind.QuestionMark};
                    return;
                }

                if (ConfigurationIcon.IconType == ProfileConfigurationIconType.SvgImage)
                {
                    SvgSource source = new();
                    source.Load(stream);
                    Content = new Image {Source = new SvgImage {Source = source}};
                }
                else if (ConfigurationIcon.IconType == ProfileConfigurationIconType.BitmapImage)
                    Content = new Image {Source = new Bitmap(ConfigurationIcon.GetIconStream())};
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

            if (Content is Image image && image.Source is IDisposable disposable) 
                disposable.Dispose();
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