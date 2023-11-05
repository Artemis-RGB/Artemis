using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.ReactiveUI;
using AvaloniaEdit.TextMate;
using ReactiveUI;
using TextMateSharp.Grammars;
using VisualExtensions = Artemis.UI.Shared.Extensions.VisualExtensions;

namespace Artemis.UI.Screens.Workshop.Entries.Details;

public partial class EntrySpecificationsView : ReactiveUserControl<EntrySpecificationsViewModel>
{
    private ScrollViewer? _editorScrollViewer;
    private ScrollViewer? _previewScrollViewer;
    private bool _updating;

    public EntrySpecificationsView()
    {
        InitializeComponent();

        DescriptionEditor.Options.AllowScrollBelowDocument = false;
        RegistryOptions options = new(ThemeName.Dark);
        TextMate.Installation? install = TextMate.InstallTextMate(DescriptionEditor, options);

        install.SetGrammar(options.GetScopeByExtension(".md"));

        this.WhenActivated(_ => SetupScrollSync());
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (this.TryFindResource("SystemAccentColorLight3", out object? resource) && resource is Color color)
            DescriptionEditor.TextArea.TextView.LinkTextForegroundBrush = new ImmutableSolidColorBrush(color);

        base.OnAttachedToVisualTree(e);
    }

    private void SetupScrollSync()
    {
        if (_editorScrollViewer != null)
            _editorScrollViewer.PropertyChanged -= EditorScrollViewerOnPropertyChanged;
        if (_previewScrollViewer != null)
            _previewScrollViewer.PropertyChanged -= PreviewScrollViewerOnPropertyChanged;

        _editorScrollViewer = VisualExtensions.GetVisualChildrenOfType<ScrollViewer>(DescriptionEditor).FirstOrDefault();
        _previewScrollViewer = VisualExtensions.GetVisualChildrenOfType<ScrollViewer>(DescriptionPreview).FirstOrDefault();

        if (_editorScrollViewer != null)
            _editorScrollViewer.PropertyChanged += EditorScrollViewerOnPropertyChanged;
        if (_previewScrollViewer != null)
            _previewScrollViewer.PropertyChanged += PreviewScrollViewerOnPropertyChanged;
    }

    private void EditorScrollViewerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(ScrollViewer.Offset) || _updating || SynchronizedScrolling.IsChecked != true)
            return;

        try
        {
            _updating = true;
            SynchronizeScrollViewers(_editorScrollViewer, _previewScrollViewer);
        }
        finally
        {
            _updating = false;
        }
    }

    private void PreviewScrollViewerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(ScrollViewer.Offset) || _updating || SynchronizedScrolling.IsChecked != true)
            return;

        try
        {
            _updating = true;
            SynchronizeScrollViewers(_previewScrollViewer, _editorScrollViewer);
        }
        finally
        {
            _updating = false;
        }
    }

    private void SynchronizeScrollViewers(ScrollViewer? source, ScrollViewer? target)
    {
        if (source == null || target == null)
            return;

        double sourceScrollableHeight = source.Extent.Height - source.Viewport.Height;
        double targetScrollableHeight = target.Extent.Height - target.Viewport.Height;

        if (sourceScrollableHeight != 0)
            target.Offset = new Vector(target.Offset.X, targetScrollableHeight * (source.Offset.Y / sourceScrollableHeight));
    }
}