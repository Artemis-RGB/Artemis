using System.Windows;
using System.Windows.Media;
using Artemis.Core.Utilities;
using Humanizer;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Timeline
{
    public class PropertyTrackEasingViewModel : PropertyChangedBase
    {
        private readonly PropertyTrackKeyframeViewModel _keyframeViewModel;
        private bool _isEasingModeSelected;

        public PropertyTrackEasingViewModel(PropertyTrackKeyframeViewModel keyframeViewModel, Easings.Functions easingFunction)
        {
            _keyframeViewModel = keyframeViewModel;
            _isEasingModeSelected = keyframeViewModel.Keyframe.EasingFunction == easingFunction;

            EasingFunction = easingFunction;
            Description = easingFunction.Humanize();

            CreateGeometry();
        }

        public Easings.Functions EasingFunction { get; }
        public PointCollection EasingPoints { get; set; }
        public string Description { get; set; }

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

        private void CreateGeometry()
        {
            EasingPoints = new PointCollection();
            for (var i = 1; i <= 10; i++)
            {
                var x = i;
                var y = Easings.Interpolate(i / 10.0, EasingFunction) * 10;
                EasingPoints.Add(new Point(x, y));
            }
        }
    }
}