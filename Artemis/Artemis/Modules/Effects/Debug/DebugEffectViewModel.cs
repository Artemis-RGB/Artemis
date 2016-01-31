using System;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.Events;
using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.Debug
{
    internal class DebugEffectViewModel : Screen, IHandle<ChangeBitmap>, IHandle<ChangeActiveEffect>
    {
        private DebugEffectSettings _debugEffectSettings;
        private ImageSource _imageSource;
        private string _selectedRectangleType;

        public DebugEffectViewModel(MainModel mainModel)
        {
            // Subscribe to main model
            MainModel = mainModel;
            MainModel.Events.Subscribe(this);

            // Settings are loaded from file by class
            DebugEffectSettings = new DebugEffectSettings();

            // Create effect model and add it to MainModel
            DebugEffectModel = new DebugEffectModel(mainModel, DebugEffectSettings);
            MainModel.EffectModels.Add(DebugEffectModel);
        }

        public MainModel MainModel { get; set; }
        public DebugEffectModel DebugEffectModel { get; set; }

        public static string Name => "Type Waves";
        public bool EffectEnabled => MainModel.IsEnabled(DebugEffectModel);

        public DebugEffectSettings DebugEffectSettings
        {
            get { return _debugEffectSettings; }
            set
            {
                if (Equals(value, _debugEffectSettings)) return;
                _debugEffectSettings = value;
                NotifyOfPropertyChange(() => DebugEffectSettings);

                SelectedRectangleType = value.Type.ToString();
            }
        }

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

                DebugEffectSettings.Type = (LinearGradientMode) Enum.Parse(typeof (LinearGradientMode), value);
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

        public void Handle(ChangeActiveEffect message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }

        public void Handle(ChangeBitmap message)
        {
            // Doesn't show transparancy
            using (var memory = new MemoryStream())
            {
                message.Bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                ImageSource = bitmapImage;
            }
        }

        public void ToggleEffect()
        {
            MainModel.EnableEffect(DebugEffectModel);
        }

        public void ResetSettings()
        {
            // TODO: Confirmation dialog (Generic MVVM approach)
            DebugEffectSettings.ToDefault();
            NotifyOfPropertyChange(() => DebugEffectSettings);
        }
    }
}