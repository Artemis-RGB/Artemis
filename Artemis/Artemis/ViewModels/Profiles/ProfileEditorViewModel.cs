﻿using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.DAL;
using Artemis.DeviceProviders;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Services;
using Artemis.Styles.DropTargetAdorners;
using Artemis.Utilities;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls.Dialogs;
using Ninject;
using Application = System.Windows.Application;
using DragDropEffects = System.Windows.DragDropEffects;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Screen = Caliburn.Micro.Screen;

namespace Artemis.ViewModels.Profiles
{
    public sealed class ProfileEditorViewModel : Screen, IHandle<ActiveKeyboardChanged>, IDropTarget
    {
        private readonly GameModel _gameModel;
        private readonly MainManager _mainManager;
        private LayerEditorViewModel _editorVm;
        private ImageSource _keyboardPreview;
        private BindableCollection<LayerModel> _layers;
        private BindableCollection<ProfileModel> _profiles;
        private ProfileModel _selectedProfile;

        public ProfileEditorViewModel(IEventAggregator events, MainManager mainManager, GameModel gameModel,
            ProfileViewModel profileViewModel, MetroDialogService dialogService)
        {
            _mainManager = mainManager;
            _gameModel = gameModel;

            Profiles = new BindableCollection<ProfileModel>();
            Layers = new BindableCollection<LayerModel>();
            ProfileViewModel = profileViewModel;
            DialogService = dialogService;

            events.Subscribe(this);

            ProfileViewModel.PropertyChanged += PropertyChangeHandler;
            PropertyChanged += PropertyChangeHandler;
            LoadProfiles();
        }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public ProfileViewModel ProfileViewModel { get; set; }

        public bool EditorEnabled => SelectedProfile != null && !SelectedProfile.IsDefault;

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

        public ProfileModel SelectedProfile
        {
            get { return _selectedProfile; }
            set
            {
                if (Equals(value, _selectedProfile)) return;
                _selectedProfile = value;
                NotifyOfPropertyChange(() => SelectedProfile);
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

        public PreviewSettings? PreviewSettings => _mainManager.DeviceManager.ActiveKeyboard?.PreviewSettings;

        public bool ProfileSelected => SelectedProfile != null;
        public bool LayerSelected => SelectedProfile != null && ProfileViewModel.SelectedLayer != null;

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

            if (e.PropertyName == "SelectedLayer")
            {
                NotifyOfPropertyChange(() => LayerSelected);
                return;
            }

            if (SelectedProfile != null)
                ProfileProvider.AddOrUpdate(SelectedProfile);

            if (e.PropertyName != "SelectedProfile")
                return;

            // Update editor enabled state
            NotifyOfPropertyChange(() => EditorEnabled);
            // Update ProfileViewModel
            ProfileViewModel.SelectedProfile = SelectedProfile;
            // Update interface
            Layers.Clear();
            if (SelectedProfile != null)
                Layers.AddRange(SelectedProfile.Layers);

            NotifyOfPropertyChange(() => ProfileSelected);
        }

        /// <summary>
        ///     Loads all profiles for the current game and keyboard
        /// </summary>
        private void LoadProfiles()
        {
            Profiles.Clear();
            if (_gameModel == null || _mainManager.DeviceManager.ActiveKeyboard == null)
                return;

            Profiles.AddRange(ProfileProvider.GetAll(_gameModel, _mainManager.DeviceManager.ActiveKeyboard));
            SelectedProfile = Profiles.FirstOrDefault();
        }

        public void EditLayer()
        {
            if (ProfileViewModel.SelectedLayer == null)
                return;

            EditLayer(ProfileViewModel.SelectedLayer);
        }

        /// <summary>
        ///     Opens a new LayerEditorView for the given layer
        /// </summary>
        /// <param name="layer">The layer to open the view for</param>
        public void EditLayer(LayerModel layer)
        {
            IWindowManager manager = new WindowManager();
            _editorVm = new LayerEditorViewModel(_gameModel.GameDataModel, layer);
            dynamic settings = new ExpandoObject();
            var iconImage = new Image
            {
                Source = (DrawingImage) Application.Current.MainWindow.Resources["BowIcon"],
                Stretch = Stretch.Uniform,
                Margin = new Thickness(20)
            };
            iconImage.Arrange(new Rect(0, 0, 100, 100));
            var bitmap = new RenderTargetBitmap(100, 100, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(iconImage);

            settings.Title = "Artemis | Edit " + layer.Name;
            settings.Icon = bitmap;

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

            // Create a new layer
            var layer = LayerModel.CreateLayer();

            // If there is a selected layer and it has a parent, bind the new layer to it
            if (ProfileViewModel.SelectedLayer?.Parent != null)
            {
                layer.Order = ProfileViewModel.SelectedLayer.Order + 1;
                ProfileViewModel.SelectedLayer.Parent.Children.Add(layer);
                ProfileViewModel.SelectedLayer.Parent.FixOrder();
            }
            else
            {
                // If there was no parent but there is a layer selected, put it below the selected layer
                if (ProfileViewModel.SelectedLayer != null)
                    layer.Order = ProfileViewModel.SelectedLayer.Order + 1;

                SelectedProfile.Layers.Add(layer);
                SelectedProfile.FixOrder();
            }

            UpdateLayerList(layer);
        }

        /// <summary>
        ///     Removes the currently selected layer from the profile
        /// </summary>
        public void RemoveLayer()
        {
            RemoveLayer(ProfileViewModel.SelectedLayer);
        }

        /// <summary>
        ///     Removes the given layer from the profile
        /// </summary>
        /// <param name="layer"></param>
        public void RemoveLayer(LayerModel layer)
        {
            if (layer == null)
                return;

            if (layer.Parent != null)
            {
                var parent = layer.Parent;
                layer.Parent.Children.Remove(layer);
                parent.FixOrder();
            }
            else if (layer.Profile != null)
            {
                var profile = layer.Profile;
                layer.Profile.Layers.Remove(layer);
                profile.FixOrder();
            }

            // Extra cleanup in case of a wonky layer that has no parent
            if (SelectedProfile.Layers.Contains(layer))
                SelectedProfile.Layers.Remove(layer);

            UpdateLayerList(null);
        }

        public async void RenameLayer(LayerModel layer)
        {
            if (layer == null)
                return;

            var newName =
                await
                    DialogService.ShowInputDialog("Rename layer", "Please enter a name for the layer",
                        new MetroDialogSettings {DefaultText = layer.Name});
            // Null when the user cancelled
            if (string.IsNullOrEmpty(newName))
                return;

            layer.Name = newName;
            UpdateLayerList(layer);
        }

        /// <summary>
        ///     Clones the currently selected layer and adds it to the profile, on top of the original
        /// </summary>
        public void CloneLayer()
        {
            if (ProfileViewModel.SelectedLayer == null)
                return;

            CloneLayer(ProfileViewModel.SelectedLayer);
        }

        /// <summary>
        ///     Clones the given layer and adds it to the profile, on top of the original
        /// </summary>
        /// <param name="layer"></param>
        public void CloneLayer(LayerModel layer)
        {
            var clone = GeneralHelpers.Clone(layer);
            clone.Order++;

            if (layer.Parent != null)
            {
                layer.Parent.Children.Add(clone);
                layer.Parent.FixOrder();
            }
            else if (layer.Profile != null)
            {
                layer.Profile.Layers.Add(clone);
                layer.Profile.FixOrder();
            }

            UpdateLayerList(clone);
        }

        private void UpdateLayerList(LayerModel selectModel)
        {
            // Update the UI
            Layers.Clear();
            if (SelectedProfile != null)
                Layers.AddRange(SelectedProfile.Layers);

            if (selectModel == null)
                return;

            // A small delay to allow the profile list to rebuild
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(20);
                ProfileViewModel.SelectedLayer = selectModel;
            });
        }

        /// <summary>
        ///     Handler for clicking
        /// </summary>
        /// <param name="e"></param>
        public void MouseDownKeyboardPreview(MouseButtonEventArgs e)
        {
            ProfileViewModel.MouseDownKeyboardPreview(e);
        }

        /// <summary>
        ///     Second handler for clicking, selects a the layer the user clicked on
        ///     if the used clicked on an empty spot, deselects the current layer
        /// </summary>
        /// <param name="e"></param>
        public void MouseUpKeyboardPreview(MouseButtonEventArgs e)
        {
            ProfileViewModel.MouseUpKeyboardPreview(e);
        }

        /// <summary>
        ///     Handler for resizing and moving the currently selected layer
        /// </summary>
        /// <param name="e"></param>
        public void MouseMoveKeyboardPreview(MouseEventArgs e)
        {
            ProfileViewModel.MouseMoveKeyboardPreview(e);
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
                KeyboardName = _mainManager.DeviceManager.ActiveKeyboard.Name,
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

            newProfile.IsDefault = false;
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