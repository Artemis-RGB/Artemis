using System.Windows;
using System.Windows.Media;
using Artemis.Core.Utilities;
using Humanizer;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineEasingViewModel
    {
        private readonly TimelineKeyframeViewModel _keyframeViewModel;
        private bool _isEasingModeSelected;

        public TimelineEasingViewModel(TimelineKeyframeViewModel keyframeViewModel, Easings.Functions easingFunction)
        {
            _keyframeViewModel = keyframeViewModel;
            _isEasingModeSelected = keyframeViewModel.BaseLayerPropertyKeyframe.EasingFunction == easingFunction;

            EasingFunction = easingFunction;
            Description = easingFunction.Humanize();

            EasingPoints = new PointCollection();
            for (var i = 1; i <= 10; i++)
            {
                var x = i;
                var y = Easings.Interpolate(i / 10.0, EasingFunction) * 10;
                EasingPoints.Add(new Point(x, y));
            }
        }

        public Easings.Functions EasingFunction { get; }
        public PointCollection EasingPoints { get; }
        public string Description { get; }

        public bool IsEasingModeSelected
        {
            get => _isEasingModeSelected;
            set
            {
                _isEasingModeSelected = value;
                if (_isEasingModeSelected)
                    _keyframeViewModel.SelectEasingMode(this);
            }
        }
    }
}