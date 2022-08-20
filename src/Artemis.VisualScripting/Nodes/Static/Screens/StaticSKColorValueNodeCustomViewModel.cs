using Artemis.Core;
using Artemis.UI.Shared.Services.NodeEditor;
using Artemis.UI.Shared.Services.NodeEditor.Commands;
using Artemis.UI.Shared.VisualScripting;
using ReactiveUI;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Static.Screens;

public class StaticSKColorValueNodeCustomViewModel : CustomNodeViewModel
{
    private readonly StaticSKColorValueNode _node;
    private readonly INodeEditorService _nodeEditorService;
    private bool _applyChanges;
    private SKColor? _currentValue;

    public StaticSKColorValueNodeCustomViewModel(StaticSKColorValueNode node, INodeScript script, INodeEditorService nodeEditorService) : base(node, script)
    {
        _node = node;
        _nodeEditorService = nodeEditorService;
        _applyChanges = true;

        NodeModified += (_, _) => CurrentValue = _node.Storage;
        CurrentValue = _node.Storage;
    }

    public SKColor? CurrentValue
    {
        get => _currentValue;
        set
        {
            if (_applyChanges && value != _node.Storage)
                _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<SKColor>(_node, value ?? SKColor.Empty));
            this.RaiseAndSetIfChanged(ref _currentValue, value);
        }
    }

    public void PauseUpdating()
    {
        _applyChanges = false;
    }

    public void ResumeUpdating()
    {
        _applyChanges = true;

        SKColor updatedValue = CurrentValue ?? SKColor.Empty;
        if (updatedValue != _node.Storage)
            _nodeEditorService.ExecuteCommand(Script, new UpdateStorage<SKColor>(_node, updatedValue));
    }
}