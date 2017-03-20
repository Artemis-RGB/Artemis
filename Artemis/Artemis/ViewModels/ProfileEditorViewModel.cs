using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Abstract;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Folder;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Styles.DropTargetAdorners;
using Artemis.Utilities;
using Caliburn.Micro;
using Castle.Components.DictionaryAdapter;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro;
using MahApps.Metro.Controls;
using MahApps.Metro.Controls.Dialogs;
using Newtonsoft.Json;
using NuGet;
using Application = System.Windows.Application;
using Clipboard = System.Windows.Clipboard;
using Cursor = System.Windows.Input.Cursor;
using Cursors = System.Windows.Input.Cursors;
using DragDropEffects = System.Windows.DragDropEffects;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Screen = Caliburn.Micro.Screen;

namespace Artemis.ViewModels
{
    public sealed class ProfileEditorViewModel : Screen, IDropTarget
    {
        private readonly KeybindModel _copyKeybind;
        private readonly DeviceManager _deviceManager;
        private readonly MetroDialogService _dialogService;
        private readonly LoopManager _loopManager;
        private readonly ModuleModel _moduleModel;
        private readonly KeybindModel _pasteKeybind;
        private ImageSource _keyboardPreview;
        private ObservableCollection<LayerModel> _layers;
        private ObservableCollection<string> _profileNames;
        private bool _saving;
        private LayerModel _selectedLayer;
        private bool _showAll;

        public ProfileEditorViewModel(ProfileEditorModel profileEditorModel, DeviceManager deviceManager,
            LoopManager loopManager, ModuleModel moduleModel, MetroDialogService dialogService)
        {
            _deviceManager = deviceManager;
            _loopManager = loopManager;
            _moduleModel = moduleModel;
            _dialogService = dialogService;
            _copyKeybind = new KeybindModel("copy", new HotKey(Key.C, ModifierKeys.Control), PressType.Down, LayerToClipboard);
            _pasteKeybind = new KeybindModel("paste", new HotKey(Key.V, ModifierKeys.Control), PressType.Up, ClipboardToLayer);
            _placeholderKeyboard = KeyboardPreview = ImageUtilities.BitmapToBitmapImage(Resources.none);
            ProfileNames = new ObservableCollection<string>();
            Layers = new ObservableCollection<LayerModel>();
            ProfileEditorModel = profileEditorModel;
            ShowAll = true;

            PropertyChanged += EditorStateHandler;
            _deviceManager.OnKeyboardChanged += DeviceManagerOnOnKeyboardChanged;
            _moduleModel.ProfileChanged += ModuleModelOnProfileChanged;

            LoadProfiles();
        }

        public new void OnActivate()
        {
            base.OnActivate();

            _loopManager.RenderCompleted += LoopManagerOnRenderCompleted;
            KeybindManager.AddOrUpdate(_copyKeybind);
            KeybindManager.AddOrUpdate(_pasteKeybind);
        }

        public new void OnDeactivate(bool close)
        {
            base.OnDeactivate(close);

            SaveSelectedProfile();
            _loopManager.RenderCompleted -= LoopManagerOnRenderCompleted;
            KeybindManager.Remove(_copyKeybind);
            KeybindManager.Remove(_pasteKeybind);
        }

        #region LUA

        public void EditLua()
        {
            if (SelectedProfile == null)
                return;
            try
            {
                ProfileEditorModel.OpenLuaEditor(_moduleModel);
            }
            catch (Exception e)
            {
                _dialogService.ShowMessageBox("Couldn't open LUA file",
                    "Please make sure you have a text editor associated with the .lua extension.\n\n" +
                    "Windows error message: \n" + e.Message);
            }
        }

        #endregion

        #region Properties

        public ProfileEditorModel ProfileEditorModel { get; }

        public ObservableCollection<string> ProfileNames
        {
            get { return _profileNames; }
            set
            {
                if (Equals(value, _profileNames))
                    return;
                _profileNames = value;
                NotifyOfPropertyChange(() => ProfileNames);
            }
        }

        public ObservableCollection<LayerModel> Layers
        {
            get { return _layers; }
            set
            {
                if (Equals(value, _layers))
                    return;
                _layers = value;
                NotifyOfPropertyChange(() => Layers);
            }
        }


        public ImageSource KeyboardPreview
        {
            get { return _keyboardPreview; }
            set
            {
                if (Equals(value, _keyboardPreview))
                    return;
                _keyboardPreview = value;
                NotifyOfPropertyChange(() => KeyboardPreview);
            }
        }

        public bool ShowAll
        {
            get { return _showAll; }
            set
            {
                if (value == _showAll)
                    return;
                _showAll = value;
                NotifyOfPropertyChange();
            }
        }

        public string SelectedProfileName
        {
            get { return SelectedProfile?.Name; }
            set
            {
                SaveSelectedProfile();
                NotifyOfPropertyChange(() => SelectedProfileName);
                if (value != null)
                    ProfileEditorModel.ChangeProfileByName(_moduleModel, value);
            }
        }

        public LayerModel SelectedLayer
        {
            get { return _selectedLayer; }
            set
            {
                if (Equals(value, _selectedLayer))
                    return;

                _selectedLayer = value;
                NotifyOfPropertyChange(() => SelectedLayer);
                NotifyOfPropertyChange(() => LayerSelected);
            }
        }

        public ProfileModel SelectedProfile => _moduleModel?.ProfileModel;
        public bool ProfileSelected => SelectedProfile != null;
        public bool LayerSelected => SelectedProfile != null && SelectedLayer != null;
        public bool EditorEnabled => SelectedProfile != null && !SelectedProfile.IsDefault && _deviceManager.ActiveKeyboard != null;
        public bool LuaButtonVisible => !_moduleModel.IsOverlay;

        /// <summary>
        ///     Set to true to keep the preview active if using the profile editor
        /// </summary>
        public bool KeepActive { get; set; }

        #endregion

        #region Layers

        public void EditLayerFromDoubleClick()
        {
            if (SelectedLayer?.LayerType is FolderType)
                return;

            EditLayer();
        }

        public void EditLayer()
        {
            if (SelectedLayer == null)
                return;

            KeepActive = true;
            ProfileEditorModel.EditLayer(SelectedLayer, _moduleModel.DataModel);
            UpdateLayerList(SelectedLayer);
            KeepActive = false;
        }

        public void EditLayer(LayerModel layerModel)
        {
            if (layerModel == null)
                return;

            KeepActive = true;
            ProfileEditorModel.EditLayer(layerModel, _moduleModel.DataModel);
            UpdateLayerList(layerModel);
            KeepActive = false;
        }

        public LayerModel AddLayer()
        {
            if (SelectedProfile == null)
                return null;

            var layer = SelectedProfile.AddLayer(SelectedLayer);
            UpdateLayerList(layer);

            return layer;
        }

        public LayerModel AddFolder()
        {
            if (SelectedProfile == null)
                return null;

            var layer = AddLayer();
            if (layer == null)
                return null;

            layer.Name = "New folder";
            layer.LayerType = new FolderType();
            layer.LayerType.SetupProperties(layer);

            return layer;
        }

        public void RemoveLayer()
        {
            RemoveLayer(SelectedLayer);
        }

        public void RemoveLayer(LayerModel layer)
        {
            ProfileEditorModel.RemoveLayer(layer, SelectedProfile);
            UpdateLayerList(null);
        }

        public async void RenameLayer(LayerModel layer)
        {
            if (layer == null)
                return;

            KeepActive = true;
            var newName = await _dialogService.ShowInputDialog("Rename layer", "Please enter a name for the layer",
                new MetroDialogSettings {DefaultText = layer.Name});

            // Null when the user cancelled
            if (string.IsNullOrEmpty(newName))
                return;

            layer.Name = newName;
            UpdateLayerList(layer);
            KeepActive = false;
        }

        /// <summary>
        ///     Clones the currently selected layer and adds it to the profile, after the original
        /// </summary>
        public void CloneLayer()
        {
            if (SelectedLayer == null)
                return;

            CloneLayer(SelectedLayer);
        }

        /// <summary>
        ///     Clones the given layer and adds it to the profile, after the original
        /// </summary>
        /// <param name="layer"></param>
        public void CloneLayer(LayerModel layer)
        {
            var clone = GeneralHelpers.Clone(layer);
            layer.InsertAfter(clone);

            UpdateLayerList(clone);
        }

        public void LayerToClipboard()
        {
            if (SelectedLayer == null || !ActiveWindowHelper.MainWindowActive)
                return;

            // Probably not how the cool kids do it but leveraging on JsonConvert gives flawless serialization
            GeneralHelpers.ExecuteSta(() => Clipboard.SetData("layer", JsonConvert.SerializeObject(SelectedLayer)));
        }

        public void ClipboardToLayer()
        {
            if (!ActiveWindowHelper.MainWindowActive)
                return;

            GeneralHelpers.ExecuteSta(() =>
            {
                var data = (string) Clipboard.GetData("layer");
                if (data == null)
                    return;

                var layerModel = JsonConvert.DeserializeObject<LayerModel>(data);
                if (layerModel == null)
                    return;

                if (SelectedLayer != null)
                    SelectedLayer.InsertAfter(layerModel);
                else
                {
                    SelectedProfile.Layers.Add(layerModel);
                    SelectedProfile.FixOrder();
                }
                Execute.OnUIThread(() => UpdateLayerList(layerModel));
            });
        }

        private void UpdateLayerList(LayerModel selectModel)
        {
            // Update the UI
            Layers.Clear();
            SelectedLayer = null;

            if (SelectedProfile != null)
                Layers.AddRange(SelectedProfile.Layers);

            if (selectModel == null)
                return;

            // A small delay to allow the profile list to rebuild
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
                SelectedLayer = selectModel;
                SelectedProfile?.OnOnProfileUpdatedEvent();
            });
        }

        #endregion

        #region Profiles

        /// <summary>
        ///     Loads all profiles for the current game and keyboard
        /// </summary>
        private void LoadProfiles()
        {
            Execute.OnUIThread(() =>
            {
                ProfileNames.Clear();
                if (_moduleModel != null && _deviceManager.ActiveKeyboard != null)
                    ProfileNames.AddRange(ProfileProvider.GetProfileNames(_deviceManager.ActiveKeyboard, _moduleModel));

                NotifyOfPropertyChange(() => SelectedProfile);
            });
        }

        public void SaveSelectedProfile()
        {
            if (_saving || SelectedProfile == null || _deviceManager.ChangingKeyboard)
                return;

            SelectedProfile.Width = _deviceManager.ActiveKeyboard.Width;
            SelectedProfile.Height = _deviceManager.ActiveKeyboard.Height;

            _saving = true;
            try
            {
                ProfileProvider.AddOrUpdate(SelectedProfile);
            }
            catch (Exception)
            {
                // ignored
            }
            _saving = false;
        }

        public async void AddProfile()
        {
            if (_deviceManager.ActiveKeyboard == null)
            {
                _dialogService.ShowMessageBox("Cannot add profile.", "To add a profile, please select a keyboard in the options menu first.");
                return;
            }

            var profile = await ProfileEditorModel.AddProfile(_moduleModel);
            if (profile == null)
                return;

            LoadProfiles();
            _moduleModel.ChangeProfile(profile);
        }

        public async void RenameProfile()
        {
            if (SelectedProfile == null)
                return;

            var renameProfile = SelectedProfile;
            await ProfileEditorModel.RenameProfile(SelectedProfile);

            LoadProfiles();
            _moduleModel.ChangeProfile(renameProfile);
        }

        public async void DuplicateProfile()
        {
            if (SelectedProfile == null)
                return;

            var newProfle = await ProfileEditorModel.DuplicateProfile(SelectedProfile);
            if (newProfle == null)
                return;

            LoadProfiles();
            _moduleModel.ChangeProfile(newProfle);
        }

        public async void DeleteProfile()
        {
            if (SelectedProfile == null)
                return;

            var confirmed = await ProfileEditorModel.DeleteProfile(SelectedProfile, _moduleModel);
            if (!confirmed)
                return;

            LoadProfiles();
            ProfileEditorModel.ChangeProfileByName(_moduleModel, null);
        }

        public async void ImportProfile()
        {
            if (_deviceManager.ActiveKeyboard == null)
            {
                _dialogService.ShowMessageBox("Cannot import profile.",
                    "To import a profile, please select a keyboard in the options menu first.");
                return;
            }

            var importProfile = await ProfileEditorModel.ImportProfile(_moduleModel);
            if (importProfile == null)
                return;

            LoadProfiles();
            _moduleModel.ChangeProfile(importProfile);
        }

        public void ExportProfile()
        {
            if (SelectedProfile == null)
                return;

            var dialog = new SaveFileDialog {Filter = "Artemis profile (*.json)|*.json"};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            ProfileProvider.ExportProfile(SelectedProfile, dialog.FileName);
        }

        #endregion

        #region Rendering

        private readonly ImageSource _placeholderKeyboard;

        private void LoopManagerOnRenderCompleted(object sender, EventArgs eventArgs)
        {
            // Besides the usual checks, also check if the ActiveKeyboard isn't the NoneKeyboard
            if (SelectedProfile == null || _deviceManager.ActiveKeyboard == null || _deviceManager.ActiveKeyboard.Slug == "none")
            {
                // Setup layers for the next frame
                if (_moduleModel.IsInitialized && ActiveWindowHelper.MainWindowActive)
                    _moduleModel.PreviewLayers = new List<LayerModel>();

                if (!Equals(KeyboardPreview, _placeholderKeyboard))
                    KeyboardPreview = _placeholderKeyboard;

                return;
            }

            var renderedLayers = RenderLayers();
            var visual = new DrawingVisual();
            var previewSettings = _deviceManager.ActiveKeyboard.PreviewSettings;
            using (var drawingContext = visual.RenderOpen())
            {
                var baseRect = new Rect(0, 0, previewSettings.BackgroundRectangle.Width, previewSettings.BackgroundRectangle.Height);
                drawingContext.PushClip(new RectangleGeometry(baseRect));
                // Draw the keyboard image
                drawingContext.DrawImage(previewSettings.Image, baseRect);
                // Draw the layers semi-transparent
                drawingContext.PushOpacity(0.8);
                drawingContext.DrawImage(renderedLayers, previewSettings.OverlayRectangle);
                drawingContext.Pop();
                drawingContext.Pop();
            }
            var drawnPreview = new DrawingImage(visual.Drawing);
            drawnPreview.Freeze();
            KeyboardPreview = drawnPreview;
        }

        private DrawingImage RenderLayers()
        {
            var renderLayers = GetRenderLayers();
            // Draw the current frame to the preview
            var keyboardRect = _deviceManager.ActiveKeyboard.KeyboardRectangle();
            var visual = new DrawingVisual();
            using (var drawingContext = visual.RenderOpen())
            {
                // Setup the DrawingVisual's size
                drawingContext.PushClip(new RectangleGeometry(keyboardRect));
                drawingContext.DrawRectangle(new SolidColorBrush(Color.FromArgb(0, 0, 0, 0)), null, keyboardRect);

                // Draw the layers
                foreach (var layer in renderLayers)
                {
                    layer.Update(null, true, false);
                    if (layer.LayerType.ShowInEdtor)
                        layer.Draw(null, drawingContext, true, false);
                }

                // Get the selection color
                var accentColor = ThemeManager.DetectAppStyle(Application.Current)?.Item2?.Resources["AccentColor"];
                if (accentColor == null)
                {
                    var preview = new DrawingImage();
                    preview.Freeze();
                    KeyboardPreview = preview;
                    return new DrawingImage();
                }

                var pen = new Pen(new SolidColorBrush((Color) accentColor), 0.4);

                // Draw the selection outline and resize indicator
                if (SelectedLayer != null && SelectedLayer.MustDraw())
                {
                    var layerRect = SelectedLayer.Properties.PropertiesRect();
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

                SelectedProfile?.RaiseDeviceDrawnEvent(new ProfileDeviceEventsArg(DrawType.Preview, null, true,
                    drawingContext));

                // Remove the clip
                drawingContext.Pop();
            }
            var drawnPreview = new DrawingImage(visual.Drawing);
            drawnPreview.Freeze();

            // Setup layers for the next frame
            if (_moduleModel.IsInitialized && ActiveWindowHelper.MainWindowActive)
                _moduleModel.PreviewLayers = renderLayers;

            return drawnPreview;
        }

        public List<LayerModel> GetRenderLayers()
        {
            // Get the layers that must be drawn
            List<LayerModel> drawLayers;
            if (ShowAll)
                return SelectedProfile.GetRenderLayers(null, false, true);

            if (SelectedLayer == null || !SelectedLayer.Enabled)
                return new EditableList<LayerModel>();

            if (SelectedLayer.LayerType is FolderType)
                drawLayers = SelectedLayer.GetRenderLayers(null, false, true);
            else
                drawLayers = new List<LayerModel> {SelectedLayer};

            return drawLayers;
        }

        #endregion

        #region Mouse actions

        private DateTime _downTime;
        private LayerModel _draggingLayer;
        private Point? _draggingLayerOffset;
        private Cursor _keyboardPreviewCursor;
        private bool _resizing;

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
        ///     Second handler for clicking, selects a the layer the user clicked on.
        /// </summary>
        /// <param name="e"></param>
        public void MouseUpKeyboardPreview(MouseButtonEventArgs e)
        {
            if (SelectedProfile == null || SelectedProfile.IsDefault)
                return;

            var timeSinceDown = DateTime.Now - _downTime;
            if (!(timeSinceDown.TotalMilliseconds < 500))
                return;
            if (_draggingLayer != null)
                return;

            var pos = GetScaledPosition(e);
            var hoverLayer = GetLayers().Where(l => l.MustDraw()).FirstOrDefault(l => l.Properties.PropertiesRect(1).Contains(pos.X, pos.Y));

            if (hoverLayer != null)
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

            var pos = GetScaledPosition(e);
            var hoverLayer = GetLayers().Where(l => l.MustDraw()).FirstOrDefault(l => l.Properties.PropertiesRect(1).Contains(pos.X, pos.Y));

            HandleDragging(e, pos.X, pos.Y, hoverLayer);

            if (hoverLayer == null)
            {
                KeyboardPreviewCursor = Cursors.Arrow;
                return;
            }

            // Turn the mouse pointer into a hand if hovering over an active layer
            if (hoverLayer == SelectedLayer)
            {
                var rect = hoverLayer.Properties.PropertiesRect(1);
                KeyboardPreviewCursor = Math.Sqrt(Math.Pow(pos.X - rect.BottomRight.X, 2) + Math.Pow(pos.Y - rect.BottomRight.Y, 2)) < 0.6
                    ? Cursors.SizeNWSE
                    : Cursors.SizeAll;
            }
            else
                KeyboardPreviewCursor = Cursors.Hand;
        }

        private Point GetScaledPosition(MouseEventArgs e)
        {
            var previewSettings = _deviceManager.ActiveKeyboard.PreviewSettings;

            var sourceImage = (Image) e.OriginalSource;
            var pos = e.GetPosition(sourceImage);
            var widthScale = sourceImage.ActualWidth / _deviceManager.ActiveKeyboard.PreviewSettings.BackgroundRectangle.Width;
            var heightScale = sourceImage.ActualHeight / _deviceManager.ActiveKeyboard.PreviewSettings.BackgroundRectangle.Height;

            // Remove the preview settings' offset from the cursor postion
            pos.X = pos.X - previewSettings.OverlayRectangle.X * widthScale;
            pos.Y = pos.Y - previewSettings.OverlayRectangle.Y * heightScale;

            // Scale the X and Y position down to match the keyboard's physical size and thus the layer positions
            pos.X = pos.X * (SelectedProfile.Width / (previewSettings.OverlayRectangle.Width * widthScale));
            pos.Y = pos.Y * (SelectedProfile.Height / (previewSettings.OverlayRectangle.Height * heightScale));

            return pos;
        }

        public Cursor KeyboardPreviewCursor
        {
            get { return _keyboardPreviewCursor; }
            set
            {
                if (Equals(value, _keyboardPreviewCursor))
                    return;
                _keyboardPreviewCursor = value;
                NotifyOfPropertyChange(() => KeyboardPreviewCursor);
            }
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
                _draggingLayer != null && SelectedLayer != _draggingLayer)
            {
                _draggingLayerOffset = null;
                _draggingLayer = null;
                return;
            }

            if (SelectedLayer == null || SelectedLayer.LayerType != null && !SelectedLayer.LayerType.ShowInEdtor)
                return;

            // Setup the dragging state on mouse press
            if (_draggingLayerOffset == null && hoverLayer != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var layerRect = hoverLayer.Properties.PropertiesRect(1);

                _draggingLayerOffset = new Point(x - SelectedLayer.Properties.X, y - SelectedLayer.Properties.Y);
                _draggingLayer = hoverLayer;
                // Detect dragging if cursor is in the bottom right
                _resizing = Math.Sqrt(Math.Pow(x - layerRect.BottomRight.X, 2) +
                                      Math.Pow(y - layerRect.BottomRight.Y, 2)) < 0.6;
            }

            if (_draggingLayerOffset == null || _draggingLayer == null || _draggingLayer != SelectedLayer)
                return;

            var draggingProps = _draggingLayer.Properties;
            // If no setup or reset was done, handle the actual dragging action
            if (_resizing)
            {
                var newWidth = Math.Round(x - draggingProps.X);
                var newHeight = Math.Round(y - draggingProps.Y);

                // Ensure the layer doesn't leave the canvas
                if (newWidth < 1 || draggingProps.X + newWidth <= 0)
                    newWidth = draggingProps.Width;
                if (newHeight < 1 || draggingProps.Y + newHeight <= 0)
                    newHeight = draggingProps.Height;

                draggingProps.Width = newWidth;
                draggingProps.Height = newHeight;
            }
            else
            {
                var newX = Math.Round(x - _draggingLayerOffset.Value.X);
                var newY = Math.Round(y - _draggingLayerOffset.Value.Y);

                // Ensure the layer doesn't leave the canvas
                if (newX >= SelectedProfile.Width || newX + draggingProps.Width <= 0)
                    newX = draggingProps.X;
                if (newY >= SelectedProfile.Height || newY + draggingProps.Height <= 0)
                    newY = draggingProps.Y;

                draggingProps.X = newX;
                draggingProps.Y = newY;
            }
        }

        private List<LayerModel> GetLayers()
        {
            if (ShowAll)
                return SelectedProfile.GetLayers();
            if (SelectedLayer == null)
                return new List<LayerModel>();

            lock (SelectedLayer)
            {
                // Get the layers that must be drawn
                if (SelectedLayer.LayerType is FolderType)
                    return SelectedLayer.GetLayers().ToList();
                return new List<LayerModel> {SelectedLayer};
            }
        }

        #endregion

        #region Event handles

        public void DragOver(IDropInfo dropInfo)
        {
            var source = dropInfo.Data as LayerModel;
            var target = dropInfo.TargetItem as LayerModel;
            if (source == null || target == null || source == target)
                return;

            if (dropInfo.InsertPosition == RelativeInsertPosition.TargetItemCenter &&
                target.LayerType is FolderType)
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
            {
                var profile = source.Profile;
                source.Profile.Layers.Remove(source);
                profile.FixOrder();
            }
            else
            {
                var parent = source.Parent;
                source.Parent.Children.Remove(source);
                parent.FixOrder();
            }

            if (dropInfo.InsertPosition == RelativeInsertPosition.TargetItemCenter &&
                target.LayerType is FolderType)
            {
                // Insert into folder
                source.Order = -1;
                target.Children.Add(source);
                target.FixOrder();
                target.Expanded = true;
            }
            else
            {
                // Insert the source into it's new profile/parent and update the order
                if (dropInfo.InsertPosition == RelativeInsertPosition.AfterTargetItem ||
                    dropInfo.InsertPosition ==
                    (RelativeInsertPosition.TargetItemCenter | RelativeInsertPosition.AfterTargetItem))
                    target.InsertAfter(source);
                else
                    target.InsertBefore(source);
            }

            UpdateLayerList(source);
        }

        private void ModuleModelOnProfileChanged(object sender, ProfileChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => SelectedProfileName);
            NotifyOfPropertyChange(() => SelectedProfile);
        }

        /// <summary>
        ///     Handles chaning the active keyboard, updating the profiles collection
        /// </summary>
        private void DeviceManagerOnOnKeyboardChanged(object sender, KeyboardChangedEventArgs e)
        {
            LoadProfiles();
        }

        private void EditorStateHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedProfile")
                return;

            // Update editor enabled state
            NotifyOfPropertyChange(() => EditorEnabled);
            // Update interface
            Layers.Clear();

            if (SelectedProfile != null)
                Layers.AddRange(SelectedProfile.Layers);

            NotifyOfPropertyChange(() => ProfileSelected);
        }

        #endregion
    }
}
