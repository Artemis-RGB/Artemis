using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using Avalonia.Controls.Mixins;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Input.Screens;

public class HotkeyToggleNodeCustomViewModel : CustomNodeViewModel
{
    private readonly HotkeyToggleNode _toggleNode;
    private readonly INodeEditorService _nodeEditorService;
    private Hotkey? _toggleHotkey;
    private bool _updating;

    /// <inheritdoc />
    public HotkeyToggleNodeCustomViewModel(HotkeyToggleNode toggleNode, INodeScript script, INodeEditorService nodeEditorService) : base(toggleNode, script)
    {
        _toggleNode = toggleNode;
        _nodeEditorService = nodeEditorService;
        
        this.WhenActivated(d =>
        {
            Observable.FromEventPattern(x => _toggleNode.StorageModified += x, x => _toggleNode.StorageModified -= x).Subscribe(_ => Update()).DisposeWith(d);
            Update();
        });
        
    }

    private void Update()
    {
        _updating = true;
        ToggleHotkey = _toggleNode.Storage?.EnableHotkey != null ? new Hotkey(_toggleNode.Storage.EnableHotkey) : null;
        _updating = false;
    }
    
    public Hotkey? ToggleHotkey
    {
        get => _toggleHotkey;
        set => this.RaiseAndSetIfChanged(ref _toggleHotkey, value);
    }
    
    public void Save()
    {
        if (_updating)
            return;
        
        _nodeEditorService.ExecuteCommand(
            Script,
            new UpdateStorage<HotkeyEnableDisableNodeEntity>(_toggleNode, new HotkeyEnableDisableNodeEntity(ToggleHotkey, null), "hotkey")
        );
    }
}