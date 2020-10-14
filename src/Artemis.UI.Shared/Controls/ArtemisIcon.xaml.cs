using System;
using System.Windows;
using System.Windows.Controls;
using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Shared.Controls
{
    /// <summary>
    ///     Interaction logic for ArtemisIcon.xaml
    /// </summary>
    public partial class ArtemisIcon : UserControl
    {
        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(nameof(Icon), typeof(object), typeof(ArtemisIcon),
            new FrameworkPropertyMetadata(IconPropertyChangedCallback));

        public static readonly DependencyProperty PackIconProperty = DependencyProperty.Register(nameof(PackIcon), typeof(PackIconKind?), typeof(ArtemisIcon),
            new FrameworkPropertyMetadata(IconPropertyChangedCallback));

        public static readonly DependencyProperty SvgSourceProperty = DependencyProperty.Register(nameof(SvgSource), typeof(Uri), typeof(ArtemisIcon),
            new FrameworkPropertyMetadata(IconPropertyChangedCallback));

        private bool _inCallback;

        public ArtemisIcon()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     Gets or sets the currently displayed icon as either a <see cref="PackIconKind" /> or an <see cref="Uri" /> pointing
        ///     to an SVG
        /// </summary>
        public object Icon
        {
            get => GetValue(IconProperty);
            set => SetValue(IconProperty, value);
        }

        /// <summary>
        ///     Gets or sets the <see cref="PackIconKind" />
        /// </summary>
        public PackIconKind? PackIcon
        {
            get => (PackIconKind?) GetValue(PackIconProperty);
            set => SetValue(PackIconProperty, value);
        }

        /// <summary>
        ///     Gets or sets the <see cref="Uri" /> pointing to the SVG
        /// </summary>
        public Uri SvgSource
        {
            get => (Uri) GetValue(SvgSourceProperty);
            set => SetValue(SvgSourceProperty, value);
        }

        private static void IconPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ArtemisIcon artemisIcon = (ArtemisIcon) d;
            if (artemisIcon._inCallback)
                return;

            try
            {
                artemisIcon._inCallback = true;
                if (artemisIcon.PackIcon != null)
                {
                    artemisIcon.Icon = artemisIcon.PackIcon;
                }
                else if (artemisIcon.SvgSource != null)
                {
                    artemisIcon.Icon = artemisIcon.SvgSource;
                }
                else if (artemisIcon.Icon is string iconString)
                {
                    if (Uri.TryCreate(iconString, UriKind.Absolute, out Uri uriResult))
                        artemisIcon.Icon = uriResult;
                    else if (Enum.TryParse(typeof(PackIconKind), iconString, true, out object result))
                        artemisIcon.Icon = result;
                }
            }
            finally
            {
                artemisIcon._inCallback = false;
            }
        }
    }
}