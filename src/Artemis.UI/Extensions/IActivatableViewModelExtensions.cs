using System;
using System.Reactive.Disposables;
using System.Threading.Tasks;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Extensions;

public static class ActivatableViewModelExtensions
{
    public static void WhenActivatedAsync(this IActivatableViewModel item, Func<CompositeDisposable, Task> block)
    {
        item.WhenActivated(d => Dispatcher.UIThread.InvokeAsync(async () => await block(d)));
    }
}