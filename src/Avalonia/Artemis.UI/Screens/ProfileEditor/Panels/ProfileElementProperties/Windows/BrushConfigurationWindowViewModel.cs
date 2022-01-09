using System;
using Artemis.UI.Shared;
using Artemis.UI.Shared.LayerBrushes;
using Artemis.UI.Shared.LayerEffects;
using Avalonia.Threading;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Windows;

public class BrushConfigurationWindowViewModel : DialogViewModelBase<object?>
{
    public BrushConfigurationWindowViewModel(BrushConfigurationViewModel configurationViewModel, LayerBrushConfigurationDialog configuration)
    {
        ConfigurationViewModel = configurationViewModel;
        Configuration = configuration;
        
        ConfigurationViewModel.CloseRequested += ConfigurationViewModelOnCloseRequested;
    }

    public BrushConfigurationViewModel ConfigurationViewModel { get; }
    public LayerBrushConfigurationDialog Configuration { get; }

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