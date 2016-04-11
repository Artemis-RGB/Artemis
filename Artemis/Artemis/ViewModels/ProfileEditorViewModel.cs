using System;
using System.Drawing.Imaging;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.DAL;
using Artemis.Events;
using Artemis.KeyboardProviders;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class ProfileEditorViewModel<T> : Screen, IHandle<ActiveKeyboardChanged>
    {
        private readonly GameModel _gameModel;
        private readonly MainManager _mainManager;
        private LayerEditorViewModel<T> _editorVm;
        private BindableCollection<ProfileModel> _profileModels;
        private ProfileModel _selectedProfileModel;

        public ProfileEditorViewModel(MainManager mainManager, GameModel gameModel)
        {
            _mainManager = mainManager;
            _gameModel = gameModel;

            ProfileModels = new BindableCollection<ProfileModel>();
            _mainManager.Events.Subscribe(this);
            LoadProfiles();
        }

        public BindableCollection<ProfileModel> ProfileModels
        {
            get { return _profileModels; }
            set
            {
                if (Equals(value, _profileModels)) return;
                _profileModels = value;
                NotifyOfPropertyChange(() => ProfileModels);
            }
        }

        public ProfileModel SelectedProfileModel
        {
            get { return _selectedProfileModel; }
            set
            {
                if (Equals(value, _selectedProfileModel)) return;
                _selectedProfileModel = value;
                NotifyOfPropertyChange(() => SelectedProfileModel);
            }
        }

        public LayerModel SelectedLayer { get; set; }

        public ImageSource KeyboardPreview
        {
            get
            {
                if (_selectedProfileModel == null)
                    return null;

                var keyboardRect = _mainManager.KeyboardManager.ActiveKeyboard.KeyboardRectangle(4);
                var visual = new DrawingVisual();
                using (var drawingContext = visual.RenderOpen())
                {
                    // Setup the DrawingVisual's size
                    drawingContext.PushClip(new RectangleGeometry(keyboardRect));
                    drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, keyboardRect);

                    // Draw the layer
                    foreach (var layerModel in _selectedProfileModel.Layers)
                    {
                        // if (layerModel.Selected)
                            drawingContext.DrawRectangle(null, new Pen(new SolidColorBrush(Colors.White), 0.5),new Rect(layerModel.LayerUserProperties.X*4,layerModel.LayerUserProperties.Y*4, layerModel.LayerUserProperties.Width*4,layerModel.LayerUserProperties.Height*4));
                        layerModel.DrawPreview(drawingContext);
                    }

                    // Remove the clip
                    drawingContext.Pop();
                }
                var image = new DrawingImage(visual.Drawing);

                return image;
            }
        }

        public ImageSource KeyboardImage
        {
            get
            {
                using (var memory = new MemoryStream())
                {
                    if (_mainManager.KeyboardManager.ActiveKeyboard?.PreviewSettings == null)
                        return null;

                    _mainManager.KeyboardManager.ActiveKeyboard.PreviewSettings.Image.Save(memory, ImageFormat.Png);
                    memory.Position = 0;

                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();

                    return bitmapImage;
                }
            }
        }

        public PreviewSettings? PreviewSettings
        {
            get { return _mainManager.KeyboardManager.ActiveKeyboard?.PreviewSettings; }
        }

        public void Handle(ActiveKeyboardChanged message)
        {
            NotifyOfPropertyChange(() => KeyboardImage);
            NotifyOfPropertyChange(() => PreviewSettings);
        }

        private void LoadProfiles()
        {
            ProfileModels.Clear();
            ProfileModels.AddRange(ProfileProvider.GetAll(_gameModel));
            SelectedProfileModel = ProfileModels.FirstOrDefault();
        }

        public async void AddProfile()
        {
            var name =
                await
                    _mainManager.DialogService.ShowInputDialog("Add new profile",
                        "Please provide a profile name unique to this game and keyboard.");
            if (name.Length < 1)
            {
                _mainManager.DialogService.ShowMessageBox("Invalid profile name", "Please provide a valid profile name");
                return;
            }

            var profile = new ProfileModel
            {
                Name = name,
                KeyboardName = _mainManager.KeyboardManager.ActiveKeyboard.Name,
                GameName = _gameModel.Name
            };
            if (ProfileProvider.GetAll().Contains(profile))
            {
                var overwrite =
                    await
                        _mainManager.DialogService.ShowQuestionMessageBox("Overwrite existing profile",
                            "A profile with this name already exists for this game. Would you like to overwrite it?");
                if (!overwrite.Value)
                    return;
            }

            ProfileProvider.AddOrUpdate(profile);

            LoadProfiles();
            SelectedProfileModel = profile;
        }

        public void LayerEditor(LayerModel layer)
        {
            IWindowManager manager = new WindowManager();
            _editorVm = new LayerEditorViewModel<T>(_mainManager.KeyboardManager.ActiveKeyboard, SelectedProfileModel,
                layer);
            dynamic settings = new ExpandoObject();

            settings.Title = "Artemis | Edit " + layer.Name;
            manager.ShowDialog(_editorVm, null, settings);
        }

        public void SetSelectedLayer(LayerModel layer)
        {
            SelectedLayer = layer;
            NotifyOfPropertyChange(() => KeyboardPreview);
        }

        public void AddLayer()
        {
            _selectedProfileModel.Layers.Add(new LayerModel
            {
                Name = "Layer " + (_selectedProfileModel.Layers.Count + 1),
                LayerType = LayerType.KeyboardRectangle
            });
            NotifyOfPropertyChange(() => SelectedProfileModel);
        }

        public void MouseMoveKeyboardPreview(MouseEventArgs e)
        {
            var pos = e.GetPosition((Image) e.OriginalSource);
            var realX =
                (int)
                    Math.Round(pos.X/
                               (_mainManager.KeyboardManager.ActiveKeyboard.PreviewSettings.Width/
                                _mainManager.KeyboardManager.ActiveKeyboard.Width));
            var realY =
                (int)
                    Math.Round(pos.Y/
                               (_mainManager.KeyboardManager.ActiveKeyboard.PreviewSettings.Height/
                                _mainManager.KeyboardManager.ActiveKeyboard.Height));
        }
    }
}