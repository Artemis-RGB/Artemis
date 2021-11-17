using System;
using System.Reactive.Disposables;
using Artemis.UI.Shared.Events;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;
using ReactiveUI;

namespace Artemis.UI.Screens.Debugger
{
    public class DebugView : ReactiveWindow<DebugViewModel>
    {
        private readonly NavigationView _navigation;

        public DebugView()
        {
            Activated += OnActivated;
            Deactivated += OnDeactivated;
            InitializeComponent();

            _navigation = this.Get<NavigationView>("Navigation");
            this.WhenActivated(d =>
            {
                ViewModel!.WhenAnyValue(vm => vm!.IsActive).Subscribe(_ => Activate()).DisposeWith(d);
                ViewModel!.SelectedItem = (NavigationViewItem) _navigation.MenuItems.ElementAt(0);
            });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void OnDeactivated(object? sender, EventArgs e)
        {
            if (ViewModel != null)
                ViewModel.IsActive = false;
        }

        private void OnActivated(object? sender, EventArgs e)
        {
            if (ViewModel != null)
                ViewModel.IsActive = true;
        }

        private void DeviceVisualizer_OnLedClicked(object? sender, LedClickedEventArgs e)
        {
            
        }
    }
}