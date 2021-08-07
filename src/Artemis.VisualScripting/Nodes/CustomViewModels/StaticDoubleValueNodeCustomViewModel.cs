using Stylet;

namespace Artemis.VisualScripting.Nodes.CustomViewModels
{
    public class StaticDoubleValueNodeCustomViewModel : PropertyChangedBase
    {
        private double _input;

        public double Input
        {
            get => _input;
            set => SetAndNotify(ref _input, value);
        }
    }
}