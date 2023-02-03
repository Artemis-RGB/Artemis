using Artemis.Core;
using Artemis.Core.Services;
using Artemis.VisualScripting.Nodes.Input.Screens;

namespace Artemis.VisualScripting.Nodes.Input;

[Node("Hotkey enable/disable", "Outputs a boolean value enabled and disabled by a set of hotkeys", "Input", OutputType = typeof(bool))]
public class HotkeyEnableDisableNode : Node<HotkeyEnableDisableNodeEntity, HotkeyEnableDisableNodeCustomViewModel>, IDisposable
{
    private readonly IInputService _inputService;
    private Hotkey? _disableHotkey;
    private Hotkey? _enableHotkey;
    private bool _value;
    private bool _retrievedInitialValue;

    public HotkeyEnableDisableNode(IInputService inputService)
    {
        _inputService = inputService;
        _inputService.KeyboardKeyUp += InputServiceOnKeyboardKeyUp;

        InitialValue = CreateInputPin<bool>();
        Output = CreateOutputPin<bool>();
        StorageModified += OnStorageModified;
    }

    public InputPin<bool> InitialValue { get; }
    public OutputPin<bool> Output { get; }

    public override void Initialize(INodeScript script)
    {
        LoadHotkeys();
    }

    public override void Evaluate()
    {
        if (!_retrievedInitialValue)
        {
            _value = InitialValue.Value;
            _retrievedInitialValue = true;
        }

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

        _enableHotkey = Storage.EnableHotkey != null ? new Hotkey(Storage.EnableHotkey) : null;
        _disableHotkey = Storage.DisableHotkey != null ? new Hotkey(Storage.DisableHotkey) : null;
    }

    private void InputServiceOnKeyboardKeyUp(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        if (Storage == null)
            return;

        if (_disableHotkey != null && _disableHotkey.MatchesEventArgs(e))
            _value = false;
        else if (_enableHotkey != null && _enableHotkey.MatchesEventArgs(e))
            _value = true;
    }

    public void Dispose()
    {
        _inputService.KeyboardKeyUp -= InputServiceOnKeyboardKeyUp;
    }
}