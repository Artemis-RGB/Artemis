using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Artemis.Core;
using Artemis.VisualScripting.Editor.Controls.Wrapper;
using Artemis.VisualScripting.ViewModel;
using VisualScript = Artemis.VisualScripting.Editor.Controls.Wrapper.VisualScript;

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

        private bool _fitPending = false;
        private Canvas _canvas;
        private ItemsControl _nodeList;
        private ItemsControl _cableList;
        private Border _selectionBorder;
        private TranslateTransform _canvasViewPortTransform;
        private ScaleTransform _canvasViewPortScale;
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
            "Script", typeof(INodeScript), typeof(VisualScriptPresenter), new PropertyMetadata(default(INodeScript), ScriptChanged));

        public INodeScript Script
        {
            get => (INodeScript) GetValue(ScriptProperty);
            set => SetValue(ScriptProperty, value);
        }

        public static readonly DependencyProperty ScaleProperty = DependencyProperty.Register(
            "Scale", typeof(double), typeof(VisualScriptPresenter), new PropertyMetadata(1.0, ScaleChanged));

        public double Scale
        {
            get => (double) GetValue(ScaleProperty);
            set => SetValue(ScaleProperty, value);
        }

        public static readonly DependencyProperty MinScaleProperty = DependencyProperty.Register(
            "MinScale", typeof(double), typeof(VisualScriptPresenter), new PropertyMetadata(0.15));

        public double MinScale
        {
            get => (double) GetValue(MinScaleProperty);
            set => SetValue(MinScaleProperty, value);
        }

        public static readonly DependencyProperty MaxScaleProperty = DependencyProperty.Register(
            "MaxScale", typeof(double), typeof(VisualScriptPresenter), new PropertyMetadata(1.0));

        public double MaxScale
        {
            get => (double) GetValue(MaxScaleProperty);
            set => SetValue(MaxScaleProperty, value);
        }

        public static readonly DependencyProperty ScaleFactorProperty = DependencyProperty.Register(
            "ScaleFactor", typeof(double), typeof(VisualScriptPresenter), new PropertyMetadata(1.3));

        public double ScaleFactor
        {
            get => (double) GetValue(ScaleFactorProperty);
            set => SetValue(ScaleFactorProperty, value);
        }

        public static readonly DependencyProperty AvailableNodesProperty = DependencyProperty.Register(
            "AvailableNodes", typeof(IEnumerable<NodeData>), typeof(VisualScriptPresenter), new PropertyMetadata(default(IEnumerable<NodeData>)));

        public IEnumerable<NodeData> AvailableNodes
        {
            get => (IEnumerable<NodeData>) GetValue(AvailableNodesProperty);
            set => SetValue(AvailableNodesProperty, value);
        }

        public static readonly DependencyProperty AlwaysShowValuesProperty = DependencyProperty.Register(
            "AlwaysShowValues", typeof(bool), typeof(VisualScriptPresenter), new PropertyMetadata(default(bool)));

        public bool AlwaysShowValues
        {
            get => (bool) GetValue(AlwaysShowValuesProperty);
            set => SetValue(AlwaysShowValuesProperty, value);
        }

        public static readonly DependencyProperty SourcePinProperty = DependencyProperty.Register(
            "SourcePin", typeof(VisualScriptPin), typeof(VisualScriptPresenter), new PropertyMetadata(default(VisualScriptPin)));

        public VisualScriptPin SourcePin
        {
            get => (VisualScriptPin) GetValue(SourcePinProperty);
            set => SetValue(SourcePinProperty, value);
        }

        public static readonly DependencyProperty CreateNodeCommandProperty = DependencyProperty.Register(
            "CreateNodeCommand", typeof(ICommand), typeof(VisualScriptPresenter), new PropertyMetadata(default(ICommand)));

        public ICommand CreateNodeCommand
        {
            get => (ICommand) GetValue(CreateNodeCommandProperty);
            private set => SetValue(CreateNodeCommandProperty, value);
        }

        public static readonly DependencyProperty GridSizeProperty = DependencyProperty.Register(
            "GridSize", typeof(int), typeof(VisualScriptPresenter), new PropertyMetadata(24));

        public int GridSize
        {
            get => (int) GetValue(GridSizeProperty);
            set => SetValue(GridSizeProperty, value);
        }

        public static readonly DependencyProperty SurfaceSizeProperty = DependencyProperty.Register(
            "SurfaceSize", typeof(int), typeof(VisualScriptPresenter), new PropertyMetadata(16384));

        public int SurfaceSize
        {
            get => (int) GetValue(SurfaceSizeProperty);
            set => SetValue(SurfaceSizeProperty, value);
        }

        public static readonly DependencyProperty AutoFitScriptProperty = DependencyProperty.Register(
            "AutoFitScript", typeof(bool), typeof(VisualScriptPresenter), new PropertyMetadata(false, AutoFitScriptChanged));

        public bool AutoFitScript
        {
            get => (bool) GetValue(AutoFitScriptProperty);
            set => SetValue(AutoFitScriptProperty, value);
        }

        #endregion

        #region Events

        public event EventHandler ScriptUpdated;

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

            _canvas.LayoutTransform = _canvasViewPortScale = new ScaleTransform(MaxScale, MaxScale);
            _canvas.RenderTransform = _canvasViewPortTransform = new TranslateTransform(0, 0);
            _canvas.MouseLeftButtonDown += OnCanvasMouseLeftButtonDown;
            _canvas.MouseLeftButtonUp += OnCanvasMouseLeftButtonUp;
            _canvas.PreviewMouseRightButtonDown += OnCanvasPreviewMouseRightButtonDown;
            _canvas.MouseMove += OnCanvasMouseMove;
            _canvas.MouseWheel += OnCanvasMouseWheel;
            _canvas.DragOver += OnCanvasDragOver;
            _canvas.Drop += OnCanvasDrop;
            _nodeList.ItemsSource = VisualScript?.Nodes;
            _cableList.ItemsSource = VisualScript?.Cables;
        }

        private static void AutoFitScriptChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is not VisualScriptPresenter scriptPresenter) return;

            if ((args.NewValue as bool?) == true)
                scriptPresenter.FitScript();
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs args)
        {
            if (sender is not VisualScriptPresenter scriptPresenter) return;

            if (AutoFitScript)
                scriptPresenter.FitScript();
            else
                scriptPresenter.UpdatePanning();
        }

        private static void ScriptChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
        {
            if (d is not VisualScriptPresenter scriptPresenter) return;

            scriptPresenter.ScriptChanged(args.NewValue is not NodeScript script ? null : new VisualScript(script, scriptPresenter.SurfaceSize, scriptPresenter.GridSize));
        }

        private void ScriptChanged(VisualScript newScript)
        {
            if (VisualScript != null)
            {
                VisualScript.ScriptUpdated -= OnVisualScriptScriptUpdated;
                VisualScript.PropertyChanged -= OnVisualScriptPropertyChanged;
                VisualScript.NodeMoved -= OnVisualScriptNodeMoved;
                VisualScript.NodeCollectionChanged -= OnVisualScriptNodeCollectionChanged;
            }

            VisualScript = newScript;

            if (VisualScript != null)
            {
                VisualScript.ScriptUpdated += OnVisualScriptScriptUpdated;
                VisualScript.PropertyChanged += OnVisualScriptPropertyChanged;
                VisualScript.NodeMoved += OnVisualScriptNodeMoved;
                VisualScript.NodeCollectionChanged += OnVisualScriptNodeCollectionChanged;

                if (_nodeList != null)
                    _nodeList.ItemsSource = VisualScript?.Nodes;

                if (_cableList != null)
                    _cableList.ItemsSource = VisualScript?.Cables;
            }

            VisualScript?.RecreateCables();

            if (AutoFitScript)
                FitScript();
            else
                CenterAt(new Vector(0, 0));
        }

        private void OnVisualScriptScriptUpdated(object sender, EventArgs e)
        {
            ScriptUpdated?.Invoke(this, EventArgs.Empty);
        }

        private void OnVisualScriptPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == nameof(VisualScript.Cables))
                if (_cableList != null)
                    _cableList.ItemsSource = VisualScript.Cables;
        }

        private void OnVisualScriptNodeMoved(object sender, EventArgs args)
        {
            if (AutoFitScript)
                FitScript();
        }

        private void OnVisualScriptNodeCollectionChanged(object sender, EventArgs e)
        {
            if (AutoFitScript)
                FitScript();
        }

        private void OnCanvasPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs args)
        {
            _lastRightClickLocation = args.GetPosition(_canvas);
        }

        private void OnCanvasMouseLeftButtonDown(object sender, MouseButtonEventArgs args)
        {
            if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
            {
                if (AutoFitScript)
                {
                    args.Handled = true;
                    return;
                }

                _dragCanvas = true;
                _dragCanvasStartLocation = args.GetPosition(this);
                _dragCanvasStartOffset = _viewportCenter;

                _movedDuringDrag = false;

                _canvas.CaptureMouse();

                args.Handled = true;

                return;
            }

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
                if (args.LeftButton == MouseButtonState.Pressed && (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
                {
                    Vector newLocation = _dragCanvasStartOffset - (((args.GetPosition(this) - _dragCanvasStartLocation)) * (1.0 / Scale));
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
            if (VisualScript.IsConnecting)
                VisualScript.OnDragOver(args.GetPosition(_canvas));
        }

        private void OnCanvasDrop(object sender, DragEventArgs args)
        {
            if (!args.Data.GetDataPresent(typeof(VisualScriptPin))) return;

            VisualScriptPin sourcePin = (VisualScriptPin) args.Data.GetData(typeof(VisualScriptPin));
            if (sourcePin == null) return;

            if (_creationBoxParent.ContextMenu != null)
            {
                SourcePin = sourcePin;

                _lastRightClickLocation = args.GetPosition(_canvas);
                _creationBoxParent.ContextMenu.IsOpen = true;
                _creationBoxParent.ContextMenu.DataContext = this;

                void ContextMenuOnClosed(object s, RoutedEventArgs e)
                {
                    SourcePin = null;
                    if (_creationBoxParent.ContextMenu != null)
                        _creationBoxParent.ContextMenu.Closed -= ContextMenuOnClosed;
                }

                _creationBoxParent.ContextMenu.Closed += ContextMenuOnClosed;
            }

            args.Handled = true;
        }

        private void OnCanvasMouseWheel(object sender, MouseWheelEventArgs args)
        {
            if (AutoFitScript)
            {
                args.Handled = true;
                return;
            }

            if (args.Delta < 0)
                Scale /= ScaleFactor;
            else
                Scale *= ScaleFactor;

            Scale = Clamp(Scale, MinScale, MaxScale);

            UpdatePanning();

            args.Handled = true;
        }

        private void SelectWithinRectangle(Rect rectangle)
        {
            if (Script == null) return;

            VisualScript.DeselectAllNodes();

            for (int i = 0; i < _nodeList.Items.Count; i++)
            {
                ContentPresenter nodeControl = (ContentPresenter) _nodeList.ItemContainerGenerator.ContainerFromIndex(i);
                VisualScriptNode node = (VisualScriptNode) nodeControl.Content;

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

            presenter._canvasViewPortScale.ScaleX = presenter.Scale;
            presenter._canvasViewPortScale.ScaleY = presenter.Scale;

            presenter.VisualScript.NodeDragScale = 1.0 / presenter.Scale;
        }

        private void FitScript()
        {
            if (_fitPending) return;

            _fitPending = true;
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(FitScriptAction));
        }

        private void FitScriptAction()
        {
            _fitPending = false;

            if ((Script == null) || (_nodeList == null)) return;

            double minX = double.MaxValue;
            double maxX = double.MinValue;
            double minY = double.MaxValue;
            double maxY = double.MinValue;
            for (int i = 0; i < _nodeList.Items.Count; i++)
            {
                DependencyObject container = _nodeList.ItemContainerGenerator.ContainerFromIndex(i);
                VisualScriptNodePresenter nodePresenter = GetChildOfType<VisualScriptNodePresenter>(container);
                if (nodePresenter != null)
                {
                    minX = Math.Min(minX, nodePresenter.Node.Node.X);
                    minY = Math.Min(minY, nodePresenter.Node.Node.Y);

                    maxX = Math.Max(maxX, nodePresenter.Node.Node.X + nodePresenter.ActualWidth);
                    maxY = Math.Max(maxY, nodePresenter.Node.Node.Y + nodePresenter.ActualHeight);
                }
            }

            if (minX >= (double.MaxValue - 1))
            {
                Scale = MaxScale;
                CenterAt(new Vector(0, 0));
            }
            else
            {
                double width = maxX - minX;
                double height = maxY - minY;

                double scaleX = ActualWidth / width;
                double scaleY = ActualHeight / height;

                Scale = Clamp(Math.Min(scaleX, scaleY) / 1.05, 0, MaxScale); //DarthAffe 21.08.2021: 5% Border

                CenterAt(new Vector(minX + (width / 2.0), minY + (height / 2.0)));
            }
        }

        private void CreateNode(NodeData nodeData)
        {
            if (nodeData == null) return;

            INode node = nodeData.CreateNode(Script, null);
            node.X = _lastRightClickLocation.X - VisualScript.LocationOffset;
            node.Y = _lastRightClickLocation.Y - VisualScript.LocationOffset;

            if (SourcePin != null)
            {
                // Connect to the first matching input or output pin
                List<IPin> pins = node.Pins.ToList();
                pins.AddRange(node.PinCollections.SelectMany(c => c));
                pins = pins.Where(p => p.Type == typeof(object) || p.Type.IsAssignableFrom(SourcePin.Pin.Type)).OrderBy(p => p.Type != typeof(object)).ToList();

                IPin preferredPin = SourcePin.Pin.Direction == PinDirection.Input
                    ? pins.FirstOrDefault(p => p.Direction == PinDirection.Output)
                    : pins.FirstOrDefault(p => p.Direction == PinDirection.Input);

                if (preferredPin != null)
                {
                    preferredPin.ConnectTo(SourcePin.Pin);
                    SourcePin.Pin.ConnectTo(preferredPin);
                }
            }

            if (_creationBoxParent.ContextMenu != null)
                _creationBoxParent.ContextMenu.IsOpen = false;
            
            Script.AddNode(node);
            VisualScript.OnScriptUpdated();
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
            _canvasViewPortTransform.X = (((-_viewportCenter.X * Scale) + (ActualWidth / 2.0))) - surfaceOffset;
            _canvasViewPortTransform.Y = (((-_viewportCenter.Y * Scale) + (ActualHeight / 2.0))) - surfaceOffset;
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

        public static T GetChildOfType<T>(DependencyObject obj)
            where T : DependencyObject
        {
            if (obj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(obj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(obj, i);

                T result = (child as T) ?? GetChildOfType<T>(child);
                if (result != null) return result;
            }

            return null;
        }

        #endregion
    }
}