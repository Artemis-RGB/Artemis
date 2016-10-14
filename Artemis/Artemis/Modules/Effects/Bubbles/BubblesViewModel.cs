using System.Windows.Media;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;

namespace Artemis.Modules.Effects.Bubbles
{
    public sealed class BubblesViewModel : EffectViewModel
    {
        private readonly BubblesModel _model;
        private SolidColorBrush _bubbleColor;

        public BubblesViewModel(MainManager main, BubblesModel model) : base(main, model)
        {
            _model = model;
            DisplayName = "Bubbles";
            BubbleColor = new SolidColorBrush(model.Settings.BubbleColor);
        }

        /// <summary>
        ///     The bubble color wrapped in a brush to allow color selection using ColorBox
        /// </summary>
        public Brush BubbleColor
        {
            get { return _bubbleColor; }
            set
            {
                if (Equals(value, _bubbleColor)) return;
                _bubbleColor = (SolidColorBrush) value;

                _model.Settings.BubbleColor = _bubbleColor.Color;
                NotifyOfPropertyChange(() => BubbleColor);
            }
        }
    }
}