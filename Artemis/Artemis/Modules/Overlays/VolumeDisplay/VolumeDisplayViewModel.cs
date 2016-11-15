using System.Windows.Media;
using Artemis.Managers;
using Artemis.Models;
using Artemis.ViewModels.Abstract;
using Ninject;

namespace Artemis.Modules.Overlays.VolumeDisplay
{
    public sealed class VolumeDisplayViewModel : OverlayViewModel
    {
        private readonly VolumeDisplayModel _model;
        private SolidColorBrush _mainColor;
        private SolidColorBrush _secondaryColor;

        public VolumeDisplayViewModel(MainManager mainManager, [Named("VolumeDisplayModel")] OverlayModel model)
            : base(mainManager, model)
        {
            _model = (VolumeDisplayModel) model;
            DisplayName = "Volume Display";

            MainColor = new SolidColorBrush(_model.Settings.MainColor);
            SecondaryColor = new SolidColorBrush(_model.Settings.SecondaryColor);
        }

        public Brush MainColor
        {
            get { return _mainColor; }
            set
            {
                if (Equals(value, _mainColor)) return;
                _mainColor = (SolidColorBrush) value;

                _model.Settings.MainColor = _mainColor.Color;
                NotifyOfPropertyChange(() => MainColor);
            }
        }

        public Brush SecondaryColor
        {
            get { return _secondaryColor; }
            set
            {
                if (Equals(value, _secondaryColor)) return;
                _secondaryColor = (SolidColorBrush) value;

                _model.Settings.SecondaryColor = _secondaryColor.Color;
                NotifyOfPropertyChange(() => SecondaryColor);
            }
        }
    }
}