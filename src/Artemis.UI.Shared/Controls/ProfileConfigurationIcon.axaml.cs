using System;
using System.IO;
using Artemis.Core;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Layout;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Material.Icons;
using Material.Icons.Avalonia;

namespace Artemis.UI.Shared;

/// <summary>
///     Represents a control that can display the icon of a specific <see cref="ProfileConfiguration" />.
/// </summary>
public partial class ProfileConfigurationIcon : UserControl, IDisposable
{
    private Stream? _stream;

    /// <summary>
    ///     Creates a new instance of the <see cref="ProfileConfigurationIcon" /> class.
    /// </summary>
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
        Dispose();

        try
        {
            if (ConfigurationIcon.IconType == ProfileConfigurationIconType.MaterialIcon)
            {
                Content = Enum.TryParse(ConfigurationIcon.IconName, true, out MaterialIconKind parsedIcon)
                    ? new MaterialIcon {Kind = parsedIcon!}
                    : new MaterialIcon {Kind = MaterialIconKind.QuestionMark};
            }
            else
            {
                Dispatcher.UIThread.Post(() =>
                {
                    Stream? stream = ConfigurationIcon.GetIconStream();
                    if (stream == null)
                        Content = new MaterialIcon {Kind = MaterialIconKind.QuestionMark};
                    else
                        LoadFromBitmap(ConfigurationIcon, stream);
                }, DispatcherPriority.ApplicationIdle);
            }
        }
        catch (Exception)
        {
            Content = new MaterialIcon {Kind = MaterialIconKind.QuestionMark};
        }
    }

    private void LoadFromBitmap(Core.ProfileConfigurationIcon configurationIcon, Stream stream)
    {
        _stream = stream;
        if (!configurationIcon.Fill)
        {
            Content = new Image {Source = new Bitmap(stream)};
            return;
        }

        Content = new Border
        {
            Background = TextElement.GetForeground(this),
            VerticalAlignment = VerticalAlignment.Stretch,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            OpacityMask = new ImageBrush(new Bitmap(stream))
        };
    }

    private void OnDetachedFromLogicalTree(object? sender, LogicalTreeAttachmentEventArgs e)
    {
        if (ConfigurationIcon != null)
            ConfigurationIcon.IconUpdated -= ConfigurationIconOnIconUpdated;

        Dispose();
    }

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != ConfigurationIconProperty)
            return;

        if (e.OldValue is Core.ProfileConfigurationIcon oldIcon)
            oldIcon.IconUpdated -= ConfigurationIconOnIconUpdated;
        if (e.NewValue is Core.ProfileConfigurationIcon newIcon)
            newIcon.IconUpdated += ConfigurationIconOnIconUpdated;

        Update();
    }

    private void ConfigurationIconOnIconUpdated(object? sender, EventArgs e)
    {
        Update();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Content is Image {Source: IDisposable d})
        {
            d.Dispose();
            Content = null;
        }

        _stream?.Dispose();
    }

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
}