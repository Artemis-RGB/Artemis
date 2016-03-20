using System.Collections.Generic;
using Artemis.Components;

namespace Artemis.Models
{
    public class ProfileModel
    {
        public ProfileModel(string name, string keyboardName, string gameName)
        {
            Name = name;
            KeyboardName = keyboardName;
            GameName = gameName;
        }

        public string Name { get; set; }
        public string KeyboardName { get; set; }
        public string GameName { get; set; }

        public LayerComposite Layers { get; set; }

        protected bool Equals(ProfileModel other)
        {
            return string.Equals(Name, other.Name) && string.Equals(KeyboardName, other.KeyboardName) && string.Equals(GameName, other.GameName);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ProfileModel) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (KeyboardName != null ? KeyboardName.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (GameName != null ? GameName.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}