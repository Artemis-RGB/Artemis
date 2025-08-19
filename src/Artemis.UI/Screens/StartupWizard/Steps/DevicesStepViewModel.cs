using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Artemis.Core.DeviceProviders;
using Artemis.Core.Services;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public class DevicesStepViewModel : WizardStepViewModel
{
    public DevicesStepViewModel(IPluginManagementService pluginManagementService, Func<PluginFeatureInfo, WizardPluginFeatureViewModel> getPluginFeatureViewModel, IDeviceService deviceService)
    {
        // Take all compatible device providers and create a view model for them
        DeviceProviders = new ObservableCollection<WizardPluginFeatureViewModel>(pluginManagementService.GetAllPlugins()
            .Where(p => p.Info.IsCompatible)
            .SelectMany(p => p.Features.Where(f => f.FeatureType.IsAssignableTo(typeof(DeviceProvider))))
            .OrderBy(f => f.Name)
            .Select(getPluginFeatureViewModel));
        
        Continue = ReactiveCommand.Create(() =>
        {
            if (deviceService.EnabledDevices.Count == 0)
                Wizard.ChangeScreen<DefaultEntriesStepViewModel>();
            else
                Wizard.ChangeScreen<LayoutsStepViewModel>();
        });
        GoBack = ReactiveCommand.Create(() => Wizard.ChangeScreen<WelcomeStepViewModel>());
    }

    public ObservableCollection<WizardPluginFeatureViewModel> DeviceProviders { get; }
}