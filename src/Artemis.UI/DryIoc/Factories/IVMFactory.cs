using System.Collections.ObjectModel;
using System.Reactive;
using Artemis.Core;
using Artemis.Core.LayerBrushes;
using Artemis.Core.LayerEffects;
using Artemis.UI.Screens.Device;
using Artemis.UI.Screens.Device.General;
using Artemis.UI.Screens.Device.InputMappings;
using Artemis.UI.Screens.Device.Layout;
using Artemis.UI.Screens.Device.Leds;
using Artemis.UI.Screens.Plugins;
using Artemis.UI.Screens.Plugins.Features;
using Artemis.UI.Screens.Plugins.Prerequisites;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.ProfileEditor.DisplayCondition.ConditionTypes;
using Artemis.UI.Screens.ProfileEditor.ProfileTree;
using Artemis.UI.Screens.ProfileEditor.ProfileTree.Dialogs.AdaptionHints;
using Artemis.UI.Screens.ProfileEditor.Properties;
using Artemis.UI.Screens.ProfileEditor.Properties.DataBinding;
using Artemis.UI.Screens.ProfileEditor.Properties.Timeline;
using Artemis.UI.Screens.ProfileEditor.Properties.Tree;
using Artemis.UI.Screens.ProfileEditor.VisualEditor.Visualizers;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Settings.Updating;
using Artemis.UI.Screens.Sidebar;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.VisualScripting;
using Artemis.UI.Screens.VisualScripting.Pins;
using Artemis.WebClient.Updating;
using DryIoc;
using ReactiveUI;

namespace Artemis.UI.DryIoc.Factories;

public interface IVmFactory
{
}

public interface IDeviceVmFactory : IVmFactory
{
    DevicePropertiesViewModel DevicePropertiesViewModel(ArtemisDevice device);
    DeviceSettingsViewModel DeviceSettingsViewModel(ArtemisDevice device, DevicesTabViewModel devicesTabViewModel);
    DeviceDetectInputViewModel DeviceDetectInputViewModel(ArtemisDevice device);
    DeviceLayoutTabViewModel DeviceLayoutTabViewModel(ArtemisDevice device);
    DeviceLedsTabViewModel DeviceLedsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds);
    InputMappingsTabViewModel InputMappingsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds);
    DeviceGeneralTabViewModel DeviceGeneralTabViewModel(ArtemisDevice device);
}

public class DeviceFactory : IDeviceVmFactory
{
    private readonly IContainer _container;

    public DeviceFactory(IContainer container)
    {
        _container = container;
    }

    public DevicePropertiesViewModel DevicePropertiesViewModel(ArtemisDevice device)
    {
        return _container.Resolve<DevicePropertiesViewModel>([device]);
    }

    public DeviceSettingsViewModel DeviceSettingsViewModel(ArtemisDevice device, DevicesTabViewModel devicesTabViewModel)
    {
        return _container.Resolve<DeviceSettingsViewModel>([device, devicesTabViewModel]);
    }

    public DeviceDetectInputViewModel DeviceDetectInputViewModel(ArtemisDevice device)
    {
        return _container.Resolve<DeviceDetectInputViewModel>([device]);
    }

    public DeviceLayoutTabViewModel DeviceLayoutTabViewModel(ArtemisDevice device)
    {
        return _container.Resolve<DeviceLayoutTabViewModel>([device]);
    }

    public DeviceLedsTabViewModel DeviceLedsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds)
    {
        return _container.Resolve<DeviceLedsTabViewModel>([device, selectedLeds]);
    }

    public InputMappingsTabViewModel InputMappingsTabViewModel(ArtemisDevice device, ObservableCollection<ArtemisLed> selectedLeds)
    {
        return _container.Resolve<InputMappingsTabViewModel>([device, selectedLeds]);
    }

    public DeviceGeneralTabViewModel DeviceGeneralTabViewModel(ArtemisDevice device)
    {
        return _container.Resolve<DeviceGeneralTabViewModel>([device]);
    }
}

public interface ISettingsVmFactory : IVmFactory
{
    PluginSettingsViewModel PluginSettingsViewModel(PluginInfo pluginInfo);
    PluginViewModel PluginViewModel(PluginInfo pluginInfo, ReactiveCommand<Unit, Unit>? reload);
    PluginFeatureViewModel PluginFeatureViewModel(PluginFeatureInfo pluginFeatureInfo, bool showShield);
}

public class SettingsVmFactory : ISettingsVmFactory
{
    private readonly IContainer _container;

    public SettingsVmFactory(IContainer container)
    {
        _container = container;
    }

    public PluginSettingsViewModel PluginSettingsViewModel(PluginInfo pluginInfo)
    {
        return _container.Resolve<PluginSettingsViewModel>([pluginInfo]);
    }

    public PluginViewModel PluginViewModel(PluginInfo pluginInfo, ReactiveCommand<Unit, Unit>? reload)
    {
        return _container.Resolve<PluginViewModel>([pluginInfo, reload]);
    }

    public PluginFeatureViewModel PluginFeatureViewModel(PluginFeatureInfo pluginFeatureInfo, bool showShield)
    {
        return _container.Resolve<PluginFeatureViewModel>([pluginFeatureInfo, showShield]);
    }
}

public interface ISidebarVmFactory : IVmFactory
{
    SidebarCategoryViewModel SidebarCategoryViewModel(ProfileCategory profileCategory);
    SidebarProfileConfigurationViewModel SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration);
}

public class SidebarVmFactory : ISidebarVmFactory
{
    private readonly IContainer _container;

    public SidebarVmFactory(IContainer container)
    {
        _container = container;
    }

    public SidebarCategoryViewModel SidebarCategoryViewModel(ProfileCategory profileCategory)
    {
        return _container.Resolve<SidebarCategoryViewModel>([profileCategory]);
    }

    public SidebarProfileConfigurationViewModel SidebarProfileConfigurationViewModel(ProfileConfiguration profileConfiguration)
    {
        return _container.Resolve<SidebarProfileConfigurationViewModel>([profileConfiguration]);
    }
}

public interface ISurfaceVmFactory : IVmFactory
{
    SurfaceDeviceViewModel SurfaceDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel);
    ListDeviceViewModel ListDeviceViewModel(ArtemisDevice device);
}

public class SurfaceVmFactory : ISurfaceVmFactory
{
    private readonly IContainer _container;

    public SurfaceVmFactory(IContainer container)
    {
        _container = container;
    }

    public SurfaceDeviceViewModel SurfaceDeviceViewModel(ArtemisDevice device, SurfaceEditorViewModel surfaceEditorViewModel)
    {
        return _container.Resolve<SurfaceDeviceViewModel>([device, surfaceEditorViewModel]);
    }

    public ListDeviceViewModel ListDeviceViewModel(ArtemisDevice device)
    {
        return _container.Resolve<ListDeviceViewModel>([device]);
    }
}

public interface IPrerequisitesVmFactory : IVmFactory
{
    PluginPrerequisiteViewModel PluginPrerequisiteViewModel(PluginPrerequisite pluginPrerequisite, bool uninstall);
}

public class PrerequisitesVmFactory : IPrerequisitesVmFactory
{
    private readonly IContainer _container;

    public PrerequisitesVmFactory(IContainer container)
    {
        _container = container;
    }

    public PluginPrerequisiteViewModel PluginPrerequisiteViewModel(PluginPrerequisite pluginPrerequisite, bool uninstall)
    {
        return _container.Resolve<PluginPrerequisiteViewModel>([pluginPrerequisite, uninstall]);
    }
}

public interface IProfileEditorVmFactory : IVmFactory
{
    ProfileEditorViewModel ProfileEditorViewModel(IScreen hostScreen);
    FolderTreeItemViewModel FolderTreeItemViewModel(TreeItemViewModel? parent, Folder folder);
    LayerTreeItemViewModel LayerTreeItemViewModel(TreeItemViewModel? parent, Layer layer);
    LayerShapeVisualizerViewModel LayerShapeVisualizerViewModel(Layer layer);
    LayerVisualizerViewModel LayerVisualizerViewModel(Layer layer);
}

public class ProfileEditorVmFactory : IProfileEditorVmFactory
{
    private readonly IContainer _container;

    public ProfileEditorVmFactory(IContainer container)
    {
        _container = container;
    }

    public FolderTreeItemViewModel FolderTreeItemViewModel(TreeItemViewModel? parent, Folder folder)
    {
        return _container.Resolve<FolderTreeItemViewModel>([parent, folder]);
    }

    public LayerShapeVisualizerViewModel LayerShapeVisualizerViewModel(Layer layer)
    {
        return _container.Resolve<LayerShapeVisualizerViewModel>([layer]);
    }

    public LayerTreeItemViewModel LayerTreeItemViewModel(TreeItemViewModel? parent, Layer layer)
    {
        return _container.Resolve<LayerTreeItemViewModel>([parent, layer]);
    }

    public LayerVisualizerViewModel LayerVisualizerViewModel(Layer layer)
    {
        return _container.Resolve<LayerVisualizerViewModel>([layer]);
    }

    public ProfileEditorViewModel ProfileEditorViewModel(IScreen hostScreen)
    {
        return _container.Resolve<ProfileEditorViewModel>([hostScreen]);
    }
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

public class LayerPropertyVmFactory : ILayerPropertyVmFactory
{
    private readonly IContainer _container;

    public LayerPropertyVmFactory(IContainer container)
    {
        _container = container;
    }

    public PropertyViewModel PropertyViewModel(ILayerProperty layerProperty)
    {
        return _container.Resolve<PropertyViewModel>([layerProperty]);
    }

    public PropertyGroupViewModel PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup)
    {
        return _container.Resolve<PropertyGroupViewModel>([layerPropertyGroup]);
    }

    public PropertyGroupViewModel PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerBrush layerBrush)
    {
        return _container.Resolve<PropertyGroupViewModel>([layerPropertyGroup, layerBrush]);
    }

    public PropertyGroupViewModel PropertyGroupViewModel(LayerPropertyGroup layerPropertyGroup, BaseLayerEffect layerEffect)
    {
        return _container.Resolve<PropertyGroupViewModel>([layerPropertyGroup, layerEffect]);
    }

    public TreeGroupViewModel TreeGroupViewModel(PropertyGroupViewModel propertyGroupViewModel)
    {
        return _container.Resolve<TreeGroupViewModel>([propertyGroupViewModel]);
    }

    public TimelineViewModel TimelineViewModel(ObservableCollection<PropertyGroupViewModel> propertyGroupViewModels)
    {
        return _container.Resolve<TimelineViewModel>([propertyGroupViewModels]);
    }

    public TimelineGroupViewModel TimelineGroupViewModel(PropertyGroupViewModel propertyGroupViewModel)
    {
        return _container.Resolve<TimelineGroupViewModel>([propertyGroupViewModel]);
    }
}

public interface IDataBindingVmFactory : IVmFactory
{
    DataBindingViewModel DataBindingViewModel();
}

public class DataBindingVmFactory : IDataBindingVmFactory
{
    private readonly IContainer _container;

    public DataBindingVmFactory(IContainer container)
    {
        _container = container;
    }

    public DataBindingViewModel DataBindingViewModel()
    {
        return _container.Resolve<DataBindingViewModel>();
    }
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

public class NodeVmFactory : INodeVmFactory
{
    private readonly IContainer _container;

    public NodeVmFactory(IContainer container)
    {
        _container = container;
    }

    public NodeScriptViewModel NodeScriptViewModel(NodeScript nodeScript, bool isPreview)
    {
        return _container.Resolve<NodeScriptViewModel>([nodeScript, isPreview]);
    }

    public NodePickerViewModel NodePickerViewModel(NodeScript nodeScript)
    {
        return _container.Resolve<NodePickerViewModel>([nodeScript]);
    }

    public NodeViewModel NodeViewModel(NodeScriptViewModel nodeScriptViewModel, INode node)
    {
        return _container.Resolve<NodeViewModel>([nodeScriptViewModel, node]);
    }

    public CableViewModel CableViewModel(NodeScriptViewModel nodeScriptViewModel, IPin from, IPin to)
    {
        return _container.Resolve<CableViewModel>([nodeScriptViewModel, from, to]);
    }

    public DragCableViewModel DragCableViewModel(PinViewModel pinViewModel)
    {
        return _container.Resolve<DragCableViewModel>([pinViewModel]);
    }

    public InputPinViewModel InputPinViewModel(IPin inputPin, NodeScriptViewModel nodeScriptViewModel)
    {
        return _container.Resolve<InputPinViewModel>([inputPin, nodeScriptViewModel]);
    }

    public OutputPinViewModel OutputPinViewModel(IPin outputPin, NodeScriptViewModel nodeScriptViewModel)
    {
        return _container.Resolve<OutputPinViewModel>([outputPin, nodeScriptViewModel]);
    }

    public InputPinCollectionViewModel InputPinCollectionViewModel(IPinCollection inputPinCollection, NodeScriptViewModel nodeScriptViewModel)
    {
        return _container.Resolve<InputPinCollectionViewModel>([inputPinCollection, nodeScriptViewModel]);
    }

    public OutputPinCollectionViewModel OutputPinCollectionViewModel(IPinCollection outputPinCollection, NodeScriptViewModel nodeScriptViewModel)
    {
        return _container.Resolve<OutputPinCollectionViewModel>([outputPinCollection, nodeScriptViewModel]);
    }
}

public interface IConditionVmFactory : IVmFactory
{
    AlwaysOnConditionViewModel AlwaysOnConditionViewModel(AlwaysOnCondition alwaysOnCondition);
    PlayOnceConditionViewModel PlayOnceConditionViewModel(PlayOnceCondition playOnceCondition);
    StaticConditionViewModel StaticConditionViewModel(StaticCondition staticCondition);
    EventConditionViewModel EventConditionViewModel(EventCondition eventCondition);
}

public class ConditionVmFactory : IConditionVmFactory
{
    private readonly IContainer _container;

    public ConditionVmFactory(IContainer container)
    {
        _container = container;
    }

    public AlwaysOnConditionViewModel AlwaysOnConditionViewModel(AlwaysOnCondition alwaysOnCondition)
    {
        return _container.Resolve<AlwaysOnConditionViewModel>([alwaysOnCondition]);
    }

    public PlayOnceConditionViewModel PlayOnceConditionViewModel(PlayOnceCondition playOnceCondition)
    {
        return _container.Resolve<PlayOnceConditionViewModel>([playOnceCondition]);
    }

    public StaticConditionViewModel StaticConditionViewModel(StaticCondition staticCondition)
    {
        return _container.Resolve<StaticConditionViewModel>([staticCondition]);
    }

    public EventConditionViewModel EventConditionViewModel(EventCondition eventCondition)
    {
        return _container.Resolve<EventConditionViewModel>([eventCondition]);
    }
}

public interface ILayerHintVmFactory : IVmFactory
{
    CategoryAdaptionHintViewModel CategoryAdaptionHintViewModel(Layer layer, CategoryAdaptionHint adaptionHint);
    DeviceAdaptionHintViewModel DeviceAdaptionHintViewModel(Layer layer, DeviceAdaptionHint adaptionHint);
    KeyboardSectionAdaptionHintViewModel KeyboardSectionAdaptionHintViewModel(Layer layer, KeyboardSectionAdaptionHint adaptionHint);
    SingleLedAdaptionHintViewModel SingleLedAdaptionHintViewModel(Layer layer, SingleLedAdaptionHint adaptionHint);
}

public class LayerHintVmFactory : ILayerHintVmFactory
{
    private readonly IContainer _container;

    public LayerHintVmFactory(IContainer container)
    {
        _container = container;
    }

    public CategoryAdaptionHintViewModel CategoryAdaptionHintViewModel(Layer layer, CategoryAdaptionHint adaptionHint)
    {
        return _container.Resolve<CategoryAdaptionHintViewModel>([layer, adaptionHint]);
    }

    public DeviceAdaptionHintViewModel DeviceAdaptionHintViewModel(Layer layer, DeviceAdaptionHint adaptionHint)
    {
        return _container.Resolve<DeviceAdaptionHintViewModel>([layer, adaptionHint]);
    }

    public KeyboardSectionAdaptionHintViewModel KeyboardSectionAdaptionHintViewModel(Layer layer, KeyboardSectionAdaptionHint adaptionHint)
    {
        return _container.Resolve<KeyboardSectionAdaptionHintViewModel>([layer, adaptionHint]);
    }

    public SingleLedAdaptionHintViewModel SingleLedAdaptionHintViewModel(Layer layer, SingleLedAdaptionHint adaptionHint)
    {
        return _container.Resolve<SingleLedAdaptionHintViewModel>([layer, adaptionHint]);
    }
}

public interface IReleaseVmFactory : IVmFactory
{
    ReleaseViewModel ReleaseListViewModel(IGetReleases_PublishedReleases_Nodes release);
}

public class ReleaseVmFactory : IReleaseVmFactory
{
    private readonly IContainer _container;

    public ReleaseVmFactory(IContainer container)
    {
        _container = container;
    }

    public ReleaseViewModel ReleaseListViewModel(IGetReleases_PublishedReleases_Nodes release)
    {
        return _container.Resolve<ReleaseViewModel>([release]);
    }
}