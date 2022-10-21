using Artemis.Core;
using Artemis.Core.Services;
using Artemis.VisualScripting.Nodes.Input.Screens;
using SkiaSharp;
using static Artemis.VisualScripting.Nodes.Input.PressedKeyPositionNodeEntity;

namespace Artemis.VisualScripting.Nodes.Input;

[Node("Pressed Key Position", "Outputs the position of a pressed key relative to a layer", "Input", OutputType = typeof(Numeric))]
public class PressedKeyPositionNode : Node<PressedKeyPositionNodeEntity, PressedKeyPositionNodeCustomViewModel>, IDisposable
{
    private readonly IInputService _inputService;
    private Layer? _layer;
    private SKPoint _ledPosition;
    private Profile? _profile;

    public PressedKeyPositionNode(IInputService inputService)
    {
        _inputService = inputService;
        XPosition = CreateOutputPin<Numeric>("X");
        YPosition = CreateOutputPin<Numeric>("Y");

        StorageModified += OnStorageModified;
        _inputService.KeyboardKeyDown += InputServiceOnKeyboardKeyDown;
        _inputService.KeyboardKeyUp += InputServiceOnKeyboardKeyUp;
    }

    public OutputPin<Numeric> XPosition { get; }
    public OutputPin<Numeric> YPosition { get; }

    public override void Initialize(INodeScript script)
    {
        Storage ??= new PressedKeyPositionNodeEntity();
        
        _profile = script.Context as Profile;
        _layer = _profile?.GetAllLayers().FirstOrDefault(l => l.EntityId == Storage.LayerId);
    }

    public override void Evaluate()
    {
        XPosition.Value = _ledPosition.X;
        YPosition.Value = _ledPosition.Y;
    }

    private void InputServiceOnKeyboardKeyDown(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        if (Storage?.RespondTo is KeyPressType.Down or KeyPressType.UpDown)
            SetLedPosition(e.Led);
    }

    private void InputServiceOnKeyboardKeyUp(object? sender, ArtemisKeyboardKeyEventArgs e)
    {
        if (Storage?.RespondTo is KeyPressType.Up or KeyPressType.UpDown)
            SetLedPosition(e.Led);
    }

    private void SetLedPosition(ArtemisLed? led)
    {
        if (_layer != null && led != null)
            _ledPosition = new SKPoint((led.AbsoluteRectangle.MidX - _layer.Bounds.Left) / _layer.Bounds.Width, (led.AbsoluteRectangle.MidY - _layer.Bounds.Top) / _layer.Bounds.Height);
    }

    private void OnStorageModified(object? sender, EventArgs e)
    {
        _layer = _profile?.GetAllLayers().FirstOrDefault(l => l.EntityId == Storage?.LayerId);
    }
    
    public void Dispose()
    {
        _inputService.KeyboardKeyDown -= InputServiceOnKeyboardKeyDown;
        _inputService.KeyboardKeyUp -= InputServiceOnKeyboardKeyUp;
    }
}