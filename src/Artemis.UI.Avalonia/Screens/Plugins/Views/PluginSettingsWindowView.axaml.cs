using System;
using Artemis.UI.Avalonia.Screens.Plugins.ViewModels;
using Avalonia;
using Avalonia.Controls.Mixins;
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

            this.WhenActivated(disposables => { ViewModel!.ConfigurationViewModel.Close.Subscribe(_ => Close()).DisposeWith(disposables); });
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}