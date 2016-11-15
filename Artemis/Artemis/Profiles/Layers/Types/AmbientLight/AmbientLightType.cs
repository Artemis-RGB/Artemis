using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator;
using Artemis.Profiles.Layers.Types.AmbientLight.Model;
using Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions;
using Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing;
using Artemis.Properties;
using Artemis.Utilities;
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
        private AmbienceCreatorType? _lastAmbienceCreatorType = null;
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
            byte[] newData = GetAmbienceCreator(properties).GetAmbience(data, ScreenCaptureManager.LastCaptureWidth, ScreenCaptureManager.LastCaptureHeight, width, height, properties);

            _lastData = _lastData?.Blend(newData, properties.SmoothMode) ?? newData;
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
            Rect thumbnailRect = new Rect(0, 0, 18, 18);
            DrawingVisual visual = new DrawingVisual();
            using (DrawingContext c = visual.RenderOpen())
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.ambilight), thumbnailRect);

            return new DrawingImage(visual.Drawing);
        }

        private IAmbienceCreator GetAmbienceCreator(AmbientLightPropertiesModel properties)
        {
            if (_lastAmbienceCreatorType == properties.AmbienceCreatorType)
                return _lastAmbienceCreator;

            _lastAmbienceCreatorType = properties.AmbienceCreatorType;
            switch (properties.AmbienceCreatorType)
            {
                case AmbienceCreatorType.Mirror: return _lastAmbienceCreator = new AmbienceCreatorMirror();
                case AmbienceCreatorType.Extend: return _lastAmbienceCreator = new AmbienceCreatorExtend();
                default: throw new InvalidEnumArgumentException();
            }
        }

        #endregion
    }
}
