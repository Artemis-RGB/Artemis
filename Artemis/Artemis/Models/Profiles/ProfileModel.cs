using System.Collections.Generic;
using System.Windows.Media;
using System.Xml.Serialization;
using Artemis.Utilities;
using CUE.NET.Helper;

namespace Artemis.Models.Profiles
{
    public class ProfileModel
    {
        public ProfileModel()
        {
            Layers = new List<LayerModel>();
            DrawingVisual = new DrawingVisual();
        }

        public string Name { get; set; }
        public string KeyboardName { get; set; }
        public string GameName { get; set; }
        public DrawingVisual DrawingVisual { get; set; }

        public List<LayerModel> Layers { get; set; }

        protected bool Equals(ProfileModel other)
        {
            return string.Equals(Name, other.Name) && string.Equals(KeyboardName, other.KeyboardName) &&
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
                LayerType = LayerType.KeyboardRectangle,
                UserProps = new LayerPropertiesModel
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
            return layer;
        }
    }
}