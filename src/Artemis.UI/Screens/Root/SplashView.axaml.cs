using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Avalonia;
using ReactiveUI.Avalonia;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.Screens.Root;

public partial class SplashView : ReactiveWindow<SplashViewModel>
{
    public SplashView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif
        this.WhenActivated(disposables =>
        {
            SplashViewModel vm = ViewModel!;
            Observable.FromEventPattern(x => vm.CoreService.Initialized += x, x => vm.CoreService.Initialized -= x)
                .Subscribe(_ => Dispatcher.UIThread.Post(Close))
                .DisposeWith(disposables);
        });
    }

}