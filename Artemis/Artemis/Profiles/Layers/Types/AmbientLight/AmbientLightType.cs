using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Modules.Abstract;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator;
using Artemis.Profiles.Layers.Types.AmbientLight.Model;
using Artemis.Profiles.Layers.Types.AmbientLight.Model.Extensions;
using Artemis.Profiles.Layers.Types.AmbientLight.ScreenCapturing;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels;
using Newtonsoft.Json;

namespace Artemis.Profiles.Layers.Types.AmbientLight
{
    public class AmbientLightType : ILayerType
    {
        #region Properties & Fields

        public string Name => "Keyboard - Ambient Light";
        public bool ShowInEdtor => true;
        public DrawType DrawType => DrawType.Keyboard;
        public int DrawScale => 4;

        [JsonIgnore] private AmbienceCreatorType? _lastAmbienceCreatorType;

        [JsonIgnore] private IAmbienceCreator _lastAmbienceCreator;

        [JsonIgnore] private byte[] _lastData;

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

        public void Update(LayerModel layerModel, ModuleDataModel dataModel, bool isPreview = false)
        {
            var properties = layerModel?.Properties as AmbientLightPropertiesModel;
            if (properties == null) return;

            var width = (int) Math.Round(properties.Width);
            var height = (int) Math.Round(properties.Height);

            var data = ScreenCaptureManager.GetLastScreenCapture();
            var newData = GetAmbienceCreator(properties)
                .GetAmbience(data, ScreenCaptureManager.LastCaptureWidth, ScreenCaptureManager.LastCaptureHeight, width,
                    height, properties);

            _lastData = _lastData?.Blend(newData, properties.SmoothMode) ?? newData;
            var stride = (width*ScreenCaptureManager.LastCapturePixelFormat.BitsPerPixel + 7)/8;
            properties.AmbientLightBrush = new DrawingBrush(new ImageDrawing
                (BitmapSource.Create(width, height, 96, 96, ScreenCaptureManager.LastCapturePixelFormat, null, _lastData,
                    stride), new Rect(0, 0, width, height)));

            layerModel.ApplyProperties(true);
        }

        public void Draw(LayerModel layerModel, DrawingContext c)
        {
            var rect = layerModel.LayerRect(DrawScale);
            c.DrawRectangle(((AmbientLightPropertiesModel) layerModel.Properties).AmbientLightBrush, null, rect);
        }

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
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
                case AmbienceCreatorType.Mirror:
                    return _lastAmbienceCreator = new AmbienceCreatorMirror();
                case AmbienceCreatorType.Extend:
                    return _lastAmbienceCreator = new AmbienceCreatorExtend();
                default:
                    throw new InvalidEnumArgumentException();
            }
        }

        #endregion
    }
}