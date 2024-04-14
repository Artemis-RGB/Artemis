using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Data;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;
using VisualExtensions = Artemis.UI.Shared.Extensions.VisualExtensions;

namespace Artemis.UI.Controls;

public partial class SplitMarkdownEditor : UserControl
{
    public static readonly StyledProperty<string> TitleProperty = AvaloniaProperty.Register<SplitMarkdownEditor, string>(nameof(Title), string.Empty);
    public static readonly StyledProperty<string> MarkdownProperty = AvaloniaProperty.Register<SplitMarkdownEditor, string>(nameof(Markdown), string.Empty, defaultBindingMode: BindingMode.TwoWay);

    private ScrollViewer? _editorScrollViewer;
    private ScrollViewer? _previewScrollViewer;
    private bool _scrolling;
    private bool _updating;

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Markdown
    {
        get => GetValue(MarkdownProperty);
        set => SetValue(MarkdownProperty, value);
    }

    public SplitMarkdownEditor()
    {
        InitializeComponent();
        PropertyChanged += OnPropertyChanged;

        DescriptionEditorLabel.Content = Title;
        DescriptionEditor.Options.AllowScrollBelowDocument = false;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (this.TryFindResource("SystemAccentColorLight3", out object? resource) && resource is Color color)
            DescriptionEditor.TextArea.TextView.LinkTextForegroundBrush = new ImmutableSolidColorBrush(color);

        SetupScrollSync();
        
        Dispatcher.UIThread.InvokeAsync(async () =>
        {
            // Installing is slow, wait for UI to settle
            await Task.Delay(300);
            
            RegistryOptions options = new(ThemeName.Dark);
            TextMate.Installation? install = DescriptionEditor.InstallTextMate(options);
            install.SetGrammar(options.GetScopeByExtension(".md"));
        }, DispatcherPriority.ApplicationIdle);
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
        if (e.Property.Name != nameof(ScrollViewer.Offset) || _scrolling || SynchronizedScrolling.IsChecked != true)
            return;

        try
        {
            _scrolling = true;
            SynchronizeScrollViewers(_editorScrollViewer, _previewScrollViewer);
        }
        finally
        {
            _scrolling = false;
        }
    }

    private void PreviewScrollViewerOnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property.Name != nameof(ScrollViewer.Offset) || _scrolling || SynchronizedScrolling.IsChecked != true)
            return;

        try
        {
            _scrolling = true;
            SynchronizeScrollViewers(_previewScrollViewer, _editorScrollViewer);
        }
        finally
        {
            _scrolling = false;
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

    private void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == TitleProperty)
            DescriptionEditorLabel.Content = Title;
        else if (e.Property == MarkdownProperty && DescriptionEditor.Text != Markdown)
        {
            try
            {
                _updating = true;
                DescriptionEditor.Clear();
                DescriptionEditor.AppendText(Markdown);
            }
            finally
            {
                _updating = false;
            }
            
        }
    }
    
    private void DescriptionEditor_OnTextChanged(object? sender, EventArgs e)
    {
        if (!_updating && Markdown != DescriptionEditor.Text)
            Markdown = DescriptionEditor.Text;
    }
}