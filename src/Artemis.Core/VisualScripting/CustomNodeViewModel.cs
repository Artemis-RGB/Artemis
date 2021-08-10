using Newtonsoft.Json;

namespace Artemis.Core
{
    public class CustomNodeViewModel : CorePropertyChanged
    {
        [JsonIgnore]
        public INode Node { get; }

        public CustomNodeViewModel(INode node)
        {
            Node = node;
        }
    }
}