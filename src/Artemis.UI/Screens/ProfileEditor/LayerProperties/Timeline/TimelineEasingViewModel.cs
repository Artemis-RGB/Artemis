using System;
using System.Windows;
using System.Windows.Media;
using Artemis.Core;
using Humanizer;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline
{
    public class TimelineEasingViewModel : PropertyChangedBase
    {
        private bool _isEasingModeSelected;

        public TimelineEasingViewModel(Easings.Functions easingFunction, bool isSelected)
        {
            _isEasingModeSelected = isSelected;

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

        public bool IsEasingModeSelected
        {
            get => _isEasingModeSelected;
            set
            {
                _isEasingModeSelected = value;
                if (value) OnEasingModeSelected();
            }
        }

        public event EventHandler EasingModeSelected;

        protected virtual void OnEasingModeSelected()
        {
            EasingModeSelected?.Invoke(this, EventArgs.Empty);
        }
    }
}