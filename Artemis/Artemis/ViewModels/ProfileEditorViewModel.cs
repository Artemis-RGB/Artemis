using System;
using System.ComponentModel;
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
using MahApps.Metro;

namespace Artemis.ViewModels
{
    public class ProfileEditorViewModel<T> : Screen, IHandle<ActiveKeyboardChanged>
    {
        private readonly GameModel _gameModel;
        private readonly MainManager _mainManager;
        private DateTime _downTime;
        private LayerModel _draggingLayer;
        private Point? _draggingLayerOffset;
        private LayerEditorViewModel<T> _editorVm;
        private Cursor _keyboardPreviewCursor;
        private BindableCollection<LayerModel> _layers;
        private BindableCollection<ProfileModel> _profiles;
        private bool _resizing;
        private LayerModel _selectedLayer;
        private ProfileModel _selectedProfile;

        public ProfileEditorViewModel(MainManager mainManager, GameModel gameModel)
        {
            _mainManager = mainManager;
            _gameModel = gameModel;

            Profiles = new BindableCollection<ProfileModel>();
            Layers = new BindableCollection<LayerModel>();
            _mainManager.Events.Subscribe(this);

            PropertyChanged += PropertyChangeHandler;
            LoadProfiles();
        }

        public BindableCollection<ProfileModel> Profiles
        {
            get { return _profiles; }
            set
            {
                if (Equals(value, _profiles)) return;
                _profiles = value;
                NotifyOfPropertyChange(() => Profiles);
            }
        }

        public BindableCollection<LayerModel> Layers
        {
            get { return _layers; }
            set
            {
                if (Equals(value, _layers)) return;
                _layers = value;
                NotifyOfPropertyChange(() => Layers);
            }
        }

        public LayerModel SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                if (Equals(value, _selectedLayer)) return;
                _selectedLayer = value;
                NotifyOfPropertyChange(() => SelectedLayer);
            }
        }

        public Cursor KeyboardPreviewCursor
        {
            get { return _keyboardPreviewCursor; }
            set
            {
                if (Equals(value, _keyboardPreviewCursor)) return;
                _keyboardPreviewCursor = value;
                NotifyOfPropertyChange(() => KeyboardPreviewCursor);
            }
        }

        public ProfileModel SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (Equals(value, _selectedProfile)) return;
                _selectedProfile = value;

                Layers.Clear();
                Layers.AddRange(_selectedProfile?.Layers);

                NotifyOfPropertyChange(() => SelectedProfile);
                NotifyOfPropertyChange(() => CanAddLayer);
            }
        }

        public ImageSource KeyboardPreview
        {
            get
            {
                if (_selectedProfile == null || ActiveKeyboard == null)
                    return null;

                var keyboardRect = ActiveKeyboard.KeyboardRectangle(4);
                var visual = new DrawingVisual();
                using (var drawingContext = visual.RenderOpen())
                {
                    // Setup the DrawingVisual's size
                    drawingContext.PushClip(new RectangleGeometry(keyboardRect));
                    drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, keyboardRect);

                    // Draw the layers
                    foreach (var layerModel in _selectedProfile.Layers.OrderByDescending(l => l.Order))
                        layerModel.DrawPreview(drawingContext);

                    // Get the selection color
                    var color = (Color) ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"];
                    var pen = new Pen(new SolidColorBrush(color), 0.4);

                    // Draw the selection outline and resize indicator
                    if (SelectedLayer != null && SelectedLayer.Enabled)
                    {
                        var layerRect = SelectedLayer.UserProps.GetRect();
                        // Deflate the rect so that the border is drawn on the inside
                        layerRect.Inflate(-0.2, -0.2);

                        // Draw an outline around the selected layer
                        drawingContext.DrawRectangle(null, pen, layerRect);
                        // Draw a resize indicator in the bottom-right
                        drawingContext.DrawLine(pen,
                            new Point(layerRect.BottomRight.X - 1, layerRect.BottomRight.Y - 0.5),
                            new Point(layerRect.BottomRight.X - 1.2, layerRect.BottomRight.Y - 0.7));
                        drawingContext.DrawLine(pen,
                            new Point(layerRect.BottomRight.X - 0.5, layerRect.BottomRight.Y - 1),
                            new Point(layerRect.BottomRight.X - 0.7, layerRect.BottomRight.Y - 1.2));
                        drawingContext.DrawLine(pen,
                            new Point(layerRect.BottomRight.X - 0.5, layerRect.BottomRight.Y - 0.5),
                            new Point(layerRect.BottomRight.X - 0.7, layerRect.BottomRight.Y - 0.7));
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
                    if (ActiveKeyboard?.PreviewSettings.Image == null)
                        return null;
                    ActiveKeyboard.PreviewSettings.Image.Save(memory, ImageFormat.Png);
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

        public PreviewSettings? PreviewSettings => ActiveKeyboard?.PreviewSettings;

        public bool CanAddLayer => _selectedProfile != null;

        private KeyboardProvider ActiveKeyboard => _mainManager.KeyboardManager.ActiveKeyboard;

        public void Handle(ActiveKeyboardChanged message)
        {
            NotifyOfPropertyChange(() => KeyboardImage);
            NotifyOfPropertyChange(() => PreviewSettings);
        }

        private void PropertyChangeHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "KeyboardPreview")
                NotifyOfPropertyChange(() => KeyboardPreview);
        }

        private void LoadProfiles()
        {
            Profiles.Clear();
            Profiles.AddRange(ProfileProvider.GetAll(_gameModel));
            SelectedProfile = Profiles.FirstOrDefault();
        }

        public async void AddProfile()
        {
            var name = await _mainManager.DialogService.ShowInputDialog("Add new profile",
                "Please provide a profile name unique to this game and keyboard.");
            if (name.Length < 1)
            {
                _mainManager.DialogService.ShowMessageBox("Invalid profile name", "Please provide a valid profile name");
                return;
            }

            var profile = new ProfileModel
            {
                Name = name,
                KeyboardName = ActiveKeyboard.Name,
                GameName = _gameModel.Name
            };

            if (ProfileProvider.GetAll().Contains(profile))
            {
                var overwrite = await _mainManager.DialogService.ShowQuestionMessageBox("Overwrite existing profile",
                    "A profile with this name already exists for this game. Would you like to overwrite it?");
                if (!overwrite.Value)
                    return;
            }

            ProfileProvider.AddOrUpdate(profile);

            LoadProfiles();
            SelectedProfile = profile;
        }

        public void ToggleEnabled(LayerModel layer)
        {
            NotifyOfPropertyChange(() => KeyboardPreview);
        }

        public void LayerEditor(LayerModel layer)
        {
            IWindowManager manager = new WindowManager();
            _editorVm = new LayerEditorViewModel<T>(ActiveKeyboard, SelectedProfile, layer);
            dynamic settings = new ExpandoObject();

            settings.Title = "Artemis | Edit " + layer.Name;
            manager.ShowDialog(_editorVm, null, settings);
        }

        public void AddLayer()
        {
            if (_selectedProfile == null)
                return;

            var layer = SelectedProfile.AddLayer();
            Layers.Add(layer);

            SelectedLayer = layer;
        }

        public void RemoveLayer()
        {
            if (_selectedProfile == null || _selectedLayer == null)
                return;

            SelectedProfile.Layers.Remove(_selectedLayer);
            Layers.Remove(_selectedLayer);
        }

        public void LayerUp()
        {
            if (SelectedLayer == null)
                return;

            var reorderLayer = SelectedLayer;

            if (SelectedLayer.ParentLayer != null)
                SelectedLayer.ParentLayer.Reorder(SelectedLayer, true);
            else
                SelectedLayer.ParentProfile.Reorder(SelectedLayer, true);

            NotifyOfPropertyChange(() => Layers);
            SelectedLayer = reorderLayer;
        }

        public void LayerDown()
        {
            if (SelectedLayer == null)
                return;

            var reorderLayer = SelectedLayer;

            if (SelectedLayer.ParentLayer != null)
                SelectedLayer.ParentLayer.Reorder(SelectedLayer, false);
            else
                SelectedLayer.ParentProfile.Reorder(SelectedLayer, false);

            NotifyOfPropertyChange(() => Layers);
            SelectedLayer = reorderLayer;
        }

        public void MouseDownKeyboardPreview(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                _downTime = DateTime.Now;
        }

        public void MouseUpKeyboardPreview(MouseButtonEventArgs e)
        {
            var timeSinceDown = DateTime.Now - _downTime;
            if (!(timeSinceDown.TotalMilliseconds < 500))
                return;

            var pos = e.GetPosition((Image) e.OriginalSource);
            var x = pos.X/((double) ActiveKeyboard.PreviewSettings.Width/ActiveKeyboard.Width);
            var y = pos.Y/((double) ActiveKeyboard.PreviewSettings.Height/ActiveKeyboard.Height);

            var hoverLayer = SelectedProfile.Layers.OrderBy(l => l.Order).Where(l => l.Enabled)
                .FirstOrDefault(l => l.UserProps.GetRect(1).Contains(x, y));
            if (hoverLayer != null)
                SelectedLayer = hoverLayer;
        }

        public void MouseMoveKeyboardPreview(MouseEventArgs e)
        {
            var pos = e.GetPosition((Image) e.OriginalSource);
            var x = pos.X/((double) ActiveKeyboard.PreviewSettings.Width/ActiveKeyboard.Width);
            var y = pos.Y/((double) ActiveKeyboard.PreviewSettings.Height/ActiveKeyboard.Height);
            var hoverLayer = SelectedProfile.Layers.OrderBy(l => l.Order).Where(l => l.Enabled)
                .FirstOrDefault(l => l.UserProps.GetRect(1).Contains(x, y));

            HandleDragging(e, x, y, hoverLayer);

            if (hoverLayer == null)
            {
                KeyboardPreviewCursor = Cursors.Arrow;
                return;
            }


            // Turn the mouse pointer into a hand if hovering over an active layer
            if (hoverLayer == SelectedLayer)
            {
                var rect = hoverLayer.UserProps.GetRect(1);
                KeyboardPreviewCursor =
                    Math.Sqrt(Math.Pow(x - rect.BottomRight.X, 2) + Math.Pow(y - rect.BottomRight.Y, 2)) < 0.6
                        ? Cursors.SizeNWSE
                        : Cursors.SizeAll;
            }
            else
                KeyboardPreviewCursor = Cursors.Hand;
        }

        private void HandleDragging(MouseEventArgs e, double x, double y, LayerModel hoverLayer)
        {
            // Reset the dragging state on mouse release
            if (e.LeftButton == MouseButtonState.Released ||
                (_draggingLayer != null && _selectedLayer != _draggingLayer))
            {
                _draggingLayerOffset = null;
                _draggingLayer = null;
                return;
            }

            if (SelectedLayer == null)
                return;

            // Setup the dragging state on mouse press
            if (_draggingLayerOffset == null && hoverLayer != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var layerRect = hoverLayer.UserProps.GetRect(1);
                _draggingLayerOffset = new Point(x - SelectedLayer.UserProps.X, y - SelectedLayer.UserProps.Y);
                _draggingLayer = hoverLayer;
                if (Math.Sqrt(Math.Pow(x - layerRect.BottomRight.X, 2) + Math.Pow(y - layerRect.BottomRight.Y, 2)) < 0.6)
                    _resizing = true;
                else
                    _resizing = false;
            }

            if (_draggingLayerOffset == null || _draggingLayer == null || (_draggingLayer != SelectedLayer))
                return;

            // If no setup or reset was done, handle the actual dragging action
            if (_resizing)
            {
                _draggingLayer.UserProps.Width = (int) Math.Round(x - _draggingLayer.UserProps.X);
                _draggingLayer.UserProps.Height = (int) Math.Round(y - _draggingLayer.UserProps.Y);
                if (_draggingLayer.UserProps.Width < 1)
                    _draggingLayer.UserProps.Width = 1;
                if (_draggingLayer.UserProps.Height < 1)
                    _draggingLayer.UserProps.Height = 1;
            }
            else
            {
                _draggingLayer.UserProps.X = (int) Math.Round(x - _draggingLayerOffset.Value.X);
                _draggingLayer.UserProps.Y = (int) Math.Round(y - _draggingLayerOffset.Value.Y);
            }
            NotifyOfPropertyChange(() => KeyboardPreview);
        }
    }
}