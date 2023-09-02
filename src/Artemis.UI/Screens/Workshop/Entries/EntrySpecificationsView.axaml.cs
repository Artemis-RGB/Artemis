using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Immutable;
using Avalonia.ReactiveUI;
using AvaloniaEdit.TextMate;
using TextMateSharp.Grammars;

namespace Artemis.UI.Screens.Workshop.Entries;

public partial class EntrySpecificationsView : ReactiveUserControl<EntrySpecificationsViewModel>
{
    public EntrySpecificationsView()
    {
        InitializeComponent();

        RegistryOptions options = new(ThemeName.Dark);
        TextMate.Installation? install = DescriptionEditor.InstallTextMate(options);

        install.SetGrammar(options.GetScopeByExtension(".md"));
    }

    #region Overrides of Visual

    /// <inheritdoc />
    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (this.TryFindResource("SystemAccentColorLight3", out object? resource) && resource is Color color)
            DescriptionEditor.TextArea.TextView.LinkTextForegroundBrush = new ImmutableSolidColorBrush(color);
        base.OnAttachedToVisualTree(e);
    }

    #endregion
}