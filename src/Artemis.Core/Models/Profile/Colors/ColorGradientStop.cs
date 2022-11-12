using System;
using System.Linq;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     A color with a position, usually contained in a <see cref="ColorGradient" />
/// </summary>
public class ColorGradientStop : CorePropertyChanged
{
    private SKColor _color;
    private float _position;

    /// <summary>
    ///     Creates a new instance of the <see cref="ColorGradientStop" /> class
    /// </summary>
    public ColorGradientStop(SKColor color, float position)
    {
        Color = color;
        Position = position;
    }

    /// <summary>
    ///     Gets or sets the color of the stop
    /// </summary>
    public SKColor Color
    {
        get => _color;
        set => SetAndNotify(ref _color, value);
    }

    /// <summary>
    ///     Gets or sets the position of the stop
    /// </summary>
    public float Position
    {
        get => _position;
        set => SetAndNotify(ref _position, GetUpdatedPosition(value));
    }

    internal ColorGradient? ColorGradient { get; set; }

    #region Equality members

    /// <inheritdoc cref="object.Equals(object)" />
    protected bool Equals(ColorGradientStop other)
    {
        return _color.Equals(other._color) && _position.Equals(other._position);
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((ColorGradientStop) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        return HashCode.Combine(_color, _position);
    }

    /// <summary>
    ///     Gets the position of the given color stop in a safe manner that avoids overlap with other stops.
    /// </summary>
    /// <param name="stopPosition">The new position.</param>
    private float GetUpdatedPosition(float stopPosition)
    {
        if (ColorGradient == null)
            return stopPosition;
        // Find the first available spot going down
        while (ColorGradient.Any(s => !ReferenceEquals(s, this) && Math.Abs(s.Position - stopPosition) < 0.001f))
            stopPosition -= 0.001f;

        // If we ran out of space, try going up
        if (stopPosition < 0)
        {
            stopPosition = 0;
            while (ColorGradient.Any(s => !ReferenceEquals(s, this) && Math.Abs(s.Position - stopPosition) < 0.001f))
                stopPosition += 0.001f;
        }

        // If we ran out of space there too, movement isn't possible
        if (stopPosition > 1)
            stopPosition = Position;

        return stopPosition;
    }

    /// <summary>
    ///     Interpolates a color gradient stop between the this stop and the provided <paramref name="targetValue"/>.
    /// </summary>
    /// <param name="targetValue">The second stop.</param>
    /// <param name="progress">A value between 0 and 1.</param>
    /// <returns>The interpolated color gradient stop.</returns>
    public void Interpolate(ColorGradientStop targetValue, float progress)
    {
        Color = Color.Interpolate(targetValue.Color, progress);
        Position += (targetValue.Position - Position) * progress;
    }

    #endregion
}