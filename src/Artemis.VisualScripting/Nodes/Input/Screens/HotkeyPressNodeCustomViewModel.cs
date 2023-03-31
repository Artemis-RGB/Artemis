using System.Reactive.Disposables;
using System.Reactive.Linq;
using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;

namespace Artemis.VisualScripting.Nodes.Input.Screens;

public class HotkeyPressNodeCustomViewModel : CustomNodeViewModel
{
    private readonly HotkeyPressNode _pressNode;
    private readonly INodeEditorService _nodeEditorService;
    private Hotkey? _toggleHotkey;
    private bool _updating;

    /// <inheritdoc />
    public HotkeyPressNodeCustomViewModel(HotkeyPressNode pressNode, INodeScript script, INodeEditorService nodeEditorService) : base(pressNode, script)
    {
        _pressNode = pressNode;
        _nodeEditorService = nodeEditorService;
        
        this.WhenActivated(d =>
        {
            Observable.FromEventPattern(x => _pressNode.StorageModified += x, x => _pressNode.StorageModified -= x).Subscribe(_ => Update()).DisposeWith(d);
            Update();
        });
        
    }

    private void Update()
    {
        _updating = true;
        ToggleHotkey = _pressNode.Storage?.EnableHotkey != null ? new Hotkey(_pressNode.Storage.EnableHotkey) : null;
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
            new UpdateStorage<HotkeyEnableDisableNodeEntity>(_pressNode, new HotkeyEnableDisableNodeEntity(ToggleHotkey, null), "hotkey")
        );
    }
}