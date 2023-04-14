using System;
using System.IO;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins.Help;

public class MarkdownPluginHelpPageViewModel : ActivatableViewModelBase, IPluginHelpPage
{
    private readonly MarkdownPluginHelpPage _helpPage;
    private string? _markdownText;

    public MarkdownPluginHelpPageViewModel(MarkdownPluginHelpPage helpPage)
    {
        _helpPage = helpPage;
        this.WhenActivated(d =>
        {
            FileSystemWatcher watcher = new();
            watcher.Path = Path.GetDirectoryName(_helpPage.MarkdownFile) ?? throw new InvalidOperationException($"Path \"{_helpPage.MarkdownFile}\" does not contain a directory");
            watcher.Filter = Path.GetFileName(_helpPage.MarkdownFile);
            watcher.EnableRaisingEvents = true;
            watcher.Changed += WatcherOnChanged;
            watcher.DisposeWith(d);
            
            LoadMarkdown().DisposeWith(d);
        });
    }

    public string? MarkdownText
    {
        get => _markdownText;
        set => RaiseAndSetIfChanged(ref _markdownText, value);
    }

    private async void WatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        await LoadMarkdown();
    }

    private async Task LoadMarkdown()
    {
        try
        {
            await using FileStream stream = new(_helpPage.MarkdownFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using StreamReader reader = new(stream);
            MarkdownText = await reader.ReadToEndAsync();
        }
        catch (Exception e)
        {
            MarkdownText = e.Message;
        }
    }

    /// <inheritdoc />
    public Plugin Plugin => _helpPage.Plugin;

    /// <inheritdoc />
    public string Title => _helpPage.Title;

    /// <inheritdoc />
    public string Id => _helpPage.Id;
}