using System;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.LayerEffects;

namespace Artemis.UI.Screens.Profile.ProfileEditor.Properties.Windows;

public class EffectConfigurationWindowViewModel : DialogViewModelBase<object?>
{
    public EffectConfigurationWindowViewModel(EffectConfigurationViewModel configurationViewModel, LayerEffectConfigurationDialog configuration)
    {
        ConfigurationViewModel = configurationViewModel;
        Configuration = configuration;

        ConfigurationViewModel.CloseRequested += ConfigurationViewModelOnCloseRequested;
    }

    public EffectConfigurationViewModel ConfigurationViewModel { get; }
    public LayerEffectConfigurationDialog Configuration { get; }

    public async Task<bool> CanClose()
    {
        // ReSharper disable once MethodHasAsyncOverload - Checking both in case the plugin developer only implemented CanClose
        return ConfigurationViewModel.CanClose() && await ConfigurationViewModel.CanCloseAsync();
    }

    private async void ConfigurationViewModelOnCloseRequested(object? sender, EventArgs e)
    {
        if (await CanClose())
            Close(null);
    }
}