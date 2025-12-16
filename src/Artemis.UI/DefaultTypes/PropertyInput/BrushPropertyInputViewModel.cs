using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables.Fluent;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.Services;
using Artemis.UI.Screens.ProfileEditor.Properties.Tree.Dialogs;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Builders;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.ProfileEditor.Commands;
using Artemis.UI.Shared.Services.PropertyInput;
using Avalonia.Threading;
using ReactiveUI;

namespace Artemis.UI.DefaultTypes.PropertyInput;

public class BrushPropertyInputViewModel : PropertyInputViewModel<LayerBrushReference>
{
    private readonly IPluginManagementService _pluginManagementService;
    private readonly IProfileEditorService _profileEditorService;
    private readonly IWindowService _windowService;
    private ObservableCollection<LayerBrushDescriptor> _descriptors;

    public BrushPropertyInputViewModel(LayerProperty<LayerBrushReference> layerProperty, IPluginManagementService pluginManagementService, IWindowService windowService,
        IProfileEditorService profileEditorService, IPropertyInputService propertyInputService)
        : base(layerProperty, profileEditorService, propertyInputService)
    {
        _pluginManagementService = pluginManagementService;
        _windowService = windowService;
        _profileEditorService = profileEditorService;
        _descriptors = new ObservableCollection<LayerBrushDescriptor>(pluginManagementService.GetFeaturesOfType<LayerBrushProvider>().SelectMany(l => l.LayerBrushDescriptors));

        this.WhenActivated(d =>
        {
            Observable.FromEventPattern<PluginFeatureEventArgs>(x => pluginManagementService.PluginFeatureEnabled += x, x => pluginManagementService.PluginFeatureEnabled -= x)
                .Subscribe(e => UpdateDescriptorsIfChanged(e.EventArgs))
                .DisposeWith(d);
            Observable.FromEventPattern<PluginFeatureEventArgs>(x => pluginManagementService.PluginFeatureDisabled += x, x => pluginManagementService.PluginFeatureDisabled -= x)
                .Subscribe(e => UpdateDescriptorsIfChanged(e.EventArgs))
                .DisposeWith(d);
        });
    }

    public ObservableCollection<LayerBrushDescriptor> Descriptors
    {
        get => _descriptors;
        set => this.RaiseAndSetIfChanged(ref _descriptors, value);
    }

    public LayerBrushDescriptor? SelectedDescriptor
    {
        get => Descriptors.FirstOrDefault(d => d.MatchesLayerBrushReference(InputValue));
        set => SetBrushByDescriptor(value);
    }

    /// <inheritdoc />
    protected override void ApplyInputValue()
    {
        if (LayerProperty.ProfileElement is not Layer layer || SelectedDescriptor == null)
            return;

        _profileEditorService.ExecuteCommand(new ChangeLayerBrush(layer, SelectedDescriptor));
        if (layer.LayerBrush?.Presets != null && layer.LayerBrush.Presets.Any())
            Dispatcher.UIThread.InvokeAsync(() => _windowService.CreateContentDialog()
                .WithTitle("Select preset")
                .WithViewModel(out LayerBrushPresetViewModel _, layer.LayerBrush)
                .WithDefaultButton(ContentDialogButton.Close)
                .WithCloseButtonText("Use defaults")
                .ShowAsync());
    }

    #region Overrides of PropertyInputViewModel<LayerBrushReference>

    /// <inheritdoc />
    protected override void OnInputValueChanged()
    {
        this.RaisePropertyChanged(nameof(SelectedDescriptor));
    }

    #endregion

    private void UpdateDescriptorsIfChanged(PluginFeatureEventArgs e)
    {
        if (e.PluginFeature is not LayerBrushProvider)
            return;

        Descriptors = new ObservableCollection<LayerBrushDescriptor>(_pluginManagementService.GetFeaturesOfType<LayerBrushProvider>().SelectMany(l => l.LayerBrushDescriptors));
    }

    private void SetBrushByDescriptor(LayerBrushDescriptor? value)
    {
        if (value != null)
            InputValue = new LayerBrushReference(value);
    }
}