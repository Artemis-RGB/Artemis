using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator;
using Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing;
using Artemis.ViewModels.Profiles;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Types.AmbientLight
{
    public class AmbientLightType : ILayerType
    {
        #region Properties & Fields

        public string Name => "Keyboard - Ambient Light";
        public bool ShowInEdtor => true;
        public DrawType DrawType => DrawType.Keyboard;

        [JsonIgnore]
        private IAmbienceCreator _lastAmbienceCreator;

        [JsonIgnore]
        private byte[] _lastData;

        #endregion

        #region Methods

        public LayerPropertiesViewModel SetupViewModel(LayerEditorViewModel layerEditorViewModel,
            LayerPropertiesViewModel layerPropertiesViewModel)
        {
            if (layerPropertiesViewModel is AmbientLightPropertiesViewModel)
                return layerPropertiesViewModel;
            return new AmbientLightPropertiesViewModel(layerEditorViewModel);
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is AmbientLightPropertiesModel)
                return;

            layerModel.Properties = new AmbientLightPropertiesModel(layerModel.Properties);
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            AmbientLightPropertiesModel properties = layerModel?.Properties as AmbientLightPropertiesModel;
            if (properties == null) return;

            int width = (int)Math.Round(properties.Width);
            int height = (int)Math.Round(properties.Height);

            byte[] data = ScreenCaptureManager.GetLastScreenCapture();
            _lastData = GetAmbienceCreator().GetAmbience(data, ScreenCaptureManager.LastCaptureWidth, ScreenCaptureManager.LastCaptureHeight, width, height, properties);

            int stride = (width * ScreenCaptureManager.LastCapturePixelFormat.BitsPerPixel + 7) / 8;
            properties.AmbientLightBrush = new DrawingBrush(new ImageDrawing
                (BitmapSource.Create(width, height, 96, 96, ScreenCaptureManager.LastCapturePixelFormat, null, _lastData, stride), new Rect(0, 0, width, height)));
        }

        public void Draw(LayerModel layer, DrawingContext c)
        {
            Rect rect = new Rect(layer.Properties.X * 4,
                                 layer.Properties.Y * 4,
                                 layer.Properties.Width * 4,
                                 layer.Properties.Height * 4);

            c.DrawRectangle(((AmbientLightPropertiesModel)layer.Properties).AmbientLightBrush, null, rect);
        }

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            //TODO DarthAffe 30.10.2016: Add a real thumbnail
            Rect thumbnailRect = new Rect(0, 0, 18, 18);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext c = visual.RenderOpen())
                c.DrawRectangle(new SolidColorBrush(Colors.Magenta), new Pen(new SolidColorBrush(Colors.DarkMagenta), 2), thumbnailRect);

            return new DrawingImage(visual.Drawing);
        }

        private IAmbienceCreator GetAmbienceCreator()
        {
            //TODO DarthAffe 30.10.2016: Create from settings
            return _lastAmbienceCreator ?? (_lastAmbienceCreator = new AmbienceCreatorMirror());
        }

        #endregion
    }
}
