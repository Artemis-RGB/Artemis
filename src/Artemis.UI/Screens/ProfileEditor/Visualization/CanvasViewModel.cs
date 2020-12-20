using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.Visualization
{
    public abstract class CanvasViewModel : Screen
    {
        private double _x;
        private double _y;

        public double X
        {
            get => _x;
            set => SetAndNotify(ref _x, value);
        }

        public double Y
        {
            get => _y;
            set => SetAndNotify(ref _y, value);
        }
    }
}