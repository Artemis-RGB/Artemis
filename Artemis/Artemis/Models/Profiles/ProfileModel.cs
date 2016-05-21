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
        public string KeyboardName { get; set; }
        public string GameName { get; set; }

        [XmlIgnore]
        public DrawingVisual DrawingVisual { get; set; }

        protected bool Equals(ProfileModel other)
        {
            return string.Equals(Name, other.Name) &&
                   string.Equals(KeyboardName, other.KeyboardName) &&
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
                hashCode = (hashCode*397) ^ (KeyboardName?.GetHashCode() ?? 0);
                hashCode = (hashCode*397) ^ (GameName?.GetHashCode() ?? 0);
                return hashCode;
            }
        }

        /// <summary>
        ///     Adds a new layer with default settings to the profile
        /// </summary>
        /// <returns>The newly added layer</returns>
        public LayerModel AddLayer()
        {
            var layer = new LayerModel
            {
                Name = "New layer",
                Enabled = true,
                Order = -1,
                LayerType = LayerType.Keyboard,
                Properties = new KeyboardPropertiesModel
                {
                    Brush = new SolidColorBrush(ColorHelpers.GetRandomRainbowMediaColor()),
                    Animation = LayerAnimation.None,
                    Height = 1,
                    Width = 1,
                    X = 0,
                    Y = 0,
                    Opacity = 1
                }
            };

            Layers.Add(layer);
            FixOrder();

            return layer;
        }

        public void Reorder(LayerModel selectedLayer, bool moveUp)
        {
            // Fix the sorting just in case
            FixOrder();

            int newOrder;
            if (moveUp)
                newOrder = selectedLayer.Order - 1;
            else
                newOrder = selectedLayer.Order + 1;

            var target = Layers.FirstOrDefault(l => l.Order == newOrder);
            if (target == null)
                return;

            if (target.LayerType == LayerType.Folder)
            {
                if (selectedLayer.Parent == null)
                    selectedLayer.Profile.Layers.Remove(selectedLayer);
                else
                    selectedLayer.Parent.Children.Remove(selectedLayer);

                target.Children.Add(selectedLayer);
                
                        
                if (moveUp)
                {
                    var parentTarget = target.Children.OrderBy(c => c.Order).LastOrDefault();
                    if (parentTarget != null)
                        selectedLayer.Order = parentTarget.Order + 1;
                    else
                        selectedLayer.Order = 1;
                }
                else
                {
                    var parentTarget = target.Children.OrderBy(c => c.Order).LastOrDefault();
                    if (parentTarget != null)
                        selectedLayer.Order = parentTarget.Order - 1;
                    else
                        selectedLayer.Order = 1;
                }
                target.FixOrder();
            }
            target.Order = selectedLayer.Order;
            selectedLayer.Order = newOrder;
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
            Bitmap bitmap = null;
            DrawingVisual.Dispatcher.Invoke(() =>
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

                bitmap = ImageUtilities.DrawinVisualToBitmap(visual, keyboardRect);
            });
            return bitmap;
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
        /// Gives all the layers and their children in a flat list
        /// </summary>
        public List<LayerModel> GetAllLayers()
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Layers)
            {
                layers.Add(layerModel);
                layers.AddRange(layerModel.GetAllLayers());
            }
            
            return layers;
        }
    }
}