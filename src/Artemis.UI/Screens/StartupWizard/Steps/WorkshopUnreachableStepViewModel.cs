using Artemis.Core;
using Artemis.Core.Services;

namespace Artemis.UI.Screens.StartupWizard.Steps;

public class WorkshopUnreachableStepViewModel : WizardStepViewModel
{
    private readonly ISettingsService _settingsService;

    public WorkshopUnreachableStepViewModel(ISettingsService settingsService)
    {
        _settingsService = settingsService;
        HideAllButtons = true;
    }

    public void Retry()
    {
        Wizard.ChangeScreen<DefaultEntriesStepViewModel>();
    }

    public void Skip()
    {
        PluginSetting<bool> setting = _settingsService.GetSetting("UI.SetupWizardCompleted", false);
        setting.Value = false;
        setting.Save();
        
        Wizard.Close(false);
    }
}