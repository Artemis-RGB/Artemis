using Stylet;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class StaticStringValueNodeCustomViewModel : PropertyChangedBase
    {
        private string _input;

        public string Input
        {
            get => _input;
            set => SetAndNotify(ref _input, value);
        }
    }
}