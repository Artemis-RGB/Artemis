using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Screens.Modules;
using Artemis.UI.Screens.Modules.Tabs;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.ProfileEditor.Conditions;
using Artemis.UI.Screens.ProfileEditor.LayerProperties;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.ConditionalDataBinding;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.DirectDataBinding;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.LayerEffects;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline;
using Artemis.UI.Screens.ProfileEditor.LayerProperties.Tree;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.TreeItem;
using Artemis.UI.Screens.ProfileEditor.Visualization;
using Artemis.UI.Screens.ProfileEditor.Visualization.Tools;
using Artemis.UI.Screens.Settings.Debug;
using Artemis.UI.Screens.Settings.Debug.Device.Tabs;
using Artemis.UI.Screens.Settings.Tabs.Devices;
using Artemis.UI.Screens.Settings.Tabs.Plugins;
using Artemis.UI.Screens.Shared;
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
        PluginFeatureViewModel CreatePluginFeatureViewModel(PluginFeature feature);
        DeviceSettingsViewModel CreateDeviceSettingsViewModel(ArtemisDevice device);
    }

    public interface IDeviceDebugVmFactory : IVmFactory
    {
        DeviceDebugViewModel DeviceDebugViewModel(ArtemisDevice device);
        DevicePropertiesTabViewModel DevicePropertiesTabViewModel(ArtemisDevice device);
        DeviceLedsTabViewModel DeviceLedsTabViewModel(ArtemisDevice device);
    }

    public interface IProfileTreeVmFactory : IVmFactory
    {
        FolderViewModel FolderViewModel(ProfileElement folder);
        LayerViewModel LayerViewModel(ProfileElement layer);
    }

    public interface IProfileLayerVmFactory : IVmFactory
    {
        ProfileLayerViewModel Create(Layer layer, PanZoomViewModel panZoomViewModel);
    }

    public interface IVisualizationToolVmFactory : IVmFactory
    {
        ViewpointMoveToolViewModel ViewpointMoveToolViewModel(PanZoomViewModel panZoomViewModel);
        EditToolViewModel EditToolViewModel(PanZoomViewModel panZoomViewModel);
        SelectionToolViewModel SelectionToolViewModel(PanZoomViewModel panZoomViewModel);
        SelectionRemoveToolViewModel SelectionRemoveToolViewModel(PanZoomViewModel panZoomViewModel);
    }

    public interface IDataModelConditionsVmFactory : IVmFactory
    {
        DataModelConditionGroupViewModel DataModelConditionGroupViewModel(DataModelConditionGroup dataModelConditionGroup, ConditionGroupType groupType);
        DataModelConditionListViewModel DataModelConditionListViewModel(DataModelConditionList dataModelConditionList);
        DataModelConditionEventViewModel DataModelConditionEventViewModel(DataModelConditionEvent dataModelConditionEvent);
        DataModelConditionGeneralPredicateViewModel DataModelConditionGeneralPredicateViewModel(DataModelConditionGeneralPredicate dataModelConditionGeneralPredicate);
        DataModelConditionListPredicateViewModel DataModelConditionListPredicateViewModel(DataModelConditionListPredicate dataModelConditionListPredicate);
        DataModelConditionEventPredicateViewModel DataModelConditionEventPredicateViewModel(DataModelConditionEventPredicate dataModelConditionEventPredicate);
    }

    public interface ILayerPropertyVmFactory : IVmFactory
    {
        LayerPropertyViewModel LayerPropertyViewModel(ILayerProperty layerProperty);

        LayerPropertyGroupViewModel LayerPropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup);
        TreeGroupViewModel TreeGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel);
        TimelineGroupViewModel TimelineGroupViewModel(LayerPropertyGroupViewModel layerPropertyGroupViewModel);

        TreeViewModel TreeViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IObservableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
        EffectsViewModel EffectsViewModel(LayerPropertiesViewModel layerPropertiesViewModel);
        TimelineViewModel TimelineViewModel(LayerPropertiesViewModel layerPropertiesViewModel, IObservableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
        TimelineSegmentViewModel TimelineSegmentViewModel(SegmentViewModelType segment, IObservableCollection<LayerPropertyGroupViewModel> layerPropertyGroups);
    }

    public interface IDataBindingsVmFactory
    {
        IDataBindingViewModel DataBindingViewModel(IDataBindingRegistration registration);
        DirectDataBindingModeViewModel<TLayerProperty, TProperty> DirectDataBindingModeViewModel<TLayerProperty, TProperty>(DirectDataBinding<TLayerProperty, TProperty> directDataBinding);
        DataBindingModifierViewModel<TLayerProperty, TProperty> DataBindingModifierViewModel<TLayerProperty, TProperty>(DataBindingModifier<TLayerProperty, TProperty> modifier);
        ConditionalDataBindingModeViewModel<TLayerProperty, TProperty> ConditionalDataBindingModeViewModel<TLayerProperty, TProperty>(ConditionalDataBinding<TLayerProperty, TProperty> conditionalDataBinding);
        DataBindingConditionViewModel<TLayerProperty, TProperty> DataBindingConditionViewModel<TLayerProperty, TProperty>(DataBindingCondition<TLayerProperty, TProperty> dataBindingCondition);
    }

    public interface IPropertyVmFactory
    {
        ITreePropertyViewModel TreePropertyViewModel(ILayerProperty layerProperty, LayerPropertyViewModel layerPropertyViewModel);
        ITimelinePropertyViewModel TimelinePropertyViewModel(ILayerProperty layerProperty, LayerPropertyViewModel layerPropertyViewModel);
    }
}