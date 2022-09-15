using Artemis.Core;
using Artemis.Core.Events;
using Artemis.VisualScripting.Nodes.Color.Screens;
using SkiaSharp;
using System.Collections.Specialized;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Color Gradient", "Outputs a configurable Color Gradient", "Color", OutputType = typeof(ColorGradient))]
public class ColorGradientNode : Node<ColorGradient, ColorGradientNodeCustomViewModel>
{
    #region Constructors

    public ColorGradientNode()
        : base("Color Gradient", "Outputs a configurable Color Gradient")
    {
        _inputPins = new();

        Output = CreateOutputPin<ColorGradient>();

        StorageModified += OnStorageModified;

        ComputeInputPins();
    }

    private void OnStorageModified(object? sender, EventArgs e)
    {
        ComputeInputPins();
        Storage!.CollectionChanged += OnGradientCollectionChanged;
    }

    private void OnGradientCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        //if the user reorders the gradient, let it slide and do nothing.
        //of course, the user might want to change the input pins since they will no longer line up.
        if (e.Action == NotifyCollectionChangedAction.Move)
            return;

        DisconnectAllInputPins();

        ComputeInputPins();
    }

    private void DisconnectAllInputPins()
    {
        foreach (InputPin item in _inputPins)
        {
            item.DisconnectAll();
        }
    }

    #endregion

    #region Methods

    public override void Evaluate()
    {
        if (Storage is null)
            return;

        ColorGradientStop[] stops = Storage.ToArray();

        if (_inputPins.Count != stops.Length)
            return;

        for (int i = 0; i < _inputPins.Count; i++)
        {
            //if nothing is connected, leave the stop alone.
            if (_inputPins[i].ConnectedTo.Count == 0)
                continue;

            //if the pin has a connection, update the stop.
            if (_inputPins[i].PinValue is SKColor color)
                stops[i].Color = color;
        }

        Output.Value = Storage;
    }

    private void ComputeInputPins()
    {
        int newAmount = Storage?.Count ?? 0;

        if (newAmount == _inputPins.Count)
        {
            return;
        }

        while (newAmount > _inputPins.Count)
        {
            _inputPins.Add(CreateOrAddInputPin(typeof(SKColor), string.Empty));
        }

        while (newAmount < _inputPins.Count)
        {
            InputPin pin = _inputPins.Last();
            RemovePin(pin);
            _inputPins.Remove(pin);
        }
    }

    #endregion

    #region Properties & Fields

    private readonly List<InputPin> _inputPins;

    public OutputPin<ColorGradient> Output { get; }

    #endregion
}

[Node("Color Gradient", "Outputs a Color Gradient from colors and positions", OutputType = typeof(ColorGradient))]
public class ColorGradientFromPinsNode : Node
{
    public OutputPin<ColorGradient> Gradient { get; set; }
    public InputPinCollection<SKColor> Colors { get; set; }
    public InputPinCollection<Numeric> Positions { get; set; }

    public ColorGradientFromPinsNode() : base("Color Gradient", "Outputs a Color Gradient from colors and positions")
    {
        Colors = CreateInputPinCollection<SKColor>("Colors", 0);
        Positions = CreateInputPinCollection<Numeric>("Positions", 0);
        Gradient = CreateOutputPin<ColorGradient>("Gradient");

        Colors.PinAdded += OnPinAdded;
        Colors.PinRemoved += OnPinRemoved;
        Positions.PinAdded += OnPinAdded;
        Positions.PinRemoved += OnPinRemoved;
    }

    private void OnPinRemoved(object? sender, SingleValueEventArgs<IPin> e)
    {
        var colorsCount = Colors.Count();
        var positionsCount = Positions.Count();
        if (colorsCount == positionsCount)
            return;

        while (colorsCount > positionsCount)
        {
            var pinToRemove = Colors.Last();
            Colors.Remove(pinToRemove);

            --colorsCount;
        }

        while (positionsCount > colorsCount)
        {
            var pinToRemove = Positions.Last();
            Positions.Remove(pinToRemove);
            --positionsCount;
        }

        RenamePins();
    }

    private void OnPinAdded(object? sender, SingleValueEventArgs<IPin> e)
    {
        var colorsCount = Colors.Count();
        var positionsCount = Positions.Count();
        if (colorsCount == positionsCount)
            return;

        while (colorsCount < positionsCount)
        {
            Colors.Add(Colors.CreatePin());

            ++colorsCount;
        }

        while (positionsCount < colorsCount)
        {
            Positions.Add(Positions.CreatePin());

            ++positionsCount;
        }

        RenamePins();
    }

    private void RenamePins()
    {
        int colors = 0;
        foreach (var item in Colors)
        {
            item.Name = $"Color #{++colors}";
        }

        int positions = 0;
        foreach(var item in Positions)
        {
            item.Name = $"Position #{++positions}";
        }
    }

    public override void Evaluate()
    {
        var stops = new List<ColorGradientStop>();
        var colors = Colors.Pins.ToArray();
        var positions = Positions.Pins.ToArray();
        for (int i = 0; i < colors.Length; i++)
        {
            stops.Add(new ColorGradientStop(colors[i].Value, positions[i].Value));
        }

        Gradient.Value = new ColorGradient(stops);
    }
}