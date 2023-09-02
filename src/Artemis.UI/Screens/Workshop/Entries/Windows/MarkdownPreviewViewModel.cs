using System;
using Artemis.UI.Shared;

namespace Artemis.UI.Screens.Workshop.Entries.Windows;

public class MarkdownPreviewViewModel : ActivatableViewModelBase
{
    public event EventHandler? Closed;
    
    public IObservable<string> Markdown { get; }

    public MarkdownPreviewViewModel(IObservable<string> markdown)
    {
        Markdown = markdown;
    }

    protected virtual void OnClosed()
    {
        Closed?.Invoke(this, EventArgs.Empty);
    }
}