using Artemis.Core;
using Artemis.VisualScripting.Nodes.Color.Screens;
using SkiaSharp;
using System.Collections.Specialized;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Color Gradient (Simple)", "Outputs a color gradient with the given colors", "Color", OutputType = typeof(ColorGradient))]
public class ColorGradientNode : Node<ColorGradient, ColorGradientNodeCustomViewModel>
{
    #region Constructors

    public ColorGradientNode()
        : base("Color Gradient", "Outputs a color gradient with the given colors")
    {
        Storage = ColorGradient.GetUnicornBarf();

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
        
        int index = 0;
        foreach (var item in _inputPins)
        {
            item.Name = $"Color #{++index}";
        }
    }

    #endregion

    #region Properties & Fields

    private readonly List<InputPin> _inputPins;

    public OutputPin<ColorGradient> Output { get; }

    #endregion
}
