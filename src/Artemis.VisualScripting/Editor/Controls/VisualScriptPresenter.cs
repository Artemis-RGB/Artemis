using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.Core.VisualScripting;
using Artemis.VisualScripting.Editor.Controls.Wrapper;
using Artemis.VisualScripting.Model;
using Artemis.VisualScripting.ViewModel;

namespace Artemis.VisualScripting.Editor.Controls
{
    [TemplatePart(Name = PART_CANVAS, Type = typeof(Canvas))]
    [TemplatePart(Name = PART_NODELIST, Type = typeof(ItemsControl))]
    [TemplatePart(Name = PART_CABLELIST, Type = typeof(ItemsControl))]
    [TemplatePart(Name = PART_SELECTION_BORDER, Type = typeof(Border))]
    [TemplatePart(Name = PART_CREATION_BOX_PARENT, Type = typeof(Panel))]
    public class VisualScriptPresenter : Control
    {
        #region Constants

        private const string PART_CANVAS = "PART_Canvas";
        private const string PART_NODELIST = "PART_NodeList";
        private const string PART_CABLELIST = "PART_CableList";
        private const string PART_SELECTION_BORDER = "PART_SelectionBorder";
        private const string PART_CREATION_BOX_PARENT = "PART_CreationBoxParent";

        #endregion

        #region Properties & Fields

        private Canvas _canvas;
        private ItemsControl _nodeList;
        private ItemsControl _cableList;
        private Border _selectionBorder;
        private TranslateTransform _canvasViewPortTransform;
        private Panel _creationBoxParent;

        private Vector _viewportCenter = new(0, 0);

        private bool _dragCanvas = false;
        private Point _dragCanvasStartLocation;
        private Vector _dragCanvasStartOffset;
        private bool _movedDuringDrag = false;

        private bool _boxSelect = false;
        private Point _boxSelectStartPoint;

        private Point _lastRightClickLocation;

        internal VisualScript VisualScript { get; private set; }

        #endregion

        #region Dependency Properties

        public static readonly DependencyProperty ScriptProperty = DependencyProperty.Register(
            "Script", typeof(IScript), typeof(VisualScriptPresenter), new PropertyMetadata(default(IScript), ScriptChanged));

        public IScript Script
        {
            get => (IScript)GetValue(ScriptProperty);
            set => SetValue(ScriptProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(VisualScriptPresenter), new PropertyMetadata(1.0, ScaleChanged));

        public double Scale
        {
            get => (double)GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
            "MinScale", typeof(double), typeof(VisualScriptPresenter), new PropertyMetadata(0.15));

        public double MinScale
        {
            get => (double)GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, value);
        }

        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
            "MaxScale", typeof(double), typeof(VisualScriptPresenter), new PropertyMetadata(1.0));

        public double MaxScale
        {
            get => (double)GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.Register(
            "ScaleFactor", typeof(double), typeof(VisualScriptPresenter), new PropertyMetadata(1.3));

        public double ScaleFactor
        {
            get => (double)GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }

        public static readonly DependencyProperty AvailableNodesProperty = DependencyProperty.Register(
            "AvailableNodes", typeof(IEnumerable<NodeData>), typeof(VisualScriptPresenter), new PropertyMetadata(default(IEnumerable<NodeData>)));

        public IEnumerable<NodeData> AvailableNodes
        {
            get => (IEnumerable<NodeData>)GetValue(AvailableNodesProperty);
            set => SetValue(AvailableNodesProperty, value);
        }

        public static readonly DependencyProperty CreateNodeCommandProperty = DependencyProperty.Register(
            "CreateNodeCommand", typeof(ICommand), typeof(VisualScriptPresenter), new PropertyMetadata(default(ICommand)));

        public ICommand CreateNodeCommand
        {
            get => (ICommand)GetValue(CreateNodeCommandProperty);
            private set => SetValue(CreateNodeCommandProperty, value);
        }

        public static readonly DependencyProperty GridSizeProperty = DependencyProperty.Register(
            "GridSize", typeof(int), typeof(VisualScriptPresenter), new PropertyMetadata(24));

        public int GridSize
        {
            get => (int)GetValue(GridSizeProperty);
            set => SetValue(GridSizeProperty, value);
        }

        public static readonly DependencyProperty SurfaceSizeProperty = DependencyProperty.Register(
            "SurfaceSize", typeof(int), typeof(VisualScriptPresenter), new PropertyMetadata(16384));

        public int SurfaceSize
        {
            get => (int)GetValue(SurfaceSizeProperty);
            set => SetValue(SurfaceSizeProperty, value);
        }

        #endregion

        #region Constructors

        public VisualScriptPresenter()
        {
            CreateNodeCommand = new ActionCommand<NodeData>(CreateNode);

            this.SizeChanged += OnSizeChanged;
        }

        #endregion

        #region Methods

        public override void OnApplyTemplate()
        {
            _canvas = GetTemplateChild(PART_CANVAS) as Canvas ?? throw new NullReferenceException($"The Canvas '{PART_CANVAS}' is missing.");
            _selectionBorder = GetTemplateChild(PART_SELECTION_BORDER) as Border ?? throw new NullReferenceException($"The Border '{PART_SELECTION_BORDER}' is missing.");
            _nodeList = GetTemplateChild(PART_NODELIST) as ItemsControl ?? throw new NullReferenceException($"The ItemsControl '{PART_NODELIST}' is missing.");
            _cableList = GetTemplateChild(PART_CABLELIST) as ItemsControl ?? throw new NullReferenceException($"The ItemsControl '{PART_CABLELIST}' is missing.");
            _creationBoxParent = GetTemplateChild(PART_CREATION_BOX_PARENT) as Panel ?? throw new NullReferenceException($"The Panel '{PART_CREATION_BOX_PARENT}' is missing.");

            _canvas.AllowDrop = true;

            _canvas.RenderTransform = _canvasViewPortTransform = new TranslateTransform(0, 0);
            _canvas.MouseLeftButtonDown += OnCanvasMouseLeftButtonDown;
            _canvas.MouseLeftButtonUp += OnCanvasMouseLeftButtonUp;
            _canvas.MouseRightButtonDown += OnCanvasMouseRightButtonDown;
            _canvas.MouseRightButtonUp += OnCanvasMouseRightButtonUp;
            _canvas.PreviewMouseRightButtonDown += OnCanvasPreviewMouseRightButtonDown;
            _canvas.MouseMove += OnCanvasMouseMove;
            _canvas.MouseWheel += OnCanvasMouseWheel;
            _canvas.DragOver += OnCanvasDragOver;

            _nodeList.ItemsSource = VisualScript?.Nodes;
            _cableList.ItemsSource = VisualScript?.Cables;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (sender is not VisualScriptPresenter scriptPresenter) return;

            scriptPresenter.UpdatePanning();
        }

        private static void ScriptChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is not VisualScriptPresenter scriptPresenter) return;

            scriptPresenter.ScriptChanged(args.NewValue is not Script script ? null : new VisualScript(script, scriptPresenter.SurfaceSize, scriptPresenter.GridSize));
        }

        private void ScriptChanged(VisualScript newScript)
        {
            if (VisualScript != null)
                VisualScript.PropertyChanged -= OnVisualScriptPropertyChanged;

            VisualScript = newScript;

            if (VisualScript != null)
            {
                VisualScript.PropertyChanged += OnVisualScriptPropertyChanged;

                if (_nodeList != null)
                    _nodeList.ItemsSource = VisualScript?.Nodes;

                if (_cableList != null)
                    _cableList.ItemsSource = VisualScript?.Cables;

                VisualScript.Nodes.Clear();
                foreach (INode node in VisualScript.Script.Nodes)
                    InitializeNode(node);
            }

            VisualScript?.RecreateCables();

            CenterAt(new Vector(0, 0));
        }

        private void OnVisualScriptPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(VisualScript.Cables))
                if (_cableList != null)
                    _cableList.ItemsSource = VisualScript.Cables;
        }

        private void OnCanvasPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs args)
        {
            _lastRightClickLocation = args.GetPosition(_canvas);
        }

        private void OnCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            VisualScript.DeselectAllNodes();

            _boxSelect = true;
            _boxSelectStartPoint = args.GetPosition(_canvas);
            Canvas.SetLeft(_selectionBorder, _boxSelectStartPoint.X);
            Canvas.SetTop(_selectionBorder, _boxSelectStartPoint.Y);
            _selectionBorder.Width = 0;
            _selectionBorder.Height = 0;
            _selectionBorder.Visibility = Visibility.Visible;
            _canvas.CaptureMouse();

            args.Handled = true;
        }

        private void OnCanvasMouseLeftButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (_boxSelect)
            {
                _boxSelect = false;
                _canvas.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                _selectionBorder.Visibility = Visibility.Hidden;
                SelectWithinRectangle(new Rect(Canvas.GetLeft(_selectionBorder), Canvas.GetTop(_selectionBorder), _selectionBorder.Width, _selectionBorder.Height));
                args.Handled = _movedDuringDrag;
            }
        }

        private void OnCanvasMouseRightButtonDown(object sender, MouseButtonEventArgs args)
        {
            _dragCanvas = true;
            _dragCanvasStartLocation = args.GetPosition(this);
            _dragCanvasStartOffset = _viewportCenter;

            _movedDuringDrag = false;

            _canvas.CaptureMouse();

            args.Handled = true;
        }

        private void OnCanvasMouseRightButtonUp(object sender, MouseButtonEventArgs args)
        {
            if (_dragCanvas)
            {
                _dragCanvas = false;
                _canvas.ReleaseMouseCapture();
                Mouse.OverrideCursor = null;
                args.Handled = _movedDuringDrag;
            }
        }

        private void OnCanvasMouseMove(object sender, MouseEventArgs args)
        {
            if (_dragCanvas)
            {
                if (args.RightButton == MouseButtonState.Pressed)
                {
                    Vector newLocation = _dragCanvasStartOffset + (((args.GetPosition(this) - _dragCanvasStartLocation)) * (1.0 / Scale));
                    CenterAt(newLocation);

                    _movedDuringDrag = true;
                    Mouse.OverrideCursor = Cursors.ScrollAll;
                }
                else
                    _dragCanvas = false;

                args.Handled = true;
            }
            else if (_boxSelect)
            {
                if (args.LeftButton == MouseButtonState.Pressed)
                {
                    double x = _boxSelectStartPoint.X;
                    double y = _boxSelectStartPoint.Y;
                    Point mousePosition = args.GetPosition(_canvas);

                    double rectX = mousePosition.X > x ? x : mousePosition.X;
                    double rectY = mousePosition.Y > y ? y : mousePosition.Y;
                    double rectWidth = Math.Abs(x - mousePosition.X);
                    double rectHeight = Math.Abs(y - mousePosition.Y);

                    Canvas.SetLeft(_selectionBorder, rectX);
                    Canvas.SetTop(_selectionBorder, rectY);
                    _selectionBorder.Width = rectWidth;
                    _selectionBorder.Height = rectHeight;
                }
                else
                    _boxSelect = false;

                args.Handled = true;
            }
        }

        private void OnCanvasDragOver(object sender, DragEventArgs args)
        {
            if (VisualScript == null) return;

            if (VisualScript.IsConnecting)
                VisualScript.OnDragOver(args.GetPosition(_canvas));
        }

        private void OnCanvasMouseWheel(object sender, MouseWheelEventArgs args)
        {
            if (args.Delta < 0)
                Scale /= ScaleFactor;
            else
                Scale *= ScaleFactor;

            Scale = Clamp(Scale, MinScale, MaxScale);

            _canvas.LayoutTransform = new ScaleTransform(Scale, Scale);

            UpdatePanning();

            args.Handled = true;
        }

        private void SelectWithinRectangle(Rect rectangle)
        {
            if (Script == null) return;

            VisualScript.DeselectAllNodes();

            for (int i = 0; i < _nodeList.Items.Count; i++)
            {
                ContentPresenter nodeControl = (ContentPresenter)_nodeList.ItemContainerGenerator.ContainerFromIndex(i);
                VisualScriptNode node = (VisualScriptNode)nodeControl.Content;

                double nodeWidth = nodeControl.ActualWidth;
                double nodeHeight = nodeControl.ActualHeight;

                if (rectangle.IntersectsWith(new Rect(node.X, node.Y, nodeWidth, nodeHeight)))
                    node.Select(true);
            }
        }

        private static void ScaleChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is not VisualScriptPresenter presenter) return;
            if (presenter.VisualScript == null) return;

            presenter.VisualScript.NodeDragScale = 1.0 / presenter.Scale;
        }

        private void CreateNode(NodeData nodeData)
        {
            if (nodeData == null) return;

            if (_creationBoxParent.ContextMenu != null)
                _creationBoxParent.ContextMenu.IsOpen = false;

            INode node = nodeData.CreateNode();
            Script.AddNode(node);

            InitializeNode(node, _lastRightClickLocation);
        }

        private void InitializeNode(INode node, Point? initialLocation = null)
        {
            VisualScriptNode visualScriptNode = new(VisualScript, node);
            if (initialLocation != null)
            {
                visualScriptNode.X = initialLocation.Value.X;
                visualScriptNode.Y = initialLocation.Value.Y;
            }
            visualScriptNode.SnapNodeToGrid();
            VisualScript.Nodes.Add(visualScriptNode);
        }

        private void CenterAt(Vector vector)
        {
            double halfSurface = (SurfaceSize / 2.0);
            _viewportCenter = Clamp(vector, new Vector(-halfSurface, -halfSurface), new Vector(halfSurface, halfSurface));
            UpdatePanning();
        }

        private void UpdatePanning()
        {
            if (_canvasViewPortTransform == null) return;

            double surfaceOffset = (SurfaceSize / 2.0) * Scale;
            _canvasViewPortTransform.X = (((_viewportCenter.X * Scale) + (ActualWidth / 2.0))) - surfaceOffset;
            _canvasViewPortTransform.Y = (((_viewportCenter.Y * Scale) + (ActualHeight / 2.0))) - surfaceOffset;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static double Clamp(double value, double min, double max)
        {
            // ReSharper disable ConvertIfStatementToReturnStatement - I'm not sure why, but inlining this statement reduces performance by ~10%
            if (value < min) return min;
            if (value > max) return max;
            return value;
            // ReSharper restore ConvertIfStatementToReturnStatement
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static Vector Clamp(Vector value, Vector min, Vector max)
        {
            double x = Clamp(value.X, min.X, max.X);
            double y = Clamp(value.Y, min.Y, max.Y);
            return new Vector(x, y);
        }

        #endregion
    }
}
