using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins.Help;

public class MarkdownPluginHelpPageViewModel : ActivatableViewModelBase, IPluginHelpPage
{
    private string? _markdownText;
    private readonly MarkdownPluginHelpPage _helpPage;

    public MarkdownPluginHelpPageViewModel(MarkdownPluginHelpPage helpPage)
    {
        _helpPage = helpPage;
        this.WhenActivated(d => Load().DisposeWith(d));
    }

    public string? MarkdownText
    {
        get => _markdownText;
        set => RaiseAndSetIfChanged(ref _markdownText, value);
    }

    /// <inheritdoc />
    public Plugin Plugin => _helpPage.Plugin;

    /// <inheritdoc />
    public string Title => _helpPage.Title;

    /// <inheritdoc />
    public string Id => _helpPage.Id;
    
    private async Task Load()
    {
        MarkdownText ??= await File.ReadAllTextAsync(_helpPage.MarkdownFile);
    }
}