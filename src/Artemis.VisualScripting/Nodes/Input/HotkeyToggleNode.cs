using Artemis.Core;
using Artemis.Core.Services;
using Artemis.VisualScripting.Nodes.Input.Screens;

namespace Artemis.VisualScripting.Nodes.Input;

[Node("Hotkey toggle", "Outputs a boolean value toggled by a hotkey", "Input", OutputType = typeof(bool))]
public class HotkeyToggleNode : Node<HotkeyEnableDisableNodeEntity, HotkeyToggleNodeCustomViewModel>, IDisposable
{
    private readonly IInputService _inputService;
    private Hotkey? _toggleHotkey;
    private bool _value;
    private bool _retrievedInitialValue;

    public HotkeyToggleNode(IInputService inputService)
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

        _toggleHotkey = Storage.EnableHotkey != null ? new Hotkey(Storage.EnableHotkey) : null;
    }

    private void InputServiceOnKeyboardKeyUp(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        if (Storage == null)
            return;
        if (_toggleHotkey != null && _toggleHotkey.MatchesEventArgs(e))
            _value = !_value;
    }

    public void Dispose()
    {
        _inputService.KeyboardKeyUp -= InputServiceOnKeyboardKeyUp;
    }
}