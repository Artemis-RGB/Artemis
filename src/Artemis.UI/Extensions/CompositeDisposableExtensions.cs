using System.Reactive.Disposables;
using System.Reactive.Disposables.Fluent;
using System.Threading;

namespace Artemis.UI.Extensions;

public static class CompositeDisposableExtensions
{
    public static CancellationToken AsCancellationToken(this CompositeDisposable disposable)
    {
        CancellationTokenSource tokenSource = new();
        Disposable.Create(tokenSource, s => s.Cancel()).DisposeWith(disposable);
        return tokenSource.Token;
    }
}