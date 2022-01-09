using System;
using Artemis.UI.Shared;
using Artemis.UI.Shared.LayerEffects;
using Avalonia.Threading;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Windows;

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

    public bool CanClose()
    {
        return ConfigurationViewModel.CanClose() && Dispatcher.UIThread.InvokeAsync(async () => await ConfigurationViewModel.CanCloseAsync()).GetAwaiter().GetResult();
    }
    
    private void ConfigurationViewModelOnCloseRequested(object? sender, EventArgs e)
    {
        if (CanClose())
            Close(null);
    }
}