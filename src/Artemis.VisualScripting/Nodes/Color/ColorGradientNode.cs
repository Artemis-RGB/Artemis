using System.Collections.Specialized;
using Artemis.Core;
using Artemis.VisualScripting.Nodes.Color.Screens;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Color Gradient (Simple)", "Outputs a color gradient with the given colors", "Color", OutputType = typeof(ColorGradient))]
public class ColorGradientNode : Node<ColorGradient, ColorGradientNodeCustomViewModel>
{
    private readonly List<InputPin> _inputPins;

    public ColorGradientNode() : base("Color Gradient", "Outputs a color gradient with the given colors")
    {
        _inputPins = new List<InputPin>();

        Gradient = ColorGradient.GetUnicornBarf();
        Output = CreateOutputPin<ColorGradient>();
        ViewModelPosition = CustomNodeViewModelPosition.AbovePins;
    }

    public ColorGradient Gradient { get; private set; }
    public OutputPin<ColorGradient> Output { get; }

    public override void Initialize(INodeScript script)
    {
        UpdateGradient();
        ComputeInputPins();

        // Not expecting storage to get modified, but lets just make sure
        StorageModified += OnStorageModified;
    }

    public override void Evaluate()
    {
        ColorGradientStop[] stops = Gradient.ToArray();

        if (_inputPins.Count != stops.Length)
            return;

        for (int i = 0; i < _inputPins.Count; i++)
        {
            // if nothing is connected, leave the stop alone.
            if (_inputPins[i].ConnectedTo.Count == 0)
                continue;

            // if the pin has a connection, update the stop.
            if (_inputPins[i].PinValue is SKColor color)
                stops[i].Color = color;
        }

        Output.Value = Gradient;
    }

    private void DisconnectAllInputPins()
    {
        foreach (InputPin item in _inputPins)
            item.DisconnectAll();
    }

    private void UpdateGradient()
    {
        Gradient.CollectionChanged -= OnGradientCollectionChanged;
        if (Storage != null)
            Gradient = Storage;
        else
            Storage = Gradient;
        Gradient.CollectionChanged += OnGradientCollectionChanged;
    }

    private void ComputeInputPins()
    {
        int newAmount = Gradient.Count;
        if (newAmount == _inputPins.Count)
            return;

        while (newAmount > _inputPins.Count)
            _inputPins.Add(CreateOrAddInputPin(typeof(SKColor), string.Empty));

        while (newAmount < _inputPins.Count)
        {
            InputPin pin = _inputPins.Last();
            RemovePin(pin);
            _inputPins.Remove(pin);
        }

        int index = 0;
        foreach (InputPin item in _inputPins)
            item.Name = $"Color #{++index}";
    }
    
    private void OnStorageModified(object? sender, EventArgs e)
    {
        UpdateGradient();
        ComputeInputPins();
    }

    private void OnGradientCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // if the user reorders the gradient, let it slide and do nothing.
        // of course, the user might want to change the input pins since they will no longer line up.
        if (e.Action == NotifyCollectionChangedAction.Move)
            return;

        // DisconnectAllInputPins();
        ComputeInputPins();
    }
}