using System.Windows.Media;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Ninject;

namespace Artemis.Modules.General.Bubbles
{
    public sealed class BubblesViewModel : ModuleViewModel
    {
        private SolidColorBrush _bubbleColor;

        public BubblesViewModel(MainManager mainManager, [Named(nameof(BubblesModel))] ModuleModel model, IKernel kernel)
            : base(mainManager, model, kernel)
        {
            DisplayName = "Bubbles";
            BubbleColor = new SolidColorBrush(((BubblesSettings) ModuleModel.Settings).BubbleColor);
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

                ((BubblesSettings) ModuleModel.Settings).BubbleColor = _bubbleColor.Color;
                NotifyOfPropertyChange(() => BubbleColor);
            }
        }

        public override bool UsesProfileEditor => false;
    }
}