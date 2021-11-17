using System.Windows;
using System.Windows.Media;
using Artemis.Core;
using Humanizer;
using Stylet;

namespace Artemis.VisualScripting.Nodes.Easing.CustomViewModels
{
    public class NodeEasingViewModel : PropertyChangedBase
    {
        public NodeEasingViewModel(Easings.Functions easingFunction)
        {
            EasingFunction = easingFunction;
            Description = easingFunction.Humanize();

            EasingPoints = new PointCollection();
            for (int i = 1; i <= 10; i++)
            {
                int x = i;
                double y = Easings.Interpolate(i / 10.0, EasingFunction) * 10;
                EasingPoints.Add(new Point(x, y));
            }
        }

        public Easings.Functions EasingFunction { get; }
        public PointCollection EasingPoints { get; }
        public string Description { get; }
    }
}