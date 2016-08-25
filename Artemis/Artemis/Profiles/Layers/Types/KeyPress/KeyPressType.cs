using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Artemis.Managers;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Animations;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.Utilities.Keyboard;

namespace Artemis.Profiles.Layers.Types.KeyPress
{
    internal class KeyPressType : ILayerType
    {
        private readonly MainManager _mainManager;
        private List<LayerModel> _keyPressLayers = new List<LayerModel>();
        private LayerModel _layerModel;

        public KeyPressType(MainManager mainManager)
        {
            _mainManager = mainManager;
            KeyboardHook.KeyDownCallback += KeyboardHookOnKeyDownCallback;
        }

        public RadialGradientBrush TempBrush { get; set; }


        public string Name { get; } = "Keyboard - Key press";
        public bool ShowInEdtor { get; } = false;
        public DrawType DrawType { get; } = DrawType.Keyboard;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.gif), thumbnailRect);
            }

            var image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layer, DrawingContext c)
        {
            lock (_keyPressLayers)
            {
                foreach (var keyPressLayer in _keyPressLayers)
                    keyPressLayer.LayerType.Draw(keyPressLayer, c);
            }
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            // Key press is always as large as the entire keyboard it is drawn for
            layerModel.Properties.Width = _mainManager.DeviceManager.ActiveKeyboard.Width;
            layerModel.Properties.Height = _mainManager.DeviceManager.ActiveKeyboard.Height;
            layerModel.Properties.X = 0;
            layerModel.Properties.Y = 0;
            layerModel.Properties.Contain = true;

            _layerModel = layerModel;

            if (isPreview)
                return;

            lock (_keyPressLayers)
            {
                // Remove expired key presses
                _keyPressLayers = _keyPressLayers.Where(k => !k.LayerAnimation.MustExpire(k)).ToList();
                // Update the ones that are still active
                foreach (var keyPressLayer in _keyPressLayers)
                    keyPressLayer.Update(null, false, true);
            }
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is KeyPressPropertiesModel)
                return;

            layerModel.Properties = new KeyPressPropertiesModel(layerModel.Properties);
        }

        public LayerPropertiesViewModel SetupViewModel(LayerPropertiesViewModel layerPropertiesViewModel,
            List<ILayerAnimation> layerAnimations, IDataModel dataModel, LayerModel proposedLayer)
        {
            if (layerPropertiesViewModel is KeyPressPropertiesViewModel)
                return layerPropertiesViewModel;
            return new KeyPressPropertiesViewModel(proposedLayer, dataModel);
        }

        private void KeyboardHookOnKeyDownCallback(KeyEventArgs e)
        {
            if (_layerModel == null)
                return;

            // Reset animation progress if layer wasn't drawn for 100ms
            if (new TimeSpan(0, 0, 0, 0, 100) < DateTime.Now - _layerModel.LastRender)
                return;

            lock (_keyPressLayers)
            {
                if (_keyPressLayers.Count >= 25)
                    return;
            }

            var keyMatch = _mainManager.DeviceManager.ActiveKeyboard.GetKeyPosition(e.KeyCode);
            if (keyMatch == null)
                return;

            lock (_keyPressLayers)
            {
                var properties = (KeyPressPropertiesModel) _layerModel.Properties;
                var layer = LayerModel.CreateLayer();
                layer.Properties.X = keyMatch.Value.X - properties.Scale/2;
                layer.Properties.Y = keyMatch.Value.Y - properties.Scale/2;
                layer.Properties.Width = properties.Scale;
                layer.Properties.Height = properties.Scale;
                layer.Properties.AnimationSpeed = properties.AnimationSpeed;
                layer.LayerAnimation = new GrowAnimation();

                // Setup the brush according to settings
                layer.Properties.Brush = properties.RandomColor
                    ? ColorHelpers.RandomizeBrush(properties.Brush)
                    : properties.Brush.CloneCurrentValue();

                _keyPressLayers.Add(layer);
            }
        }
    }
}