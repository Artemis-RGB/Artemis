using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core;
using Artemis.VisualScripting.Editor.Controls.Wrapper;

namespace Artemis.VisualScripting.Editor.Controls
{
    public class VisualScriptNodePresenter : Control
    {
        #region Properties & Fields

        private bool _isDragging;
        private bool _isDragged;
        private bool _isDeselected;
        private Point _dragStartPosition;

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty NodeProperty = DependencyProperty.Register(
            "Node", typeof(VisualScriptNode), typeof(VisualScriptNodePresenter), new PropertyMetadata(default(VisualScriptNode), NodePropertyChangedCallback));

        public VisualScriptNode Node
        {
            get => (VisualScriptNode) GetValue(NodeProperty);
            set => SetValue(NodeProperty, value);
        }

        public static readonly DependencyProperty CustomViewModelProperty = DependencyProperty.Register(
            "CustomViewModel", typeof(ICustomNodeViewModel), typeof(VisualScriptNodePresenter), new PropertyMetadata(null));

        public ICustomNodeViewModel CustomViewModel
        {
            get => (ICustomNodeViewModel) GetValue(CustomViewModelProperty);
            set => SetValue(CustomViewModelProperty, value);
        }

        public static readonly DependencyProperty TitleBrushProperty = DependencyProperty.Register(
            "TitleBrush", typeof(Brush), typeof(VisualScriptNodePresenter), new PropertyMetadata(default(Brush)));

        public Brush TitleBrush
        {
            get => (Brush) GetValue(TitleBrushProperty);
            set => SetValue(TitleBrushProperty, value);
        }

        #endregion

        #region Constructors

        public VisualScriptNodePresenter()
        {
            Unloaded += OnUnloaded;
        }

        #endregion

        #region Methods

        protected override Size MeasureOverride(Size constraint)
        {
            int SnapToGridSize(int value)
            {
                int mod = value % Node.Script.GridSize;
                return mod switch
                {
                    < 0 => Node.Script.GridSize,
                    > 0 => value + (Node.Script.GridSize - mod),
                    _ => value
                };
            }

            Size neededSize = base.MeasureOverride(constraint);
            int width = (int) Math.Ceiling(neededSize.Width);
            int height = (int) Math.Ceiling(neededSize.Height);

            return new Size(SnapToGridSize(width), SnapToGridSize(height));
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs args)
        {
            base.OnMouseLeftButtonDown(args);

            _isDragged = false;
            _isDragging = true;
            _isDeselected = false;

            bool isShiftDown = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);
            if (!Node.IsSelected)
                Node.Select(isShiftDown);
            else if (isShiftDown)
            {
                _isDeselected = true;
                Node.Deselect(true);
            }

            Node.DragStart();

            _dragStartPosition = PointToScreen(args.GetPosition(this));

            CaptureMouse();
            args.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs args)
        {
            base.OnMouseLeftButtonUp(args);

            if (_isDragging)
            {
                _isDragging = false;
                Node.DragEnd();

                ReleaseMouseCapture();
            }

            if (!_isDragged && !_isDeselected)
                Node.Select(Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift));

            args.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs args)
        {
            base.OnMouseMove(args);

            if (!_isDragging) return;

            Point mousePosition = PointToScreen(args.GetPosition(this));
            Vector offset = mousePosition - _dragStartPosition;

            if (args.LeftButton == MouseButtonState.Pressed)
            {
                _dragStartPosition = mousePosition;
                Node.DragMove(offset.X, offset.Y);

                if ((offset.X != 0) && (offset.Y != 0))
                    _isDragged = true;
            }
            else
            {
                _isDragging = false;
                Node.DragEnd();
                ReleaseMouseCapture();
            }

            args.Handled = true;
        }

        private void GetCustomViewModel()
        {
            CustomViewModel?.OnDeactivate();

            if (Node?.Node is Node customViewModelNode)
            {
                CustomViewModel = customViewModelNode.GetCustomViewModel();
                CustomViewModel?.OnActivate();
            }
            else
                CustomViewModel = null;
        }

        private static void NodePropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is VisualScriptNodePresenter presenter)
                presenter.GetCustomViewModel();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            CustomViewModel?.OnDeactivate();
            CustomViewModel = null;
        }

        #endregion
    }
}