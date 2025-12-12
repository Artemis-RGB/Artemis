using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
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