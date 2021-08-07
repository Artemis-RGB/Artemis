using Stylet;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class StaticIntegerValueNodeCustomViewModel : PropertyChangedBase
    {
        private int _input;

        public int Input
        {
            get => _input;
            set => SetAndNotify(ref _input, value);
        }
    }
}