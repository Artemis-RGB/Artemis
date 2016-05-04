using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Utilities;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.Debug
{
    internal class DebugEffectViewModel : EffectViewModel, IHandle<ChangeBitmap>, IHandle<ActiveEffectChanged>
    {
        private ImageSource _imageSource;
        private string _selectedRectangleType;

        public DebugEffectViewModel(MainManager mainManager)
        {
            // Subscribe to main model
            MainManager = mainManager;
            MainManager.Events.Subscribe(this);

            // Settings are loaded from file by class
            EffectSettings = new DebugEffectSettings();

            // Create effect model and add it to MainManager
            EffectModel = new DebugEffectModel(mainManager, (DebugEffectSettings) EffectSettings);
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }


        public static string Name => "Debug Effect";

        public BindableCollection<string> RectangleTypes
            => new BindableCollection<string>(Enum.GetNames(typeof (LinearGradientMode)));

        public string SelectedRectangleType
        {
            get { return _selectedRectangleType; }
            set
            {
                if (value == _selectedRectangleType) return;
                _selectedRectangleType = value;
                NotifyOfPropertyChange(() => SelectedRectangleType);

                ((DebugEffectSettings) EffectSettings).Type =
                    (LinearGradientMode) Enum.Parse(typeof (LinearGradientMode), value);
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