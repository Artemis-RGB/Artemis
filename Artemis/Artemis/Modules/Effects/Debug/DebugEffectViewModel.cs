using System;
using System.Drawing.Drawing2D;
using System.Windows.Media;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.Debug
{
    internal sealed class DebugEffectViewModel : EffectViewModel, IHandle<ChangeBitmap>, IHandle<ActiveEffectChanged>
    {
        private ImageSource _imageSource;
        private string _selectedRectangleType;

        public DebugEffectViewModel(MainManager main, IEventAggregator events)
            : base(main, new DebugEffectModel(main, new DebugEffectSettings()))
        {
            DisplayName = "Debug Effect";

            events.Subscribe(this);
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }

        public BindableCollection<string> RectangleTypes
            => new BindableCollection<string>(Enum.GetNames(typeof(LinearGradientMode)));

        public string SelectedRectangleType
        {
            get { return _selectedRectangleType; }
            set
            {
                if (value == _selectedRectangleType) return;
                _selectedRectangleType = value;
                NotifyOfPropertyChange(() => SelectedRectangleType);

                ((DebugEffectSettings) EffectSettings).Type =
                    (LinearGradientMode) Enum.Parse(typeof(LinearGradientMode), value);
            }
        }

        public ImageSource ImageSource
        {
            get { return _imageSource; }
            set
            {
                _imageSource = value;
                NotifyOfPropertyChange(() => ImageSource);
            }
        }

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }

        public void Handle(ChangeBitmap message)
        {
            ImageSource = ImageUtilities.BitmapToBitmapImage(message.Bitmap);
        }
    }
}