using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Disposables;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.Core.Services;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.DataModel.CustomViewModels;

public class DataModelNodeCustomViewModel : CustomNodeViewModel
{
    private readonly DataModelNode _node;
    private ObservableCollection<Module>? _modules;

    public DataModelNodeCustomViewModel(DataModelNode node, ISettingsService settingsService) : base(node)
    {
        _node = node;

        ShowFullPaths = settingsService.GetSetting("ProfileEditor.ShowFullPaths", true);
        ShowDataModelValues = settingsService.GetSetting("ProfileEditor.ShowDataModelValues", false);

        this.WhenActivated(d =>
        {
            if (Modules != null)
                return;

            Modules = new ObservableCollection<Module>();
            if (_node.Script.Context is Profile scriptProfile && scriptProfile.Configuration.Module != null)
                Modules.Add(scriptProfile.Configuration.Module);
            else if (_node.Script.Context is ProfileConfiguration profileConfiguration && profileConfiguration.Module != null)
                Modules.Add(profileConfiguration.Module);

            _node.PropertyChanged += NodeOnPropertyChanged;
            Disposable.Create(() => _node.PropertyChanged -= NodeOnPropertyChanged).DisposeWith(d);
        });
    }

    public PluginSetting<bool> ShowFullPaths { get; }
    public PluginSetting<bool> ShowDataModelValues { get; }

    public ObservableCollection<Module>? Modules
    {
        get => _modules;
        set => RaiseAndSetIfChanged(ref _modules, value);
    }

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
            _node.UpdateOutputPin(false);
        }
    }

    private void NodeOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(DataModelNode.DataModelPath))
            this.RaisePropertyChanged(nameof(DataModelPath));
    }
}