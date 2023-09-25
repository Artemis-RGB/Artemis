using System.Collections.ObjectModel;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.Storage.Entities.Profile;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.DataModel.Screens;

public class DataModelNodeCustomViewModel : CustomNodeViewModel
{
    private readonly DataModelNode _node;
    private readonly INodeEditorService _nodeEditorService;
    private DataModelPath? _dataModelPath;
    private ObservableCollection<Module>? _modules;
    private bool _updating;

    public DataModelNodeCustomViewModel(DataModelNode node, INodeScript script, ISettingsService settingsService, INodeEditorService nodeEditorService, INodeService nodeService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;

        ShowFullPaths = settingsService.GetSetting("ProfileEditor.ShowFullPaths", true);
        ShowDataModelValues = settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);

        List<Type> nodePinTypes = nodeService.GetRegisteredTypes();
        nodePinTypes.AddRange(Constants.NumberTypes);
        NodePinTypes = new ObservableCollection<Type>(nodePinTypes);
        
        this.WhenActivated(d =>
        {
            // Set up extra modules
            if (_node.Script?.Context is Profile scriptProfile && scriptProfile.Configuration.Module != null)
                Modules = new ObservableCollection<Module> {scriptProfile.Configuration.Module};
            else if (_node.Script?.Context is ProfileConfiguration profileConfiguration && profileConfiguration.Module != null)
                Modules = new ObservableCollection<Module> {profileConfiguration.Module};

            // Subscribe to node changes 
            _node.WhenAnyValue(n => n.Storage).Subscribe(UpdateDataModelPath).DisposeWith(d);
            this.WhenAnyValue(vm => vm.DataModelPath).WhereNotNull().Subscribe(ApplyDataModelPath).DisposeWith(d);

            Disposable.Create(() =>
            {
                _dataModelPath?.Dispose();
                _dataModelPath = null;
            }).DisposeWith(d);
        });
    }

    public PluginSetting<bool> ShowFullPaths { get; }
    public PluginSetting<bool> ShowDataModelValues { get; }
    public ObservableCollection<Type> NodePinTypes { get; }
    
    public ObservableCollection<Module>? Modules
    {
        get => _modules;
        set => this.RaiseAndSetIfChanged(ref _modules, value);
    }

    public DataModelPath? DataModelPath
    {
        get => _dataModelPath;
        set => this.RaiseAndSetIfChanged(ref _dataModelPath, value);
    }

    private void UpdateDataModelPath(DataModelPathEntity? entity)
    {
        try
        {
            if (_updating)
                return;

            _updating = true;

            DataModelPath? old = DataModelPath;
            DataModelPath = entity != null ? new DataModelPath(entity) : null;
            old?.Dispose();
        }
        finally
        {
            _updating = false;
        }
    }

    private void ApplyDataModelPath(DataModelPath path)
    {
        try
        {
            if (_updating)
                return;
            if (path.Path == _node.Storage?.Path)
                return;

            _updating = true;

            path.Save();
            _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<DataModelPathEntity>(_node, path?.Entity, "path"));
        }
        finally
        {
            _updating = false;
        }
    }
}