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
                List<SKColor> colors = this.Select(c => c.Color).ToList();
                for (int i = 0; i <= timesToRepeat; i++)
                    result.AddRange(colors);
            }

            if (seamless && !IsSeamless())
                result.Add(result[0]);

            return result.ToArray();
        }

        /// <summary>
        ///     Gets all the positions in the color gradient
        /// </summary>
        /// <param name="timesToRepeat">
        ///     The amount of times to repeat the positions
        /// </param>
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
        public SKColor GetColor(float position)
        {
            if (!this.Any())
                return SKColor.Empty;

            ColorGradientStop[] stops = this.ToArray();
            if (position <= 0) return stops[0].Color;
            if (position >= 1) return stops[^1].Color;
            ColorGradientStop left = stops[0];
            ColorGradientStop? right = null;
            foreach (ColorGradientStop stop in stops)
            {
                if (stop.Position >= position)
                {
                    right = stop;
                    break;
                }

                left = stop;
            }

            if (right == null || left == right)
                return left.Color;

            position = (float) Math.Round((position - left.Position) / (right.Position - left.Position), 2);
            byte a = (byte) ((right.Color.Alpha - left.Color.Alpha) * position + left.Color.Alpha);
            byte r = (byte) ((right.Color.Red - left.Color.Red) * position + left.Color.Red);
            byte g = (byte) ((right.Color.Green - left.Color.Green) * position + left.Color.Green);
            byte b = (byte) ((right.Color.Blue - left.Color.Blue) * position + left.Color.Blue);
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