using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.UI.Shared;
using Avalonia;
using Avalonia.Markup.Xaml;
using ReactiveUI;

namespace Artemis.UI.Screens.Plugins;

public partial class PluginSettingsWindowView : ReactiveAppWindow<PluginSettingsWindowViewModel>
{
    public PluginSettingsWindowView()
    {
        InitializeComponent();
#if DEBUG
        this.AttachDevTools();
#endif

        this.WhenActivated(disposables =>
            {
                PluginSettingsWindowViewModel vm = ViewModel!;
                Observable.FromEventPattern(x => vm.ConfigurationViewModel.CloseRequested += x, x => vm.ConfigurationViewModel.CloseRequested -= x)
                    .Subscribe(_ => Close())
                    .DisposeWith(disposables);
            }
        );
    }
}