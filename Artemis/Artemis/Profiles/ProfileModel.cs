using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Headset;
using Artemis.Profiles.Layers.Types.Keyboard;
using Artemis.Profiles.Layers.Types.Mouse;
using Artemis.Utilities;
using Artemis.Utilities.ParentChild;
using Color = System.Windows.Media.Color;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Artemis.Profiles
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

        public void FixOrder()
        {
            Layers.Sort(l => l.Order);
            for (var i = 0; i < Layers.Count; i++)
                Layers[i].Order = i;
        }

        public void DrawProfile(Graphics keyboard, Rect keyboardRect, IDataModel dataModel, bool preview,
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
                    layerModel.Draw(dataModel, c, preview, updateAnimations);

                // Remove the clip
                c.Pop();
            }

            using (var bmp = ImageUtilities.DrawinVisualToBitmap(visual, keyboardRect))
                keyboard.DrawImage(bmp, new PointF(0, 0));
        }

        /// <summary>
        ///     Gives all the layers and their children in a flat list
        /// </summary>
        public List<LayerModel> GetLayers()
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Layers)
            {
                layers.Add(layerModel);
                layers.AddRange(layerModel.GetLayers());
            }

            return layers;
        }

        /// <summary>
        ///     Generates a flat list containing all layers that must be rendered on the keyboard,
        ///     the first mouse layer to be rendered and the first headset layer to be rendered
        /// </summary>
        /// <typeparam name="T">The game data model to base the conditions on</typeparam>
        /// <param name="dataModel">Instance of said game data model</param>
        /// <param name="includeMice">Whether or not to include mice in the list</param>
        /// <param name="includeHeadsets">Whether or not to include headsets in the list</param>
        /// <param name="ignoreConditions"></param>
        /// <returns>A flat list containing all layers that must be rendered</returns>
        public List<LayerModel> GetRenderLayers(IDataModel dataModel, bool includeMice, bool includeHeadsets,
            bool ignoreConditions = false)
        {
            var layers = new List<LayerModel>();
            foreach (var layerModel in Layers.OrderByDescending(l => l.Order))
            {
                if (!layerModel.Enabled || !includeMice && layerModel.LayerType is MouseType ||
                    !includeHeadsets && layerModel.LayerType is HeadsetType)
                    continue;

                if (!ignoreConditions & !layerModel.ConditionsMet(dataModel))
                    continue;

                layers.Add(layerModel);
                layers.AddRange(layerModel.GetRenderLayers(dataModel, includeMice, includeHeadsets, ignoreConditions));
            }

            return layers;
        }

        /// <summary>
        ///     Looks at all the layers wthin the profile and makes sure they are within boundaries of the given rectangle
        /// </summary>
        /// <param name="keyboardRectangle"></param>
        public void FixBoundaries(Rect keyboardRectangle)
        {
            foreach (var layer in GetLayers())
            {
                if (!layer.LayerType.MustDraw)
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

        /// <summary>
        ///     Draw all the provided layers of type Keyboard and KeyboardGif
        /// </summary>
        /// <param name="keyboard">The graphics to draw on</param>
        /// <param name="renderLayers">The layers to render</param>
        /// <param name="dataModel">The data model to base the layer's properties on</param>
        /// <param name="keyboardRect">A rectangle matching the current keyboard's size on a scale of 4, used for clipping</param>
        /// <param name="preview">Indicates wheter the layer is drawn as a preview, ignoring dynamic properties</param>
        /// <param name="updateAnimations">Wheter or not to update the layer's animations</param>
        internal void DrawProfile(Graphics keyboard, List<LayerModel> renderLayers, IDataModel dataModel,
            Rect keyboardRect, bool preview, bool updateAnimations)
        {
            var visual = new DrawingVisual();
            using (var c = visual.RenderOpen())
            {
                // Setup the DrawingVisual's size
                c.PushClip(new RectangleGeometry(keyboardRect));
                c.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, keyboardRect);

                // Draw the layers
                foreach (var layerModel in renderLayers.Where(l => l.LayerType.MustDraw))
                    layerModel.Draw(dataModel, c, preview, updateAnimations);

                // Remove the clip
                c.Pop();
            }

            using (var bmp = ImageUtilities.DrawinVisualToBitmap(visual, keyboardRect))
                keyboard.DrawImage(bmp, new PointF(0, 0));
        }

        #region Compare

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

        #endregion
    }
}