using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
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
                    Observable.FromEventPattern(
                            x => ViewModel!.ConfigurationViewModel.CloseRequested += x,
                            x => ViewModel!.ConfigurationViewModel.CloseRequested -= x
                        )
                        .Subscribe(_ => Close())
                        .DisposeWith(disposables);
                }
            );
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}