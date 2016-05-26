using System;
using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Artemis.DAL;
using Artemis.DeviceProviders;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Models.Profiles.Properties;
using Artemis.Services;
using Artemis.Styles.DropTargetAdorners;
using Artemis.Utilities;
using Artemis.ViewModels.LayerEditor;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro;
using Ninject;
using Application = System.Windows.Application;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using DragDropEffects = System.Windows.DragDropEffects;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Screen = Caliburn.Micro.Screen;
using Timer = System.Timers.Timer;

namespace Artemis.ViewModels
{
    public sealed class ProfileEditorViewModel : Screen, IHandle<ActiveKeyboardChanged>, IDropTarget
    {
        private readonly GameModel _gameModel;
        private readonly MainManager _mainManager;
        private DateTime _downTime;
        private LayerModel _draggingLayer;
        private Point? _draggingLayerOffset;
        private LayerEditorViewModel _editorVm;
        private ImageSource _keyboardPreview;
        private Cursor _keyboardPreviewCursor;
        private BindableCollection<LayerModel> _layers;
        private BindableCollection<ProfileModel> _profiles;
        private bool _resizing;
        private LayerModel _selectedLayer;
        private ProfileModel _selectedProfile;

        public ProfileEditorViewModel(IEventAggregator events, MainManager mainManager, GameModel gameModel,
            MetroDialogService dialogService)
        {
            _mainManager = mainManager;
            _gameModel = gameModel;

            Profiles = new BindableCollection<ProfileModel>();
            Layers = new BindableCollection<LayerModel>();
            ActiveKeyboard = _mainManager.DeviceManager.ActiveKeyboard;
            DialogService = dialogService;

            events.Subscribe(this);

            PreviewTimer = new Timer(40);
            PreviewTimer.Elapsed += InvokeUpdateKeyboardPreview;

            PropertyChanged += PropertyChangeHandler;
            LoadProfiles();
        }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public Timer PreviewTimer { get; set; }

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
                NotifyOfPropertyChange(() => LayerSelected);
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
                if (_selectedProfile != null)
                    Layers.AddRange(SelectedProfile.Layers);

                NotifyOfPropertyChange(() => SelectedProfile);
                NotifyOfPropertyChange(() => ProfileSelected);
                NotifyOfPropertyChange(() => LayerSelected);
            }
        }

        public ImageSource KeyboardPreview
        {
            get { return _keyboardPreview; }
            set
            {
                if (Equals(value, _keyboardPreview)) return;
                _keyboardPreview = value;
                NotifyOfPropertyChange(() => KeyboardPreview);
            }
        }

        public ImageSource KeyboardImage => ImageUtilities.BitmapToBitmapImage(ActiveKeyboard?.PreviewSettings.Image);

        public PreviewSettings? PreviewSettings => ActiveKeyboard?.PreviewSettings;

        public bool ProfileSelected => SelectedProfile != null;
        public bool LayerSelected => SelectedProfile != null && _selectedLayer != null;

        private KeyboardProvider ActiveKeyboard { get; set; }

        public void DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.Data as LayerModel;
            var target = dropInfo.TargetItem as LayerModel;
            if (source == null || target == null || source == target)
                return;

            if (dropInfo.InsertPosition == RelativeInsertPosition.TargetItemCenter &&
                target.LayerType == LayerType.Folder)
            {
                dropInfo.DropTargetAdorner = typeof(DropTargetMetroHighlightAdorner);
                dropInfo.Effects = DragDropEffects.Copy;
            }
            else
            {
                dropInfo.DropTargetAdorner = typeof(DropTargetMetroInsertionAdorner);
                dropInfo.Effects = DragDropEffects.Move;
            }
        }

        public void Drop(IDropInfo dropInfo)
        {
            var source = dropInfo.Data as LayerModel;
            var target = dropInfo.TargetItem as LayerModel;
            if (source == null || target == null || source == target)
                return;

            // Don't allow a folder to become it's own child, that's just weird
            if (target.Parent == source)
                return;

            // Remove the source from it's old profile/parent
            if (source.Parent == null)
                source.Profile.Layers.Remove(source);
            else
                source.Parent.Children.Remove(source);

            if (dropInfo.InsertPosition == RelativeInsertPosition.TargetItemCenter &&
                target.LayerType == LayerType.Folder)
            {
                // Insert into folder
                source.Order = -1;
                target.Children.Add(source);
                target.FixOrder();
            }
            else
            {
                // Insert the source into it's new profile/parent and update the order
                if (dropInfo.InsertPosition == RelativeInsertPosition.AfterTargetItem)
                    source.Order = target.Order + 1;
                else
                    source.Order = target.Order - 1;
                if (target.Parent == null)
                    target.Profile.Layers.Add(source);
                else
                    target.Parent.Children.Add(source);
            }

            target.Profile?.FixOrder();
            target.Parent?.FixOrder();
            UpdateLayerList(source);
        }

        /// <summary>
        ///     Handles chaning the active keyboard, updating the preview image and profiles collection
        /// </summary>
        /// <param name="message"></param>
        public void Handle(ActiveKeyboardChanged message)
        {
            ActiveKeyboard = _mainManager.DeviceManager.ActiveKeyboard;
            NotifyOfPropertyChange(() => KeyboardImage);
            NotifyOfPropertyChange(() => PreviewSettings);
            LoadProfiles();
        }

        /// <summary>
        ///     Handles refreshing the layer preview
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PropertyChangeHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "KeyboardPreview")
                return;

            if (SelectedProfile != null)
                ProfileProvider.AddOrUpdate(SelectedProfile);
        }

        /// <summary>
        ///     Loads all profiles for the current game and keyboard
        /// </summary>
        private void LoadProfiles()
        {
            Profiles.Clear();
            if (_gameModel == null || ActiveKeyboard == null)
                return;

            Profiles.AddRange(ProfileProvider.GetAll(_gameModel, ActiveKeyboard));
            SelectedProfile = Profiles.FirstOrDefault();
        }

        public void EditLayer()
        {
            if (SelectedLayer == null)
                return;

            LayerEditor(SelectedLayer);
        }

        /// <summary>
        ///     Opens a new LayerEditorView for the given layer
        /// </summary>
        /// <param name="layer">The layer to open the view for</param>
        public void LayerEditor(LayerModel layer)
        {
            IWindowManager manager = new WindowManager();
            _editorVm = new LayerEditorViewModel(_gameModel.GameDataModel, layer);
            dynamic settings = new ExpandoObject();

            settings.Title = "Artemis | Edit " + layer.Name;
            manager.ShowDialog(_editorVm, null, settings);

            // If the layer was a folder, but isn't anymore, assign it's children to it's parent.
            if (layer.LayerType != LayerType.Folder && layer.Children.Any())
            {
                while (layer.Children.Any())
                {
                    var child = layer.Children[0];
                    layer.Children.Remove(child);
                    if (layer.Parent != null)
                    {
                        layer.Parent.Children.Add(child);
                        layer.Parent.FixOrder();
                    }
                    else
                    {
                        layer.Profile.Layers.Add(child);
                        layer.Profile.FixOrder();
                    }
                }
            }

            UpdateLayerList(layer);
        }

        /// <summary>
        ///     Adds a new layer to the profile and selects it
        /// </summary>
        public void AddLayer()
        {
            if (SelectedProfile == null)
                return;

            var layer = SelectedProfile.AddLayer();
            Layers.Add(layer);

            SelectedLayer = layer;
        }

        /// <summary>
        ///     Removes the currently selected layer from the profile
        /// </summary>
        public void RemoveLayer()
        {
            if (SelectedProfile == null || _selectedLayer == null)
                return;

            SelectedProfile.Layers.Remove(_selectedLayer);
            Layers.Remove(_selectedLayer);

            SelectedProfile.FixOrder();
        }

        /// <summary>
        ///     Removes the given layer from the profile
        /// </summary>
        /// <param name="layer"></param>
        public void RemoveLayerFromMenu(LayerModel layer)
        {
            SelectedProfile.Layers.Remove(layer);
            Layers.Remove(layer);

            SelectedProfile.FixOrder();
        }

        public void CloneLayer()
        {
            if (SelectedLayer == null)
                return;

            CloneLayer(SelectedLayer);
        }

        /// <summary>
        ///     Clones the given layer and adds it to the profile, on top of the original
        /// </summary>
        /// <param name="layer"></param>
        public void CloneLayer(LayerModel layer)
        {
            var clone = GeneralHelpers.Clone(layer);
            clone.Order = layer.Order - 1;
            SelectedProfile.Layers.Add(clone);
            Layers.Add(clone);

            SelectedProfile.FixOrder();
        }

        private void UpdateLayerList(LayerModel selectModel)
        {
            // Update the UI
            Layers.Clear();
            if (SelectedProfile != null)
                Layers.AddRange(SelectedProfile.Layers);

            // A small delay to allow the profile list to rebuild
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(20);
                SelectedLayer = selectModel;
            });
        }

        /// <summary>
        ///     Handler for clicking
        /// </summary>
        /// <param name="e"></param>
        public void MouseDownKeyboardPreview(MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                _downTime = DateTime.Now;
        }

        /// <summary>
        ///     Second handler for clicking, selects a the layer the user clicked on
        ///     if the used clicked on an empty spot, deselects the current layer
        /// </summary>
        /// <param name="e"></param>
        public void MouseUpKeyboardPreview(MouseButtonEventArgs e)
        {
            if (SelectedProfile == null)
                return;

            var timeSinceDown = DateTime.Now - _downTime;
            if (!(timeSinceDown.TotalMilliseconds < 500))
                return;

            var pos = e.GetPosition((Image) e.OriginalSource);
            var x = pos.X/((double) ActiveKeyboard.PreviewSettings.Width/ActiveKeyboard.Width);
            var y = pos.Y/((double) ActiveKeyboard.PreviewSettings.Height/ActiveKeyboard.Height);

            var hoverLayer = SelectedProfile.GetLayers()
                .Where(l => l.MustDraw())
                .FirstOrDefault(l => ((KeyboardPropertiesModel) l.Properties)
                    .GetRect(1)
                    .Contains(x, y));

            SelectedLayer = hoverLayer;
        }

        /// <summary>
        ///     Handler for resizing and moving the currently selected layer
        /// </summary>
        /// <param name="e"></param>
        public void MouseMoveKeyboardPreview(MouseEventArgs e)
        {
            if (SelectedProfile == null)
                return;

            var pos = e.GetPosition((Image) e.OriginalSource);
            var x = pos.X/((double) ActiveKeyboard.PreviewSettings.Width/ActiveKeyboard.Width);
            var y = pos.Y/((double) ActiveKeyboard.PreviewSettings.Height/ActiveKeyboard.Height);
            var hoverLayer = SelectedProfile.GetLayers()
                .Where(l => l.MustDraw())
                .FirstOrDefault(l => ((KeyboardPropertiesModel) l.Properties)
                    .GetRect(1).Contains(x, y));

            HandleDragging(e, x, y, hoverLayer);

            if (hoverLayer == null)
            {
                KeyboardPreviewCursor = Cursors.Arrow;
                return;
            }


            // Turn the mouse pointer into a hand if hovering over an active layer
            if (hoverLayer == SelectedLayer)
            {
                var rect = ((KeyboardPropertiesModel) hoverLayer.Properties).GetRect(1);
                KeyboardPreviewCursor =
                    Math.Sqrt(Math.Pow(x - rect.BottomRight.X, 2) + Math.Pow(y - rect.BottomRight.Y, 2)) < 0.6
                        ? Cursors.SizeNWSE
                        : Cursors.SizeAll;
            }
            else
                KeyboardPreviewCursor = Cursors.Hand;
        }

        private void InvokeUpdateKeyboardPreview(object sender, ElapsedEventArgs e)
        {
            Application.Current.Dispatcher.InvokeAsync(UpdateKeyboardPreview, DispatcherPriority.ContextIdle);
        }

        /// <summary>
        ///     Generates a new image for the keyboard preview
        /// </summary>
        public void UpdateKeyboardPreview()
        {
            if (SelectedProfile == null || ActiveKeyboard == null)
            {
                KeyboardPreview = new DrawingImage();
                return;
            }

            var keyboardRect = ActiveKeyboard.KeyboardRectangle(4);
            var visual = new DrawingVisual();
            using (var drawingContext = visual.RenderOpen())
            {
                // Setup the DrawingVisual's size
                drawingContext.PushClip(new RectangleGeometry(keyboardRect));
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, keyboardRect);

                // Draw the layers
                var drawLayers = SelectedProfile.Layers
                    .OrderByDescending(l => l.Order)
                    .Where(l => l.Enabled &&
                                (l.LayerType == LayerType.Keyboard ||
                                 l.LayerType == LayerType.KeyboardGif ||
                                 l.LayerType == LayerType.Folder));
                foreach (var layer in drawLayers)
                    layer.Draw<object>(null, drawingContext, true, false);

                // Get the selection color
                var color = (Color) ThemeManager.DetectAppStyle(Application.Current).Item2.Resources["AccentColor"];
                var pen = new Pen(new SolidColorBrush(color), 0.4);

                // Draw the selection outline and resize indicator
                if (SelectedLayer != null && SelectedLayer.MustDraw())
                {
                    var layerRect = ((KeyboardPropertiesModel) SelectedLayer.Properties).GetRect();
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
            KeyboardPreview = new DrawingImage(visual.Drawing);
        }

        /// <summary>
        ///     Handles dragging the given layer
        /// </summary>
        /// <param name="e"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="hoverLayer"></param>
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
                var layerRect = ((KeyboardPropertiesModel) hoverLayer.Properties).GetRect(1);
                var selectedProps = (KeyboardPropertiesModel) SelectedLayer.Properties;

                _draggingLayerOffset = new Point(x - selectedProps.X, y - selectedProps.Y);
                _draggingLayer = hoverLayer;
                // Detect dragging if cursor is in the bottom right
                _resizing = Math.Sqrt(Math.Pow(x - layerRect.BottomRight.X, 2) +
                                      Math.Pow(y - layerRect.BottomRight.Y, 2)) < 0.6;
            }

            if (_draggingLayerOffset == null || _draggingLayer == null || (_draggingLayer != SelectedLayer))
                return;

            var draggingProps = (KeyboardPropertiesModel) _draggingLayer?.Properties;

            // If no setup or reset was done, handle the actual dragging action
            if (_resizing)
            {
                draggingProps.Width = (int) Math.Round(x - draggingProps.X);
                draggingProps.Height = (int) Math.Round(y - draggingProps.Y);
                if (draggingProps.Width < 1)
                    draggingProps.Width = 1;
                if (draggingProps.Height < 1)
                    draggingProps.Height = 1;
            }
            else
            {
                draggingProps.X = (int) Math.Round(x - _draggingLayerOffset.Value.X);
                draggingProps.Y = (int) Math.Round(y - _draggingLayerOffset.Value.Y);
            }
        }

        /// <summary>
        ///     Adds a new profile to the current game and keyboard
        /// </summary>
        public async void AddProfile()
        {
            var name = await DialogService.ShowInputDialog("Add new profile",
                "Please provide a profile name unique to this game and keyboard.");

            // Null when the user cancelled
            if (name == null)
                return;

            if (name.Length < 1)
            {
                DialogService.ShowMessageBox("Invalid profile name", "Please provide a valid profile name");
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
                var overwrite = await DialogService.ShowQuestionMessageBox("Overwrite existing profile",
                    "A profile with this name already exists for this game. Would you like to overwrite it?");
                if (!overwrite.Value)
                    return;
            }

            ProfileProvider.AddOrUpdate(profile);

            LoadProfiles();
            SelectedProfile = profile;
        }

        public async void RenameProfile()
        {
            if (SelectedProfile == null)
                return;

            var oldName = SelectedProfile.Name;
            SelectedProfile.Name =
                await DialogService.ShowInputDialog("Rename profile", "Please enter a unique new profile name");
            // Verify the name
            while (ProfileProvider.GetAll().Contains(SelectedProfile))
            {
                SelectedProfile.Name =
                    await DialogService.ShowInputDialog("Name already in use", "Please enter a unique new profile name");

                // Null when the user cancelled
                if (string.IsNullOrEmpty(SelectedProfile.Name))
                {
                    SelectedProfile.Name = oldName;
                    return;
                }
            }

            var newName = SelectedProfile.Name;
            SelectedProfile.Name = oldName;
            ProfileProvider.RenameProfile(SelectedProfile, newName);

            LoadProfiles();
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == newName);
        }

        public async void DuplicateProfile()
        {
            if (SelectedProfile == null)
                return;

            var newProfile = GeneralHelpers.Clone(SelectedProfile);
            newProfile.Name =
                await DialogService.ShowInputDialog("Duplicate profile", "Please enter a unique profile name");
            // Verify the name
            while (ProfileProvider.GetAll().Contains(newProfile))
            {
                newProfile.Name =
                    await DialogService.ShowInputDialog("Name already in use", "Please enter a unique profile name");

                // Null when the user cancelled
                if (string.IsNullOrEmpty(SelectedProfile.Name))
                    return;
            }

            ProfileProvider.AddOrUpdate(newProfile);
            LoadProfiles();
            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == newProfile.Name);
        }

        public async void DeleteProfile()
        {
            if (SelectedProfile == null)
                return;

            var confirm = await
                DialogService.ShowQuestionMessageBox("Delete profile",
                    $"Are you sure you want to delete the profile named: {SelectedProfile.Name}?\n\n" +
                    "This cannot be undone.");
            if (!confirm.Value)
                return;

            ProfileProvider.DeleteProfile(SelectedProfile);
            LoadProfiles();
        }

        public async void ImportProfile()
        {
            var dialog = new OpenFileDialog {Filter = "Artemis profile (*.xml)|*.xml"};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            var profile = ProfileProvider.LoadProfileIfValid(dialog.FileName);
            if (profile == null)
            {
                DialogService.ShowErrorMessageBox("Oh noes, the profile you provided is invalid. " +
                                                  "If this keeps happening, please make an issue on GitHub and provide the profile.");
                return;
            }

            // Verify the game
            if (profile.GameName != _gameModel.Name)
            {
                DialogService.ShowErrorMessageBox(
                    $"Oh oops! This profile is ment for {profile.GameName}, not {_gameModel.Name} :c");
                return;
            }

            // Verify the keyboard
            if (profile.KeyboardName != _mainManager.DeviceManager.ActiveKeyboard.Name)
            {
                var adjustKeyboard = await DialogService.ShowQuestionMessageBox("Profile not inteded for this keyboard",
                    $"Watch out, this profile wasn't ment for this keyboard, but for the {profile.KeyboardName}. " +
                    "You can still import it but you'll probably have to do some adjusting\n\n" +
                    "Continue?");
                if (!adjustKeyboard.Value)
                    return;

                profile.KeyboardName = _mainManager.DeviceManager.ActiveKeyboard.Name;
                profile.FixBoundaries(_mainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(1));
            }

            // Verify the name
            while (ProfileProvider.GetAll().Contains(profile))
            {
                profile.Name = await DialogService.ShowInputDialog("Rename imported profile",
                    "A profile with this name already exists for this game. Please enter a new name");

                // Null when the user cancelled
                if (string.IsNullOrEmpty(profile.Name))
                    return;
            }

            ProfileProvider.AddOrUpdate(profile);
            LoadProfiles();

            SelectedProfile = Profiles.FirstOrDefault(p => p.Name == profile.Name);
        }

        public void ExportProfile()
        {
            if (SelectedProfile == null)
                return;

            var dialog = new SaveFileDialog {Filter = "Artemis profile (*.xml)|*.xml"};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ProfileProvider.ExportProfile(SelectedProfile, dialog.FileName);
        }
    }
}