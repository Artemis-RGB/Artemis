using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core;
using Artemis.UI.Extensions;
using Artemis.UI.Screens.Shared;
using Artemis.UI.Services;
using Artemis.UI.Shared.Services;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Visualization
{
    public class ProfileLayerViewModel : CanvasViewModel
    {
        private readonly ILayerEditorService _layerEditorService;
        private readonly PanZoomViewModel _panZoomViewModel;
        private readonly IProfileEditorService _profileEditorService;
        private bool _isSelected;
        private Geometry _shapeGeometry;
        private Rect _viewportRectangle;

        public ProfileLayerViewModel(Layer layer, PanZoomViewModel panZoomViewModel, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService)
        {
            _panZoomViewModel = panZoomViewModel;
            _profileEditorService = profileEditorService;
            _layerEditorService = layerEditorService;
            Layer = layer;
        }

        public Layer Layer { get; }

        public Geometry ShapeGeometry
        {
            get => _shapeGeometry;
            set => SetAndNotify(ref _shapeGeometry, value);
        }

        public Rect ViewportRectangle
        {
            get => _viewportRectangle;
            set
            {
                if (!SetAndNotify(ref _viewportRectangle, value)) return;
                NotifyOfPropertyChange(nameof(LayerPosition));
            }
        }

        public double StrokeThickness
        {
            get
            {
                if (IsSelected)
                    return Math.Max(2 / _panZoomViewModel.Zoom, 1);
                return Math.Max(2 / _panZoomViewModel.Zoom, 1) / 2;
            }
        }

        public double LayerStrokeThickness
        {
            get
            {
                if (IsSelected)
                    return StrokeThickness / 2;
                return StrokeThickness;
            }
        }

        public Thickness LayerPosition => new(ViewportRectangle.Left, ViewportRectangle.Top, 0, 0);

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (!SetAndNotify(ref _isSelected, value)) return;
                NotifyOfPropertyChange(nameof(StrokeThickness));
            }
        }

        private void PanZoomViewModelOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_panZoomViewModel.Zoom))
            {
                NotifyOfPropertyChange(nameof(StrokeThickness));
                NotifyOfPropertyChange(nameof(LayerStrokeThickness));
            }
        }

        private void Update()
        {
            IsSelected = _profileEditorService.SelectedProfileElement == Layer;

            CreateShapeGeometry();
            CreateViewportRectangle();
        }

        #region Updating

        private LayerShape _lastShape;
        private Rect _lastBounds;

        private SKPoint _lastAnchor;
        private SKPoint _lastPosition;
        private double _lastRotation;
        private SKSize _lastScale;

        private void CreateShapeGeometry()
        {
            if (Layer.LayerShape == null || !Layer.Leds.Any())
            {
                ShapeGeometry = Geometry.Empty;
                return;
            }

            Rect bounds = _layerEditorService.GetLayerBounds(Layer);

            // Only bother the UI thread for both of these if we're sure
            if (HasShapeChanged(bounds))
            {
                Execute.OnUIThreadSync(() =>
                {
                    if (Layer.LayerShape is RectangleShape)
                        ShapeGeometry = new RectangleGeometry(bounds);
                    if (Layer.LayerShape is EllipseShape)
                        ShapeGeometry = new EllipseGeometry(bounds);
                });
            }
            if ((Layer.LayerBrush == null || Layer.LayerBrush.SupportsTransformation) && HasTransformationChanged())
            {
                Execute.OnUIThreadSync(() => ShapeGeometry.Transform = _layerEditorService.GetLayerTransformGroup(Layer));
            }
        }

        private void CreateViewportRectangle()
        {
            if (!Layer.Leds.Any() || Layer.LayerShape == null)
            {
                ViewportRectangle = Rect.Empty;
                return;
            }

            ViewportRectangle = _layerEditorService.GetLayerBounds(Layer);
        }

        private bool HasShapeChanged(Rect bounds)
        {
            bool result = !Equals(_lastBounds, bounds) || !Equals(_lastShape, Layer.LayerShape);
            _lastShape = Layer.LayerShape;
            _lastBounds = bounds;
            return result;
        }

        private bool HasTransformationChanged()
        {
            bool result = _lastAnchor != Layer.Transform.AnchorPoint.CurrentValue ||
                          _lastPosition != Layer.Transform.Position.CurrentValue ||
                          _lastRotation != Layer.Transform.Rotation.CurrentValue ||
                          _lastScale != Layer.Transform.Scale.CurrentValue;

            _lastAnchor = Layer.Transform.AnchorPoint.CurrentValue;
            _lastPosition = Layer.Transform.Position.CurrentValue;
            _lastRotation = Layer.Transform.Rotation.CurrentValue;
            _lastScale = Layer.Transform.Scale.CurrentValue;

            return result;
        }

        #endregion

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            Update();
            Layer.RenderPropertiesUpdated += LayerOnRenderPropertiesUpdated;
            _profileEditorService.SelectedProfileElementChanged += OnSelectedProfileElementChanged;
            _profileEditorService.SelectedProfileElementSaved += OnSelectedProfileElementSaved;
            _profileEditorService.ProfilePreviewUpdated += ProfileEditorServiceOnProfilePreviewUpdated;
            _panZoomViewModel.PropertyChanged += PanZoomViewModelOnPropertyChanged;
            base.OnInitialActivate();
        }

        /// <inheritdoc />
        protected override void OnClose()
        {
            Layer.RenderPropertiesUpdated -= LayerOnRenderPropertiesUpdated;
            _profileEditorService.SelectedProfileElementChanged -= OnSelectedProfileElementChanged;
            _profileEditorService.SelectedProfileElementSaved -= OnSelectedProfileElementSaved;
            _profileEditorService.ProfilePreviewUpdated -= ProfileEditorServiceOnProfilePreviewUpdated;
            _panZoomViewModel.PropertyChanged -= PanZoomViewModelOnPropertyChanged;
            base.OnClose();
        }

        #endregion

        #region Event handlers

        private void LayerOnRenderPropertiesUpdated(object sender, EventArgs e)
        {
            Update();
        }

        private void OnSelectedProfileElementChanged(object sender, EventArgs e)
        {
            IsSelected = _profileEditorService.SelectedProfileElement == Layer;
        }

        private void OnSelectedProfileElementSaved(object sender, EventArgs e)
        {
            Update();
        }

        private void ProfileEditorServiceOnProfilePreviewUpdated(object sender, EventArgs e)
        {
            if (!Layer.Transform.PropertiesInitialized || !Layer.General.PropertiesInitialized)
                return;

            CreateShapeGeometry();
            CreateViewportRectangle();
        }

        #endregion
    }
}