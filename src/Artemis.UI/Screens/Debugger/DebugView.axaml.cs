using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger;

public class DebugView : ReactiveAppWindow<DebugViewModel>
{
    public DebugView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern(x => ViewModel!.ActivationRequested += x, x => ViewModel!.ActivationRequested -= x).Subscribe(_ =>
            {
                WindowState = WindowState.Normal;
                Activate();
            }).DisposeWith(d);
        });
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    private void DeviceVisualizer_OnLedClicked(object? sender, LedClickedEventArgs e)
    {
    }
}