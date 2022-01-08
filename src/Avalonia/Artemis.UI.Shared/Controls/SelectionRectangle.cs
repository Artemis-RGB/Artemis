﻿using System;
using Artemis.Core;
using Artemis.UI.Shared.Events;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;

namespace Artemis.UI.Shared.Controls
{
    /// <summary>
    ///     Visualizes an <see cref="ArtemisDevice" /> with optional per-LED colors
    /// </summary>
    public class SelectionRectangle : Control
    {
        /// <summary>
        ///     Defines the <see cref="Background" /> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> BackgroundProperty =
            AvaloniaProperty.Register<SelectionRectangle, IBrush>(nameof(Background), new SolidColorBrush(Colors.CadetBlue, 0.25));

        /// <summary>
        ///     Defines the <see cref="BorderBrush" /> property.
        /// </summary>
        public static readonly StyledProperty<IBrush> BorderBrushProperty =
            AvaloniaProperty.Register<SelectionRectangle, IBrush>(nameof(BorderBrush), new SolidColorBrush(Colors.CadetBlue));

        /// <summary>
        ///     Defines the <see cref="BorderThickness" /> property.
        /// </summary>
        public static readonly StyledProperty<double> BorderThicknessProperty =
            AvaloniaProperty.Register<SelectionRectangle, double>(nameof(BorderThickness), 1);

        /// <summary>
        ///     Defines the <see cref="BorderRadius" /> property.
        /// </summary>
        public static readonly StyledProperty<double> BorderRadiusProperty =
            AvaloniaProperty.Register<SelectionRectangle, double>(nameof(BorderRadius), 0);

        /// <summary>
        ///     Defines the <see cref="InputElement" /> property.
        /// </summary>
        public static readonly StyledProperty<IControl?> InputElementProperty =
            AvaloniaProperty.Register<SelectionRectangle, IControl?>(nameof(InputElement), notifying: OnInputElementChanged);

        private Rect? _displayRect;
        private IControl? _oldInputElement;
        private Point _startPosition;

        /// <inheritdoc />
        public SelectionRectangle()
        {
            AffectsRender<TextBlock>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty);
            IsHitTestVisible = false;
        }

        /// <summary>
        ///     Gets or sets a brush used to paint the control's background.
        /// </summary>
        public IBrush Background
        {
            get => GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        /// <summary>
        ///     Gets or sets a brush used to paint the control's border
        /// </summary>
        public IBrush BorderBrush
        {
            get => GetValue(BorderBrushProperty);
            set => SetValue(BorderBrushProperty, value);
        }

        /// <summary>
        ///     Gets or sets the width of the control's border
        /// </summary>
        public double BorderThickness
        {
            get => GetValue(BorderThicknessProperty);
            set => SetValue(BorderThicknessProperty, value);
        }

        /// <summary>
        ///     Gets or sets the radius of the control's border
        /// </summary>
        public double BorderRadius
        {
            get => GetValue(BorderRadiusProperty);
            set => SetValue(BorderRadiusProperty, value);
        }

        /// <summary>
        ///     Gets or sets the element that captures input for the selection rectangle.
        /// </summary>
        public IControl? InputElement
        {
            get => GetValue(InputElementProperty);
            set => SetValue(InputElementProperty, value);
        }

        /// <summary>
        ///     Occurs when the selection rect is being updated, indicating the user is dragging.
        /// </summary>
        public event EventHandler<SelectionRectangleEventArgs>? SelectionUpdated;

        /// <summary>
        ///     Occurs when the selection has finished, indicating the user stopped dragging.
        /// </summary>
        public event EventHandler<SelectionRectangleEventArgs>? SelectionFinished;

        /// <summary>
        ///     Invokes the <see cref="SelectionUpdated" /> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSelectionUpdated(SelectionRectangleEventArgs e)
        {
            SelectionUpdated?.Invoke(this, e);
        }

        /// <summary>
        ///     Invokes the <see cref="SelectionFinished" /> event
        /// </summary>
        /// <param name="e"></param>
        protected virtual void OnSelectionFinished(SelectionRectangleEventArgs e)
        {
            SelectionFinished?.Invoke(this, e);
        }

        private static void OnInputElementChanged(IAvaloniaObject sender, bool before)
        {
            ((SelectionRectangle) sender).SubscribeToInputElement();
        }

        private void ParentOnPointerPressed(object? sender, PointerPressedEventArgs e)
        {
            if (!e.GetCurrentPoint(this).Properties.IsLeftButtonPressed)
                return;

            e.Pointer.Capture(this);

            _startPosition = e.GetPosition(Parent);
            _displayRect = null;
        }

        private void ParentOnPointerMoved(object? sender, PointerEventArgs e)
        {
            if (!ReferenceEquals(e.Pointer.Captured, this))
                return;

            Point currentPosition = e.GetPosition(Parent);
            _displayRect = new Rect(
                new Point(Math.Min(_startPosition.X, currentPosition.X), Math.Min(_startPosition.Y, currentPosition.Y)),
                new Point(Math.Max(_startPosition.X, currentPosition.X), Math.Max(_startPosition.Y, currentPosition.Y))
            );
            OnSelectionUpdated(new SelectionRectangleEventArgs(_displayRect.Value, e.KeyModifiers));
            InvalidateVisual();
        }

        private void ParentOnPointerReleased(object? sender, PointerReleasedEventArgs e)
        {
            if (!ReferenceEquals(e.Pointer.Captured, this))
                return;

            e.Pointer.Capture(null);

            if (_displayRect != null)
                OnSelectionFinished(new SelectionRectangleEventArgs(_displayRect.Value, e.KeyModifiers));

            _displayRect = null;
            InvalidateVisual();
        }

        private void SubscribeToInputElement()
        {
            if (_oldInputElement != null)
            {
                _oldInputElement.PointerPressed -= ParentOnPointerPressed;
                _oldInputElement.PointerMoved -= ParentOnPointerMoved;
                _oldInputElement.PointerReleased -= ParentOnPointerReleased;
            }

            _oldInputElement = InputElement;

            if (InputElement != null)
            {
                InputElement.PointerPressed += ParentOnPointerPressed;
                InputElement.PointerMoved += ParentOnPointerMoved;
                InputElement.PointerReleased += ParentOnPointerReleased;
            }
        }

        #region Overrides of Visual

        /// <inheritdoc />
        public override void Render(DrawingContext drawingContext)
        {
            if (_displayRect != null)
                drawingContext.DrawRectangle(Background, new Pen(BorderBrush, BorderThickness), _displayRect.Value, BorderRadius, BorderRadius);
        }

        /// <inheritdoc />
        protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
        {
            SubscribeToInputElement();
            base.OnAttachedToVisualTree(e);
        }

        /// <inheritdoc />
        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            if (_oldInputElement != null)
            {
                _oldInputElement.PointerPressed -= ParentOnPointerPressed;
                _oldInputElement.PointerMoved -= ParentOnPointerMoved;
                _oldInputElement.PointerReleased -= ParentOnPointerReleased;
                _oldInputElement = null;
            }

            base.OnDetachedFromVisualTree(e);
        }

        #endregion
    }
}