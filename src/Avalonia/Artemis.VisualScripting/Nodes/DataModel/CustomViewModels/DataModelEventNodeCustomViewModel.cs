using System.Collections.ObjectModel;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.VisualScripting;

namespace Artemis.VisualScripting.Nodes.DataModel.CustomViewModels;

public class DataModelEventNodeCustomViewModel : CustomNodeViewModel
{
    private readonly DataModelEventNode _node;
    private ObservableCollection<Module> _modules;

    public DataModelEventNodeCustomViewModel(DataModelEventNode node, ISettingsService settingsService) : base(node)
    {
        _node = node;

        ShowFullPaths = settingsService.GetSetting("ProfileEditor.ShowFullPaths", true);
        ShowDataModelValues = settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);
    }

    public PluginSetting<bool> ShowFullPaths { get; }
    public PluginSetting<bool> ShowDataModelValues { get; }
    public ObservableCollection<Type> FilterTypes { get; } = new() {typeof(IDataModelEvent)};

    // public ObservableCollection<Module> Modules
    // {
    //     get => _modules;
    //     set => SetAndNotify(ref _modules, value);
    // }

    public DataModelPath DataModelPath
    {
        get => _node.DataModelPath;
        set
        {
            if (ReferenceEquals(_node.DataModelPath, value))
                return;

            _node.DataModelPath?.Dispose();
            _node.DataModelPath = value;
            _node.DataModelPath.Save();

            _node.Storage = _node.DataModelPath.Entity;
        }
    }

    // public override void OnActivate()
    // {
    //     if (Modules != null)
    //         return;
    //
    //     Modules = new ObservableCollection<Module>();
    //     if (_node.Script.Context is Profile scriptProfile && scriptProfile.Configuration.Module != null)
    //         Modules.Add(scriptProfile.Configuration.Module);
    //     else if (_node.Script.Context is ProfileConfiguration profileConfiguration && profileConfiguration.Module != null)
    //         Modules.Add(profileConfiguration.Module);
    //
    //     _node.PropertyChanged += NodeOnPropertyChanged;
    // }
    //
    // public override void OnDeactivate()
    // {
    //     _node.PropertyChanged -= NodeOnPropertyChanged;
    // }
    //
    // private void NodeOnPropertyChanged(object sender, PropertyChangedEventArgs e)
    // {
    //     if (e.PropertyName == nameof(DataModelNode.DataModelPath))
    //         OnPropertyChanged(nameof(DataModelPath));
    // }
}