using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Input.Screens;

public class HotkeyEnableDisableNodeCustomViewModel : CustomNodeViewModel
{
    private readonly HotkeyEnableDisableNode _enableDisableNode;
    private readonly INodeEditorService _nodeEditorService;
    private Hotkey? _enableHotkey;
    private Hotkey? _disableHotkey;
    private bool _updating;

    /// <inheritdoc />
    public HotkeyEnableDisableNodeCustomViewModel(HotkeyEnableDisableNode enableDisableNode, INodeScript script, INodeEditorService nodeEditorService) : base(enableDisableNode, script)
    {
        _enableDisableNode = enableDisableNode;
        _nodeEditorService = nodeEditorService;
        
        this.WhenActivated(d =>
        {
            Observable.FromEventPattern(x => _enableDisableNode.StorageModified += x, x => _enableDisableNode.StorageModified -= x).Subscribe(_ => Update()).DisposeWith(d);
            Update();
        });
        
    }

    private void Update()
    {
        _updating = true;
        
        EnableHotkey = _enableDisableNode.Storage?.EnableHotkey != null ? new Hotkey(_enableDisableNode.Storage.EnableHotkey) : null;
        DisableHotkey = _enableDisableNode.Storage?.DisableHotkey != null ? new Hotkey(_enableDisableNode.Storage.DisableHotkey) : null;
        
        _updating = false;
    }
    
    public Hotkey? EnableHotkey
    {
        get => _enableHotkey;
        set => this.RaiseAndSetIfChanged(ref _enableHotkey, value);
    }

    public Hotkey? DisableHotkey
    {
        get => _disableHotkey;
        set => this.RaiseAndSetIfChanged(ref _disableHotkey, value);
    }

    public void Save()
    {
        if (_updating)
            return;
        
        _nodeEditorService.ExecuteCommand(
            Script,
            new UpdateStorage<HotkeyEnableDisableNodeEntity>(_enableDisableNode, new HotkeyEnableDisableNodeEntity(EnableHotkey, DisableHotkey), "hotkey")
        );
    }
}