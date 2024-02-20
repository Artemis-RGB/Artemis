using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.DryIoc.Factories;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Screens.Plugins.Features;
using Artemis.UI.Shared;
using ReactiveUI;

namespace Artemis.UI.Screens.Workshop.Plugins.Dialogs;

public class PluginDialogViewModel : ContentDialogViewModelBase
{
    public PluginDialogViewModel(Plugin plugin, ISettingsVmFactory settingsVmFactory)
    {
        PluginViewModel = settingsVmFactory.PluginViewModel(plugin, ReactiveCommand.Create(() => {}, Observable.Empty<bool>()));
        PluginFeatures = new ObservableCollection<PluginFeatureViewModel>(plugin.Features.Select(f => settingsVmFactory.PluginFeatureViewModel(f, false)));
    }

    public PluginViewModel PluginViewModel { get; }
    public ObservableCollection<PluginFeatureViewModel> PluginFeatures { get; }
}