using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Screens.Modules;
using Artemis.UI.Screens.Modules.Tabs;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.ProfileEditor.DisplayConditions;
using Artemis.UI.Screens.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.LayerEffects;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Screens.ProfileEditor.Visualization;
using Artemis.UI.Screens.ProfileEditor.Visualization.Tools;
using Artemis.UI.Screens.Settings.Debug;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Stylet;

namespace Artemis.UI.Ninject.Factories
{
    public interface IVmFactory
    {
    }

    public interface IModuleVmFactory : IVmFactory
    {
        ModuleRootViewModel CreateModuleRootViewModel(Module module);
        ProfileEditorViewModel CreateProfileEditorViewModel(ProfileModule module);
        ActivationRequirementsViewModel CreateActivationRequirementsViewModel(Module module);
        ActivationRequirementViewModel CreateActivationRequirementViewModel(IModuleActivationRequirement activationRequirement);
    }

    public interface ISettingsVmFactory : IVmFactory
    {
        PluginSettingsViewModel CreatePluginSettingsViewModel(Plugin plugin);
        DeviceSettingsViewModel CreateDeviceSettingsViewModel(ArtemisDevice device);
    }

    public interface IDeviceDebugVmFactory : IVmFactory
    {
        DeviceDebugViewModel Create(ArtemisDevice device);
    }

    public interface IProfileTreeVmFactory : IVmFactory
    {
        FolderViewModel FolderViewModel(ProfileElement folder);
        LayerViewModel LayerViewModel(ProfileElement layer);
    }

    public interface IProfileLayerVmFactory : IVmFactory
    {
        ProfileLayerViewModel Create(Layer layer, ProfileViewModel profileViewModel);
    }

    public interface IVisualizationToolVmFactory : IVmFactory
    {
        ViewpointMoveToolViewModel ViewpointMoveToolViewModel(ProfileViewModel profileViewModel);
        EditToolViewModel EditToolViewModel(ProfileViewModel profileViewModel);
        SelectionToolViewModel SelectionToolViewModel(ProfileViewModel profileViewModel);
        SelectionRemoveToolViewModel SelectionRemoveToolViewModel(ProfileViewModel profileViewModel);
    }

    public interface IDisplayConditionsVmFactory : IVmFactory
    {
        DisplayConditionGroupViewModel DisplayConditionGroupViewModel(DisplayConditionGroup displayConditionGroup, bool isListGroup);
        DisplayConditionListViewModel DisplayConditionListViewModel(DisplayConditionList displayConditionList);
        DisplayConditionPredicateViewModel DisplayConditionPredicateViewModel(DisplayConditionPredicate displayConditionPredicate);
        DisplayConditionListPredicateViewModel DisplayConditionListPredicateViewModel(DisplayConditionListPredicate displayConditionListPredicate);
    }

    public interface ILayerPropertyVmFactory : IVmFactory
    {
        LayerPropertyViewModel LayerPropertyViewModel(ILayerProperty layerProperty);

        LayerPropertyGroupViewModel LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup);
        TreeGroupViewModel TreeGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel);
        TimelineGroupViewModel TimelineGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel);

        TreeViewModel TreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
        EffectsViewModel EffectsViewModel(LayerPropertiesViewModel layerPropertiesViewModel);
        TimelineViewModel TimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
        TimelineSegmentViewModel TimelineSegmentViewModel(SegmentViewModelType segment, BindableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
    }

    public interface IDataBindingsVmFactory
    {
        IDataBindingViewModel DataBindingViewModel(IDataBindingRegistration registration);
        DataBindingModifierViewModel<TLayerProperty, TProperty> DataBindingModifierViewModel<TLayerProperty, TProperty>(DataBindingModifier<TLayerProperty, TProperty> modifier);
    }

    public interface IPropertyVmFactory
    {
        ITreePropertyViewModel TreePropertyViewModel(ILayerProperty layerProperty, LayerPropertyViewModel layerPropertyViewModel);
        ITimelinePropertyViewModel TimelinePropertyViewModel(ILayerProperty layerProperty, LayerPropertyViewModel layerPropertyViewModel);
    }
}