using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles.Properties;
using Artemis.Utilities;
using Artemis.Utilities.ParentChild;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Artemis.Models.Profiles
{
    public class ProfileModel
    {
        public ProfileModel()
        {
            Layers = new ChildItemCollection<ProfileModel, LayerModel>(this);
            DrawingVisual = new DrawingVisual();
        }

        public ChildItemCollection<ProfileModel, LayerModel> Layers { get; }

        public string Name { get; set; }
        public bool IsDefault { get; set; }
        public string KeyboardSlug { get; set; }
        public string GameName { get; set; }

        [XmlIgnore]
        public DrawingVisual DrawingVisual { get; set; }

        protected bool Equals(ProfileModel other)
        {
            return string.Equals(Name, other.Name) &&
                   string.Equals(KeyboardSlug, other.KeyboardSlug) &&
                   string.Equals(GameName, other.GameName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((ProfileModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Name?.GetHashCode() ?? 0;
                hashCode = (hashCode*397) ^ (KeyboardSlug?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (GameName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        public void FixOrder()
        {
            Layers.Sort(l => l.Order);
            for (var i = 0; i < Layers.Count; i++)
                Layers[i].Order = i;
        }

        public Bitmap GenerateBitmap<T>(Rect keyboardRect, IGameDataModel gameDataModel, bool preview,
            bool updateAnimations)
        {
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                // Setup the DrawingVisual's size
                c.PushClip(new RectangleGeometry(keyboardRect));
                c.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, keyboardRect);

                // Draw the layers
                foreach (var layerModel in Layers.OrderByDescending(l => l.Order))
                    layerModel.Draw<T>(gameDataModel, c, preview, updateAnimations);

                // Remove the clip
                c.Pop();
            }

            return ImageUtilities.DrawinVisualToBitmap(visual, keyboardRect); 
        }

        public Brush GenerateBrush<T>(IGameDataModel gameDataModel, LayerType type, bool preview, bool updateAnimations)
        {
            Brush result = null;
            // Draw the layers
            foreach (var layerModel in Layers.OrderByDescending(l => l.Order))
            {
                var generated = layerModel.GenerateBrush<T>(type, gameDataModel, preview, updateAnimations);
                if (generated != null)
                    result = generated;
            }

            return result;
        }

        /// <summary>
        ///     Gives all the layers and their children in a flat list
        /// </summary>
        public List<LayerModel> GetLayers(bool ignoreEnabled = true)
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Layers)
            {
                if (ignoreEnabled && !layerModel.Enabled)
                {
                    continue;
                }
                layers.Add(layerModel);
                layers.AddRange(layerModel.GetAllLayers(ignoreEnabled));
            }

            return layers;
        }

        /// <summary>
        ///     Looks at all the layers wthin the profile and makes sure they are within boundaries of the given rectangle
        /// </summary>
        /// <param name="keyboardRectangle"></param>
        public void FixBoundaries(Rect keyboardRectangle)
        {
            foreach (var layer in GetLayers(false))
            {
                if (layer.LayerType != LayerType.Keyboard && layer.LayerType != LayerType.KeyboardGif)
                    continue;

                var props = (KeyboardPropertiesModel) layer.Properties;
                var layerRect = new Rect(new Point(props.X, props.Y), new Size(props.Width, props.Height));
                if (keyboardRectangle.Contains(layerRect))
                    continue;

                props.X = 0;
                props.Y = 0;
                layer.Properties = props;
            }
        }
    }
}