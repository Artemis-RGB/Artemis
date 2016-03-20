using Artemis.Components;

namespace Artemis.Models
{
    public class ProfileModel
    {
        public string Name { get; set; }

        public string KeyboardName { get; set; }
        public string GameName { get; set; }

        public LayerComposite Layers { get; set; }
    }
}