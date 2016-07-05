using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Properties;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles.Layers;
using NClone;

namespace Artemis.Profiles.Layers.Types.KeyboardGif
{
    internal class KeyboardGifType : ILayerType
    {
        public string Name { get; } = "Keyboard - GIF";
        public bool MustDraw { get; } = true;

        public ImageSource DrawThumbnail(LayerModel layer)
        {
            var thumbnailRect = new Rect(0, 0, 18, 18);
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(Resources.gif), thumbnailRect);

            var image = new DrawingImage(visual.Drawing);
            return image;
        }

        public void Draw(LayerModel layer, DrawingContext c)
        {
            var props = (KeyboardPropertiesModel) layer.Properties;
            if (string.IsNullOrEmpty(props.GifFile))
                return;
            if (!File.Exists(props.GifFile))
                return;

            // Only reconstruct GifImage if the underlying source has changed
            if (layer.GifImage == null)
                layer.GifImage = new GifImage(props.GifFile);
            if (layer.GifImage.Source != props.GifFile)
                layer.GifImage = new GifImage(props.GifFile);

            var rect = new Rect(layer.AppliedProperties.X*4,
                layer.AppliedProperties.Y*4,
                layer.AppliedProperties.Width*4,
                layer.AppliedProperties.Height*4);

            lock (layer.GifImage)
            {
                var draw = layer.GifImage.GetNextFrame();
                c.DrawImage(ImageUtilities.BitmapToBitmapImage(new Bitmap(draw)), rect);
            }
        }

        public void Update(LayerModel layerModel, IDataModel dataModel, bool isPreview = false)
        {
            layerModel.AppliedProperties = Clone.ObjectGraph(layerModel.Properties);
            if (isPreview)
                return;

            // If not previewing, apply dynamic properties according to datamodel
            var keyboardProps = (KeyboardPropertiesModel) layerModel.AppliedProperties;
            foreach (var dynamicProperty in keyboardProps.DynamicProperties)
                dynamicProperty.ApplyProperty(dataModel, layerModel.AppliedProperties);
        }

        public void SetupProperties(LayerModel layerModel)
        {
            if (layerModel.Properties is KeyboardPropertiesModel)
                return;

            layerModel.Properties = new KeyboardPropertiesModel(layerModel.Properties);
        }

        public LayerPropertiesViewModel SetupViewModel(LayerPropertiesViewModel layerPropertiesViewModel,
            List<ILayerAnimation> layerAnimations, IDataModel dataModel, LayerModel proposedLayer)
        {
            var model = layerPropertiesViewModel as KeyboardPropertiesViewModel;
            if (model == null)
                return new KeyboardPropertiesViewModel(proposedLayer, dataModel, layerAnimations)
                {
                    IsGif = true
                };

            model.IsGif = true;
            return layerPropertiesViewModel;
        }
    }
}