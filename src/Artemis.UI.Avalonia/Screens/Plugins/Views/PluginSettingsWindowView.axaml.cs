using System;
using System.Reactive.Disposables;
using Artemis.UI.Avalonia.Screens.Plugins.ViewModels;
using Avalonia;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using ReactiveUI;

namespace Artemis.UI.Avalonia.Screens.Plugins.Views
{
    public class PluginSettingsWindowView : ReactiveWindow<PluginSettingsWindowViewModel>
    {
        public PluginSettingsWindowView()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif

            this.WhenActivated(disposables =>
                {
                    ViewModel!.ConfigurationViewModel.CloseRequested += ConfigurationViewModelOnCloseRequested;
                    Disposable.Create(HandleDeactivation).DisposeWith(disposables);
                }
            );
        }

        private void HandleDeactivation()
        {
            ViewModel!.ConfigurationViewModel.CloseRequested -= ConfigurationViewModelOnCloseRequested;
        }

        private void ConfigurationViewModelOnCloseRequested(object? sender, EventArgs e)
        {
            Close();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}