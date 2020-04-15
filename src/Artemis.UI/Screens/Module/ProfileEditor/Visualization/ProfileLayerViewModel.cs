using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Artemis.Core.Models.Profile;
using Artemis.Core.Models.Profile.LayerShapes;
using Artemis.UI.Services.Interfaces;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.Visualization
{
    public class ProfileLayerViewModel : CanvasViewModel
    {
        private readonly ILayerEditorService _layerEditorService;
        private readonly IProfileEditorService _profileEditorService;

        public ProfileLayerViewModel(Layer layer, IProfileEditorService profileEditorService, ILayerEditorService layerEditorService)
        {
            _profileEditorService = profileEditorService;
            _layerEditorService = layerEditorService;
            Layer = layer;

            Update();
            Layer.RenderPropertiesUpdated += LayerOnRenderPropertiesUpdated;
            _profileEditorService.ProfileElementSelected += OnProfileElementSelected;
            _profileEditorService.SelectedProfileElementUpdated += OnSelectedProfileElementUpdated;
            _profileEditorService.ProfilePreviewUpdated += ProfileEditorServiceOnProfilePreviewUpdated;
        }

        public Layer Layer { get; }
        public Rect LayerRect { get; set; }
        public Thickness LayerRectMargin => LayerRect == Rect.Empty ? new Thickness() : new Thickness(LayerRect.Left, LayerRect.Top, 0, 0);
        public bool IsSelected { get; set; }
        public Geometry ShapeGeometry { get; set; }

        public void Dispose()
        {
            Layer.RenderPropertiesUpdated -= LayerOnRenderPropertiesUpdated;
            _profileEditorService.ProfileElementSelected -= OnProfileElementSelected;
            _profileEditorService.SelectedProfileElementUpdated -= OnSelectedProfileElementUpdated;
            _profileEditorService.ProfilePreviewUpdated -= ProfileEditorServiceOnProfilePreviewUpdated;
        }

        private void Update()
        {
            IsSelected = _profileEditorService.SelectedProfileElement == Layer;
            if (!Layer.Leds.Any() || Layer.LayerShape == null)
                LayerRect = Rect.Empty;
            else
                LayerRect = _layerEditorService.GetLayerBounds(Layer);

            CreateShapeGeometry();
        }

        private void CreateShapeGeometry()
        {
            if (Layer.LayerShape == null || !Layer.Leds.Any())
            {
                ShapeGeometry = Geometry.Empty;
                return;
            }

            Execute.PostToUIThread(() =>
            {
                var bounds = _layerEditorService.GetLayerBounds(Layer);
                var shapeGeometry = Geometry.Empty;
                switch (Layer.LayerShape)
                {
                    case Ellipse _:
                        shapeGeometry = new EllipseGeometry(bounds);
                        break;
                    case Rectangle _:
                        shapeGeometry = new RectangleGeometry(bounds);
                        break;
                }

                shapeGeometry.Transform = _layerEditorService.GetLayerTransformGroup(Layer);
                ShapeGeometry = shapeGeometry;
            });
        }

        #region Event handlers

        private void LayerOnRenderPropertiesUpdated(object sender, EventArgs e)
        {
            Update();
        }

        private void OnProfileElementSelected(object sender, EventArgs e)
        {
            IsSelected = _profileEditorService.SelectedProfileElement == Layer;
        }

        private void OnSelectedProfileElementUpdated(object sender, EventArgs e)
        {
            Update();
        }

        private void ProfileEditorServiceOnProfilePreviewUpdated(object sender, EventArgs e)
        {
            Update();
        }

        #endregion
    }
}