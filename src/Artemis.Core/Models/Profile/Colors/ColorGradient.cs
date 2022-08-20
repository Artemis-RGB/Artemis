using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SkiaSharp;

namespace Artemis.Core;

/// <summary>
///     A gradient containing a list of <see cref="ColorGradientStop" />s
/// </summary>
public class ColorGradient : IList<ColorGradientStop>, IList, INotifyCollectionChanged
{
    private static readonly SKColor[] FastLedRainbow =
    {
        new(0xFFFF0000), // Red
        new(0xFFFF9900), // Orange
        new(0xFFFFFF00), // Yellow
        new(0xFF00FF00), // Green
        new(0xFF00FF7E), // Aqua
        new(0xFF0078FF), // Blue
        new(0xFF9E22FF), // Purple
        new(0xFFFF34AE), // Pink
        new(0xFFFF0000) // and back to Red
    };

    private readonly List<ColorGradientStop> _stops;
    private bool _updating;

    /// <summary>
    ///     Creates a new instance of the <see cref="ColorGradient" /> class
    /// </summary>
    public ColorGradient()
    {
        _stops = new List<ColorGradientStop>();
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ColorGradient" /> class
    /// </summary>
    /// <param name="colorGradient">The color gradient to copy</param>
    public ColorGradient(ColorGradient? colorGradient)
    {
        _stops = new List<ColorGradientStop>();
        if (colorGradient == null)
            return;

        foreach (ColorGradientStop colorGradientStop in colorGradient)
        {
            ColorGradientStop stop = new(colorGradientStop.Color, colorGradientStop.Position);
            stop.PropertyChanged += ItemOnPropertyChanged;
            _stops.Add(stop);
        }
    }

    /// <summary>
    ///     Creates a new instance of the <see cref="ColorGradient" /> class
    /// </summary>
    /// <param name="stops">The stops to copy</param>
    public ColorGradient(List<ColorGradientStop> stops)
    {
        _stops = new List<ColorGradientStop>();
        foreach (ColorGradientStop colorGradientStop in stops)
        {
            ColorGradientStop stop = new(colorGradientStop.Color, colorGradientStop.Position);
            stop.PropertyChanged += ItemOnPropertyChanged;
            _stops.Add(stop);
        }
    }

    /// <summary>
    ///     Gets all the colors in the color gradient
    /// </summary>
    /// <param name="timesToRepeat">The amount of times to repeat the colors</param>
    /// <param name="seamless">
    ///     A boolean indicating whether to make the gradient seamless by adding the first color behind the
    ///     last color
    /// </param>
    /// <returns>An array containing each color in the gradient</returns>
    public SKColor[] GetColorsArray(int timesToRepeat = 0, bool seamless = false)
    {
        List<SKColor> result = new();
        if (timesToRepeat == 0)
        {
            result = this.Select(c => c.Color).ToList();
        }
        else
        {
            for (int i = 0; i <= timesToRepeat; i++)
                result.AddRange(this.Select(c => c.Color));
        }

        if (seamless && !IsSeamless())
            result.Add(result[0]);

        return result.ToArray();
    }

    /// <summary>
    ///     Gets all the positions in the color gradient
    /// </summary>
    /// <param name="timesToRepeat">The amount of times to repeat the positions</param>
    /// <param name="seamless">
    ///     A boolean indicating whether to make the gradient seamless by adding the first color behind the
    ///     last color
    /// </param>
    /// <returns>An array containing a position for each color between 0.0 and 1.0</returns>
    public float[] GetPositionsArray(int timesToRepeat = 0, bool seamless = false)
    {
        List<float> result = new();
        if (timesToRepeat == 0)
        {
            result = this.Select(c => c.Position).ToList();
        }
        else
        {
            // Create stops and a list of divided stops
            List<float> stops = this.Select(c => c.Position / (timesToRepeat + 1)).ToList();

            // For each repeat cycle, add the base stops to the end result
            for (int i = 0; i <= timesToRepeat; i++)
            {
                float lastStop = result.LastOrDefault();
                result.AddRange(stops.Select(s => s + lastStop));
            }
        }

        if (seamless && !IsSeamless())
        {
            // Compress current points evenly
            float compression = 1f - 1f / result.Count;
            for (int index = 0; index < result.Count; index++)
                result[index] *= compression;
            // Add one extra point at the end
            result.Add(1f);
        }

        return result.ToArray();
    }

    /// <summary>
    ///     Gets a color at any position between 0.0 and 1.0 using interpolation
    /// </summary>
    /// <param name="position">A position between 0.0 and 1.0</param>
    /// <param name="timesToRepeat">The amount of times to repeat the positions</param>
    /// <param name="seamless">
    ///     A boolean indicating whether to make the gradient seamless by adding the first color behind the
    ///     last color
    /// </param>
    public SKColor GetColor(float position, int timesToRepeat = 0, bool seamless = false)
    {
        if (!this.Any())
            return new SKColor(255, 255, 255);

        SKColor[] colors = GetColorsArray(timesToRepeat, seamless);
        float[] stops = GetPositionsArray(timesToRepeat, seamless);

        // If at or over the edges, return the corresponding edge
        if (position <= 0) return colors[0];
        if (position >= 1) return colors[^1];

        // Walk through the stops until we find the one at or after the requested position, that becomes the right stop
        // The left stop is the previous stop before the right one was found.
        float left = stops[0];
        float? right = null;
        foreach (float stop in stops)
        {
            if (stop >= position)
            {
                right = stop;
                break;
            }

            left = stop;
        }

        // Get the left stop's color
        SKColor leftColor = colors[Array.IndexOf(stops, left)];

        // If no right stop was found or the left and right stops are on the same spot, return the left stop's color
        if (right == null || left == right)
            return leftColor;

        // Get the right stop's color
        SKColor rightColor = colors[Array.IndexOf(stops, right)];

        // Interpolate the position between the left and right color
        position = MathF.Round((position - left) / (right.Value - left), 2);
        byte a = (byte) ((rightColor.Alpha - leftColor.Alpha) * position + leftColor.Alpha);
        byte r = (byte) ((rightColor.Red - leftColor.Red) * position + leftColor.Red);
        byte g = (byte) ((rightColor.Green - leftColor.Green) * position + leftColor.Green);
        byte b = (byte) ((rightColor.Blue - leftColor.Blue) * position + leftColor.Blue);
        return new SKColor(r, g, b, a);
    }

    /// <summary>
    ///     Gets a new ColorGradient with colors looping through the HSV-spectrum
    /// </summary>
    public static ColorGradient GetUnicornBarf()
    {
        ColorGradient gradient = new();
        for (int index = 0; index < FastLedRainbow.Length; index++)
        {
            SKColor skColor = FastLedRainbow[index];
            float position = 1f / (FastLedRainbow.Length - 1f) * index;
            gradient.Add(new ColorGradientStop(skColor, position));
        }

        return gradient;
    }

    /// <summary>
    ///     Gets a new ColorGradient with random colors from the HSV-spectrum
    /// </summary>
    /// <param name="stops">The amount of stops to add</param>
    public ColorGradient GetRandom(int stops)
    {
        ColorGradient gradient = new();
        gradient.Randomize(stops);
        return gradient;
    }

    /// <summary>
    ///     Determines whether the gradient is seamless
    /// </summary>
    /// <returns><see langword="true" /> if the gradient is seamless; <see langword="false" /> otherwise</returns>
    public bool IsSeamless()
    {
        return Count == 0 || this.First().Color.Equals(this.Last().Color);
    }

    /// <summary>
    ///     Spreads the color stops equally across the gradient.
    /// </summary>
    public void SpreadStops()
    {
        try
        {
            _updating = true;
            for (int i = 0; i < Count; i++)
                this[i].Position = MathF.Round(i / ((float) Count - 1), 3, MidpointRounding.AwayFromZero);
        }
        finally
        {
            _updating = false;
            Sort();
        }
    }

    /// <summary>
    ///     If not already the case, makes the gradient seamless by adding the first color to the end of the gradient and
    ///     compressing the other stops.
    ///     <para>
    ///         If the gradient is already seamless, removes the last color and spreads the remaining stops to fill the freed
    ///         space.
    ///     </para>
    /// </summary>
    public void ToggleSeamless()
    {
        try
        {
            _updating = true;

            if (IsSeamless())
            {
                ColorGradientStop stopToRemove = this.Last();
                Remove(stopToRemove);

                // Uncompress the stops if there is still more than one
                if (Count >= 2)
                {
                    float multiplier = Count / (Count - 1f);
                    foreach (ColorGradientStop stop in this)
                        stop.Position = MathF.Round(Math.Min(stop.Position * multiplier, 100f), 3, MidpointRounding.AwayFromZero);
                }
            }
            else
            {
                // Compress existing stops to the left
                float multiplier = (Count - 1f) / Count;
                foreach (ColorGradientStop stop in this)
                    stop.Position = MathF.Round(stop.Position * multiplier, 3, MidpointRounding.AwayFromZero);

                // Add a stop to the end that is the same color as the first stop
                ColorGradientStop newStop = new(this.First().Color, 1f);
                Add(newStop);
            }
        }
        finally
        {
            _updating = false;
            Sort();
        }
    }

    /// <summary>
    ///     Flips the stops of the gradient.
    /// </summary>
    public void FlipStops()
    {
        try
        {
            _updating = true;
            foreach (ColorGradientStop stop in this)
                stop.Position = 1 - stop.Position;
        }
        finally
        {
            _updating = false;
            Sort();
        }
    }

    /// <summary>
    ///     Rotates the stops of the gradient shifting every stop over to the position of it's neighbor and wrapping around at
    ///     the end of the gradient.
    /// </summary>
    /// <param name="inverse">A boolean indicating whether or not the invert the rotation.</param>
    public void RotateStops(bool inverse)
    {
        try
        {
            _updating = true;
            List<ColorGradientStop> stops = inverse
                ? this.OrderBy(s => s.Position).ToList()
                : this.OrderByDescending(s => s.Position).ToList();

            float lastStopPosition = stops.Last().Position;
            foreach (ColorGradientStop stop in stops)
                (stop.Position, lastStopPosition) = (lastStopPosition, stop.Position);
        }
        finally
        {
            _updating = false;
            Sort();
        }
    }

    /// <summary>
    ///     Randomizes the color gradient with the given amount of <paramref name="stops" />.
    /// </summary>
    /// <param name="stops">The amount of stops to put into the gradient.</param>
    public void Randomize(int stops)
    {
        try
        {
            _updating = true;

            Clear();
            Random random = new();
            for (int index = 0; index < stops; index++)
            {
                SKColor skColor = SKColor.FromHsv(random.NextSingle() * 360, 100, 100);
                float position = 1f / (stops - 1f) * index;
                Add(new ColorGradientStop(skColor, position));
            }
        }
        finally
        {
            _updating = false;
            Sort();
        }
    }

    /// <summary>
    ///     Occurs when any of the stops has changed in some way
    /// </summary>
    public event EventHandler? StopChanged;

    internal void Sort()
    {
        if (_updating)
            return;

        int requiredIndex = 0;
        foreach (ColorGradientStop colorGradientStop in _stops.OrderBy(s => s.Position).ToList())
        {
            int actualIndex = _stops.IndexOf(colorGradientStop);
            if (requiredIndex != actualIndex)
            {
                _stops.RemoveAt(actualIndex);
                _stops.Insert(requiredIndex, colorGradientStop);
                OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, colorGradientStop, requiredIndex, actualIndex));
            }

            requiredIndex++;
        }
    }

    private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        Sort();
        OnStopChanged();
    }

    private void OnStopChanged()
    {
        StopChanged?.Invoke(this, EventArgs.Empty);
    }

    #region Equality members

    /// <summary>
    ///     Determines whether all the stops in this gradient are equal to the stops in the given <paramref name="other" />
    ///     gradient.
    /// </summary>
    /// <param name="other">The other gradient to compare to</param>
    protected bool Equals(ColorGradient other)
    {
        if (Count != other.Count)
            return false;

        for (int i = 0; i < Count; i++)
            if (!Equals(this[i], other[i]))
                return false;

        return true;
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
        return Equals((ColorGradient) obj);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            int hash = 19;
            foreach (ColorGradientStop stops in this)
                hash = hash * 31 + stops.GetHashCode();
            return hash;
        }
    }

    #endregion

    #region Implementation of IEnumerable

    /// <inheritdoc />
    public IEnumerator<ColorGradientStop> GetEnumerator()
    {
        return _stops.GetEnumerator();
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    #endregion

    #region Implementation of ICollection<ColorGradientStop>

    /// <inheritdoc />
    public void Add(ColorGradientStop item)
    {
        _stops.Add(item);
        item.PropertyChanged += ItemOnPropertyChanged;

        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _stops.IndexOf(item)));
        Sort();
    }

    /// <inheritdoc />
    public int Add(object? value)
    {
        if (value is ColorGradientStop stop)
            Add(stop);

        return IndexOf(value);
    }

    /// <inheritdoc cref="IList.Clear" />
    public void Clear()
    {
        foreach (ColorGradientStop item in _stops)
            item.PropertyChanged -= ItemOnPropertyChanged;
        _stops.Clear();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    /// <inheritdoc />
    public bool Contains(object? value)
    {
        return _stops.Contains(value);
    }

    /// <inheritdoc />
    public int IndexOf(object? value)
    {
        return _stops.IndexOf(value);
    }

    /// <inheritdoc />
    public void Insert(int index, object? value)
    {
        if (value is ColorGradientStop stop)
            Insert(index, stop);
    }

    /// <inheritdoc />
    public void Remove(object? value)
    {
        if (value is ColorGradientStop stop)
            Remove(stop);
    }

    /// <inheritdoc />
    public bool Contains(ColorGradientStop item)
    {
        return _stops.Contains(item);
    }

    /// <inheritdoc />
    public void CopyTo(ColorGradientStop[] array, int arrayIndex)
    {
        _stops.CopyTo(array, arrayIndex);
    }

    /// <inheritdoc />
    public bool Remove(ColorGradientStop item)
    {
        item.PropertyChanged -= ItemOnPropertyChanged;
        int index = _stops.IndexOf(item);
        bool removed = _stops.Remove(item);
        if (removed)
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));

        return removed;
    }

    /// <inheritdoc />
    public void CopyTo(Array array, int index)
    {
        _stops.CopyTo((ColorGradientStop[]) array, index);
    }

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count => _stops.Count;

    /// <inheritdoc />
    public bool IsSynchronized => false;

    /// <inheritdoc />
    public object SyncRoot => this;

    /// <inheritdoc cref="ICollection{T}.IsReadOnly" />
    public bool IsReadOnly => false;

    object? IList.this[int index]
    {
        get => this[index];
        set => this[index] = (ColorGradientStop) value!;
    }

    #endregion

    #region Implementation of IList<ColorGradientStop>

    /// <inheritdoc />
    public int IndexOf(ColorGradientStop item)
    {
        return _stops.IndexOf(item);
    }

    /// <inheritdoc />
    public void Insert(int index, ColorGradientStop item)
    {
        _stops.Insert(index, item);
        item.PropertyChanged += ItemOnPropertyChanged;
        Sort();
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, _stops.IndexOf(item)));
    }

    /// <inheritdoc cref="IList{T}.RemoveAt" />
    public void RemoveAt(int index)
    {
        _stops[index].PropertyChanged -= ItemOnPropertyChanged;
        _stops.RemoveAt(index);
        OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, index));
    }

    /// <inheritdoc />
    public bool IsFixedSize => false;

    /// <inheritdoc />
    public ColorGradientStop this[int index]
    {
        get => _stops[index];
        set
        {
            ColorGradientStop? oldValue = _stops[index];
            oldValue.PropertyChanged -= ItemOnPropertyChanged;
            _stops[index] = value;
            _stops[index].PropertyChanged += ItemOnPropertyChanged;
            Sort();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, value, oldValue));
        }
    }

    #endregion

    #region Implementation of INotifyCollectionChanged

    /// <inheritdoc />
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    private void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
    {
        CollectionChanged?.Invoke(this, e);
    }

    #endregion
}