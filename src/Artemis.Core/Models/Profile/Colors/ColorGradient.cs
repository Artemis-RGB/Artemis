using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using SkiaSharp;

namespace Artemis.Core
{
    /// <summary>
    ///     A gradient containing a list of <see cref="ColorGradientStop" />s
    /// </summary>
    public class ColorGradient : IList<ColorGradientStop>, INotifyCollectionChanged
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

        /// <summary>
        ///     Creates a new instance of the <see cref="ColorGradient" /> class
        /// </summary>
        public ColorGradient()
        {
            _stops = new List<ColorGradientStop>();
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
                    result[index] = result[index] * compression;
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
        /// <returns></returns>
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
        ///     Determines whether the gradient is seamless
        /// </summary>
        /// <returns><see langword="true" /> if the gradient is seamless; <see langword="false" /> otherwise</returns>
        public bool IsSeamless()
        {
            return Count == 0 || this.First().Color.Equals(this.Last().Color);
        }

        internal void Sort()
        {
            _stops.Sort((a, b) => a.Position.CompareTo(b.Position));
        }

        private void ItemOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Sort();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

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
            Sort();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, _stops.IndexOf(item)));
        }


        /// <inheritdoc />
        public void Clear()
        {
            foreach (ColorGradientStop item in _stops)
                item.PropertyChanged -= ItemOnPropertyChanged;
            _stops.Clear();
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
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
        public int Count => _stops.Count;

        /// <inheritdoc />
        public bool IsReadOnly => false;

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

        /// <inheritdoc />
        public void RemoveAt(int index)
        {
            _stops[index].PropertyChanged -= ItemOnPropertyChanged;
            _stops.RemoveAt(index);
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, index));
        }

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
}