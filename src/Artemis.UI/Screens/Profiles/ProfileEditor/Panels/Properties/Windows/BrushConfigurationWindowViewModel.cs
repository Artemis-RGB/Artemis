using System;
using System.Threading.Tasks;
using Artemis.UI.Shared;
using Artemis.UI.Shared.LayerBrushes;

namespace Artemis.UI.Screens.Profiles.ProfileEditor.Properties.Windows;

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