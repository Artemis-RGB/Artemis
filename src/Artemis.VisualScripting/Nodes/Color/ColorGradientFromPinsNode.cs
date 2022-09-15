using Artemis.Core;
using Artemis.Core.Events;
using SkiaSharp;

namespace Artemis.VisualScripting.Nodes.Color;

[Node("Color Gradient (Advanced)", "Outputs a Color Gradient from colors and positions", "Color", OutputType = typeof(ColorGradient))]
public class ColorGradientFromPinsNode : Node
{
    public OutputPin<ColorGradient> Gradient { get; set; }
    public InputPinCollection<SKColor> Colors { get; set; }
    public InputPinCollection<Numeric> Positions { get; set; }

    public ColorGradientFromPinsNode()
        : base("Color Gradient", "Outputs a Color Gradient from colors and positions")
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