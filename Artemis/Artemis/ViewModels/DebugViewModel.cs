using System.Windows;
using System.Windows.Media;
using Artemis.Events;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class DebugViewModel : Screen, IHandle<RazerColorArrayChanged>
    {
        private readonly IEventAggregator _events;
        private DrawingImage _razerDisplay;

        public DebugViewModel(IEventAggregator events)
        {
            _events = events;
        }

        public DrawingImage RazerDisplay
        {
            get { return _razerDisplay; }
            set
            {
                if (Equals(value, _razerDisplay)) return;
                _razerDisplay = value;
                NotifyOfPropertyChange(() => RazerDisplay);
            }
        }

        public void Handle(RazerColorArrayChanged message)
        {
            var visual = new DrawingVisual();
            using (var dc = visual.RenderOpen())
            {
                dc.PushClip(new RectangleGeometry(new Rect(0, 0, 22, 6)));
                for (var y = 0; y < 6; y++)
                {
                    for (var x = 0; x < 22; x++)
                        dc.DrawRectangle(new SolidColorBrush(message.Colors[y, x]), null, new Rect(x, y, 1, 1));
                }
            }
            var drawnDisplay = new DrawingImage(visual.Drawing);
            drawnDisplay.Freeze();
            RazerDisplay = drawnDisplay;
        }

        protected override void OnActivate()
        {
            _events.Subscribe(this);
            base.OnActivate();
        }

        protected override void OnDeactivate(bool close)
        {
            _events.Unsubscribe(this);
            base.OnDeactivate(close);
        }
    }
}