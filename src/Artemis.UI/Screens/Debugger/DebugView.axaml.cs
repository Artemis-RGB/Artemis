using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger
{
    public class DebugView : ReactiveCoreWindow<DebugViewModel>
    {
        public DebugView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            NavigationView navigation = this.Get<NavigationView>("Navigation");

            this.WhenActivated(d =>
            {
                Observable.FromEventPattern(x => ViewModel!.ActivationRequested += x, x => ViewModel!.ActivationRequested -= x).Subscribe(_ =>
                {
                    WindowState = WindowState.Normal;
                    Activate();
                    
                }).DisposeWith(d);
                ViewModel!.SelectedItem = (NavigationViewItem) navigation.MenuItems.ElementAt(0);
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
}