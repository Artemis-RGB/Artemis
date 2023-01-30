using Artemis.Core;
using Artemis.Core.Services;
using Artemis.VisualScripting.Nodes.Input.Screens;

namespace Artemis.VisualScripting.Nodes.Input;

[Node("Hotkey press", "Outputs a boolean value for as long as a hotkey is pressed", "Input", OutputType = typeof(bool))]
public class HotkeyPressNode : Node<HotkeyEnableDisableNodeEntity, HotkeyPressNodeCustomViewModel>, IDisposable
{
    private readonly IInputService _inputService;
    private Hotkey? _toggleHotkey;
    private bool _value;

    public HotkeyPressNode(IInputService inputService)
    {
        _inputService = inputService;
        _inputService.KeyboardKeyDown += InputServiceOnKeyboardKeyDown;
        _inputService.KeyboardKeyUp += InputServiceOnKeyboardKeyUp;

        Output = CreateOutputPin<bool>();
        StorageModified += OnStorageModified;
    }

    public OutputPin<bool> Output { get; }

    public override void Initialize(INodeScript script)
    {
        LoadHotkeys();
    }

    public override void Evaluate()
    {
        Output.Value = _value;
    }

    private void OnStorageModified(object? sender, EventArgs e)
    {
        LoadHotkeys();
    }

    private void LoadHotkeys()
    {
        if (Storage == null)
            return;

        _toggleHotkey = Storage.EnableHotkey != null ? new Hotkey(Storage.EnableHotkey) : null;
    }
    
    private void InputServiceOnKeyboardKeyDown(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        if (Storage == null)
            return;
        if (_toggleHotkey != null && _toggleHotkey.MatchesEventArgs(e))
            _value = true;
    }

    private void InputServiceOnKeyboardKeyUp(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        if (Storage == null)
            return;
        if (_toggleHotkey != null && _toggleHotkey.MatchesEventArgs(e))
            _value = false;
    }

    public void Dispose()
    {
        _inputService.KeyboardKeyUp -= InputServiceOnKeyboardKeyUp;
    }
}