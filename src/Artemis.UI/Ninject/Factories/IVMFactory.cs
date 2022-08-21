using System.Collections.ObjectModel;
using System.Reactive;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.Core.ScriptingProviders;
using Artemis.UI.Screens.Device;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.ProfileEditor.DisplayCondition.ConditionTypes;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;
using Artemis.UI.Screens.ProfileEditor.Properties;
using Artemis.UI.Screens.ProfileEditor.Properties.DataBinding;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.ProfileEditor.Properties.Tree;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;
using Artemis.UI.Screens.Scripting;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Screens.VisualScripting.Pins;
using ReactiveUI;

namespace Artemis.UI.Ninject.Factories;

public interface IVmFactory
{
}

public interface IDeviceVmFactory : IVmFactory
{
    DevicePropertiesViewModel DevicePropertiesViewModel(ArtemisDevice device);
    DeviceSettingsViewModel DeviceSettingsViewModel(ArtemisDevice device, DevicesTabViewModel devicesTabViewModel);
    DeviceDetectInputViewModel DeviceDetectInputViewModel(ArtemisDevice device);
    DevicePropertiesTabViewModel DevicePropertiesTabViewModel(ArtemisDevice device);
    DeviceInfoTabViewModel DeviceInfoTabViewModel(ArtemisDevice device);
    DeviceLedsTabViewModel DeviceLedsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds);
    InputMappingsTabViewModel InputMappingsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds);
}

public interface ISettingsVmFactory : IVmFactory
{
    PluginSettingsViewModel PluginSettingsViewModel(Plugin plugin);
    PluginViewModel PluginViewModel(Plugin plugin, ReactiveCommand<Unit, Unit>? reload);
    PluginFeatureViewModel PluginFeatureViewModel(PluginFeatureInfo pluginFeatureInfo, bool showShield);
}

public interface ISidebarVmFactory : IVmFactory
{
    SidebarViewModel? SidebarViewModel(IScreen hostScreen);
    SidebarCategoryViewModel SidebarCategoryViewModel(SidebarViewModel sidebarViewModel, ProfileCategory profileCategory);
    SidebarProfileConfigurationViewModel SidebarProfileConfigurationViewModel(SidebarViewModel sidebarViewModel, ProfileConfiguration profileConfiguration);
}

public interface ISurfaceVmFactory : IVmFactory
{
    SurfaceDeviceViewModel SurfaceDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel);
    ListDeviceViewModel ListDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel);
}

public interface IPrerequisitesVmFactory : IVmFactory
{
    PluginPrerequisiteViewModel PluginPrerequisiteViewModel(PluginPrerequisite pluginPrerequisite, bool uninstall);
}

public interface IProfileEditorVmFactory : IVmFactory
{
    ProfileEditorViewModel ProfileEditorViewModel(IScreen hostScreen);
    FolderTreeItemViewModel FolderTreeItemViewModel(TreeItemViewModel? parent, Folder folder);
    LayerTreeItemViewModel LayerTreeItemViewModel(TreeItemViewModel? parent, Layer layer);
    LayerShapeVisualizerViewModel LayerShapeVisualizerViewModel(Layer layer);
    LayerVisualizerViewModel LayerVisualizerViewModel(Layer layer);
}

public interface ILayerPropertyVmFactory : IVmFactory
{
    PropertyViewModel PropertyViewModel(ILayerProperty layerProperty);
    PropertyGroupViewModel PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup);
    PropertyGroupViewModel PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerBrush layerBrush);
    PropertyGroupViewModel PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerEffect layerEffect);

    TreeGroupViewModel TreeGroupViewModel(PropertyGroupViewModel propertyGroupViewModel);

    TimelineViewModel TimelineViewModel(ObservableCollection<PropertyGroupViewModel> propertyGroupViewModels);
    TimelineGroupViewModel TimelineGroupViewModel(PropertyGroupViewModel propertyGroupViewModel);
}

public interface IDataBindingVmFactory : IVmFactory
{
    DataBindingViewModel DataBindingViewModel();
}

public interface IPropertyVmFactory
{
    ITreePropertyViewModel TreePropertyViewModel(ILayerProperty layerProperty, PropertyViewModel propertyViewModel);
    ITimelinePropertyViewModel TimelinePropertyViewModel(ILayerProperty layerProperty, PropertyViewModel propertyViewModel);
}

public interface INodeVmFactory : IVmFactory
{
    NodeScriptViewModel NodeScriptViewModel(NodeScript nodeScript, bool isPreview);
    NodePickerViewModel NodePickerViewModel(NodeScript nodeScript);
    NodeViewModel NodeViewModel(NodeScriptViewModel nodeScriptViewModel, INode node);
    CableViewModel CableViewModel(NodeScriptViewModel nodeScriptViewModel, IPin from, IPin to);
    DragCableViewModel DragCableViewModel(PinViewModel pinViewModel);
    InputPinViewModel InputPinViewModel(IPin inputPin, NodeScriptViewModel nodeScriptViewModel);
    OutputPinViewModel OutputPinViewModel(IPin outputPin, NodeScriptViewModel nodeScriptViewModel);
    InputPinCollectionViewModel InputPinCollectionViewModel(IPinCollection inputPinCollection, NodeScriptViewModel nodeScriptViewModel);
    OutputPinCollectionViewModel OutputPinCollectionViewModel(IPinCollection outputPinCollection, NodeScriptViewModel nodeScriptViewModel);
}

public interface IConditionVmFactory : IVmFactory
{
    AlwaysOnConditionViewModel AlwaysOnConditionViewModel(AlwaysOnCondition alwaysOnCondition);
    PlayOnceConditionViewModel PlayOnceConditionViewModel(PlayOnceCondition playOnceCondition);
    StaticConditionViewModel StaticConditionViewModel(StaticCondition staticCondition);
    EventConditionViewModel EventConditionViewModel(EventCondition eventCondition);
}

public interface ILayerHintVmFactory : IVmFactory
{
    CategoryAdaptionHintViewModel CategoryAdaptionHintViewModel(Layer layer, CategoryAdaptionHint adaptionHint);
    DeviceAdaptionHintViewModel DeviceAdaptionHintViewModel(Layer layer, DeviceAdaptionHint adaptionHint);
    KeyboardSectionAdaptionHintViewModel KeyboardSectionAdaptionHintViewModel(Layer layer, KeyboardSectionAdaptionHint adaptionHint);
}

public interface IScriptVmFactory : IVmFactory
{
    ScriptConfigurationViewModel ScriptConfigurationViewModel(ScriptConfiguration scriptConfiguration);
    ScriptConfigurationViewModel ScriptConfigurationViewModel(Profile profile, ScriptConfiguration scriptConfiguration);
}