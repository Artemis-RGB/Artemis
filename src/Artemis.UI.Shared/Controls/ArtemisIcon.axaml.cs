using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media.Imaging;
using Avalonia.Svg.Skia;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a control that can display an arbitrary kind of icon.
    /// </summary>
    public class ArtemisIcon : UserControl
    {
        /// <summary>
        ///     Creates a new instance of the <see cref="ArtemisIcon" /> class.
        /// </summary>
        public ArtemisIcon()
        {
            InitializeComponent();
            DetachedFromLogicalTree += OnDetachedFromLogicalTree;
            LayoutUpdated += OnLayoutUpdated;
        }

        private static void IconChanging(IAvaloniaObject sender, bool before)
        {
            if (before)
                ((ArtemisIcon) sender).Update();
        }

        private void Update()
        {
            try
            {
                // First look for an enum value instead of a string
                if (Icon is MaterialIconKind materialIcon)
                {
                    Content = new MaterialIcon {Kind = materialIcon, Width = Bounds.Width, Height = Bounds.Height};
                }
                // If it's a string there are several options
                else if (Icon is string iconString)
                {
                    // An enum defined as a string
                    if (Enum.TryParse(iconString, true, out MaterialIconKind parsedIcon))
                    {
                        Content = new MaterialIcon {Kind = parsedIcon, Width = Bounds.Width, Height = Bounds.Height};
                    }
                    // An URI pointing to an SVG
                    else if (iconString.EndsWith(".svg"))
                    {
                        SvgSource source = new();
                        source.Load(iconString);
                        Content = new SvgImage {Source = source};
                    }
                    // An URI pointing to a different kind of image
                    else
                    {
                        Content = new Image {Source = new Bitmap(iconString), Width = Bounds.Width, Height = Bounds.Height};
                    }
                }
            }
            catch
            {
                Content = new MaterialIcon {Kind = MaterialIconKind.QuestionMark, Width = Bounds.Width, Height = Bounds.Height};
            }
        }

        private void OnLayoutUpdated(object? sender, EventArgs e)
        {
            if (Content is Control contentControl)
            {
                contentControl.Width = Bounds.Width;
                contentControl.Height = Bounds.Height;
            }
        }

        private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
        {
            if (Content is SvgImage svgImage)
                svgImage.Source?.Dispose();
            else if (Content is Image image)
                ((Bitmap) image.Source).Dispose();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        #region Properties

        /// <summary>
        ///     Gets or sets the currently displayed icon as either a <see cref="MaterialIconKind" /> or an <see cref="Uri" />
        ///     pointing to an SVG
        /// </summary>
        public static readonly StyledProperty<object?> IconProperty =
            AvaloniaProperty.Register<ArtemisIcon, object?>(nameof(Icon), notifying: IconChanging);


        /// <summary>
        ///     Gets or sets the currently displayed icon as either a <see cref="MaterialIconKind" /> or an <see cref="Uri" />
        ///     pointing to an SVG
        /// </summary>
        public object? Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        #endregion
    }
}