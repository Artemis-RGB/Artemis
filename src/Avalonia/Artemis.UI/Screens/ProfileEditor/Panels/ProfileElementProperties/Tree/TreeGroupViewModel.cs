using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reflection;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.UI.Exceptions;
using Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Windows;
using Artemis.UI.Shared;
using Artemis.UI.Shared.LayerBrushes;
using Artemis.UI.Shared.LayerEffects;
using Artemis.UI.Shared.Services.Interfaces;
using Artemis.UI.Shared.Services.ProfileEditor;
using Ninject;
using Ninject.Parameters;
using ReactiveUI;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties.Tree;

public class TreeGroupViewModel : ActivatableViewModelBase
{
    private readonly IProfileEditorService _profileEditorService;
    private readonly IWindowService _windowService;
    private BrushConfigurationWindowViewModel? _brushConfigurationWindowViewModel;
    private EffectConfigurationWindowViewModel? _effectConfigurationWindowViewModel;

    public TreeGroupViewModel(ProfileElementPropertyGroupViewModel profileElementPropertyGroupViewModel, IWindowService windowService, IProfileEditorService profileEditorService)
    {
        _windowService = windowService;
        _profileEditorService = profileEditorService;
        ProfileElementPropertyGroupViewModel = profileElementPropertyGroupViewModel;
        DetermineGroupType();

        this.WhenActivated(d =>
        {
            ProfileElementPropertyGroupViewModel.WhenAnyValue(vm => vm.IsExpanded).Subscribe(_ => this.RaisePropertyChanged(nameof(Children))).DisposeWith(d);
            Disposable.Create(CloseViewModels).DisposeWith(d);
        });

        // TODO: Update ProfileElementPropertyGroupViewModel visibility on change (can remove the sub on line 41 as well then)
    }


    public ProfileElementPropertyGroupViewModel ProfileElementPropertyGroupViewModel { get; }
    public LayerPropertyGroup LayerPropertyGroup => ProfileElementPropertyGroupViewModel.LayerPropertyGroup;
    public ObservableCollection<ViewModelBase>? Children => ProfileElementPropertyGroupViewModel.IsExpanded ? ProfileElementPropertyGroupViewModel.Children : null;

    public LayerPropertyGroupType GroupType { get; private set; }

    public async Task OpenBrushSettings()
    {
        BaseLayerBrush? layerBrush = LayerPropertyGroup.LayerBrush;
        if (layerBrush?.ConfigurationDialog is not LayerBrushConfigurationDialog configurationViewModel)
            return;

        try
        {
            // Limit to one constructor, there's no need to have more and it complicates things anyway
            ConstructorInfo[] constructors = configurationViewModel.Type.GetConstructors();
            if (constructors.Length != 1)
                throw new ArtemisUIException("Brush configuration dialogs must have exactly one constructor");

            // Find the BaseLayerBrush parameter, it is required by the base constructor so its there for sure
            ParameterInfo brushParameter = constructors.First().GetParameters().First(p => typeof(BaseLayerBrush).IsAssignableFrom(p.ParameterType));
            ConstructorArgument argument = new(brushParameter.Name!, layerBrush);
            BrushConfigurationViewModel viewModel = (BrushConfigurationViewModel) layerBrush.Descriptor.Provider.Plugin.Kernel!.Get(configurationViewModel.Type, argument);

            _brushConfigurationWindowViewModel = new BrushConfigurationWindowViewModel(viewModel, configurationViewModel);
            await _windowService.ShowDialogAsync(_brushConfigurationWindowViewModel);

            // Save changes after the dialog closes
            await _profileEditorService.SaveProfileAsync();
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("An exception occurred while trying to show the brush's settings window", e);
        }
    }

    public async Task OpenEffectSettings()
    {
        BaseLayerEffect? layerEffect = LayerPropertyGroup.LayerEffect;
        if (layerEffect?.ConfigurationDialog is not LayerEffectConfigurationDialog configurationViewModel)
            return;

        try
        {
            // Limit to one constructor, there's no need to have more and it complicates things anyway
            ConstructorInfo[] constructors = configurationViewModel.Type.GetConstructors();
            if (constructors.Length != 1)
                throw new ArtemisUIException("Effect configuration dialogs must have exactly one constructor");

            // Find the BaseLayerEffect parameter, it is required by the base constructor so its there for sure
            ParameterInfo effectParameter = constructors.First().GetParameters().First(p => typeof(BaseLayerEffect).IsAssignableFrom(p.ParameterType));
            ConstructorArgument argument = new(effectParameter.Name!, layerEffect);
            EffectConfigurationViewModel viewModel = (EffectConfigurationViewModel) layerEffect.Descriptor.Provider.Plugin.Kernel!.Get(configurationViewModel.Type, argument);

            _effectConfigurationWindowViewModel = new EffectConfigurationWindowViewModel(viewModel, configurationViewModel);
            await _windowService.ShowDialogAsync(_effectConfigurationWindowViewModel);

            // Save changes after the dialog closes
            await _profileEditorService.SaveProfileAsync();
        }
        catch (Exception e)
        {
            _windowService.ShowExceptionDialog("An exception occurred while trying to show the effect's settings window", e);
        }
    }

    public async Task RenameEffect()
    {
        await _windowService.ShowConfirmContentDialog("Not yet implemented", "Try again later :p");
    }

    public async Task DeleteEffect()
    {
        await _windowService.ShowConfirmContentDialog("Not yet implemented", "Try again later :p");
    }

    public double GetDepth()
    {
        int depth = 0;
        LayerPropertyGroup? current = LayerPropertyGroup.Parent;
        while (current != null)
        {
            depth++;
            current = current.Parent;
        }

        return depth;
    }

    private void CloseViewModels()
    {
        _effectConfigurationWindowViewModel?.Close(null);
        _brushConfigurationWindowViewModel?.Close(null);
    }

    private void DetermineGroupType()
    {
        if (LayerPropertyGroup is LayerGeneralProperties)
            GroupType = LayerPropertyGroupType.General;
        else if (LayerPropertyGroup is LayerTransformProperties)
            GroupType = LayerPropertyGroupType.Transform;
        else if (LayerPropertyGroup.Parent == null && LayerPropertyGroup.LayerBrush != null)
            GroupType = LayerPropertyGroupType.LayerBrushRoot;
        else if (LayerPropertyGroup.Parent == null && LayerPropertyGroup.LayerEffect != null)
            GroupType = LayerPropertyGroupType.LayerEffectRoot;
        else
            GroupType = LayerPropertyGroupType.None;
    }
}

public enum LayerPropertyGroupType
{
    General,
    Transform,
    LayerBrushRoot,
    LayerEffectRoot,
    None
}