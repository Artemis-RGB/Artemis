using System;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger;

public partial class DebugView : ReactiveAppWindow<DebugViewModel>
{
    public DebugView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        this.WhenActivated(d =>
        {
            DebugViewModel vm = ViewModel!;
            Observable.FromEventPattern(x => vm.ActivationRequested += x, x => vm.ActivationRequested -= x)
                .Subscribe(_ =>
                {
                    WindowState = WindowState.Normal;
                    Activate();
                })
                .DisposeWith(d);
        });
    }


    private void DeviceVisualizer_OnLedClicked(object? sender, LedClickedEventArgs e)
    {
    }
}