using System.ComponentModel;
using System.Dynamic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Artemis.DAL;
using Artemis.DeviceProviders;
using Artemis.Events;
using Artemis.InjectionFactories;
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
        private readonly EffectModel _gameModel;
        private readonly ILayerEditorVmFactory _layerEditorVmFactory;
        private readonly MainManager _mainManager;
        private ImageSource _keyboardPreview;
        private BindableCollection<LayerModel> _layers;
        private BindableCollection<ProfileModel> _profiles;
        private ProfileModel _selectedProfile;

        public ProfileEditorViewModel(IEventAggregator events, MainManager mainManager, EffectModel gameModel,
            ProfileViewModel profileViewModel, MetroDialogService dialogService, string lastProfile,
            ILayerEditorVmFactory layerEditorVmFactory)
        {
            _mainManager = mainManager;
            _gameModel = gameModel;
            _layerEditorVmFactory = layerEditorVmFactory;

            Profiles = new BindableCollection<ProfileModel>();
            Layers = new BindableCollection<LayerModel>();
            ProfileViewModel = profileViewModel;
            DialogService = dialogService;
            LastProfile = lastProfile;

            events.Subscribe(this);

            ProfileViewModel.PropertyChanged += PropertyChangeHandler;
            PropertyChanged += PropertyChangeHandler;
            LoadProfiles();
        }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public string LastProfile { get; set; }

        public ProfileViewModel ProfileViewModel { get; set; }

        public bool EditorEnabled
            =>
                SelectedProfile != null && !SelectedProfile.IsDefault &&
                _mainManager.DeviceManager.ActiveKeyboard != null;

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
                target.LayerType == LayerType.Folder)
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

            // If a profile name was provided, try to load it
            ProfileModel lastProfileModel = null;
            if (!string.IsNullOrEmpty(LastProfile))
                lastProfileModel = Profiles.FirstOrDefault(p => p.Name == LastProfile);

            SelectedProfile = lastProfileModel ?? Profiles.FirstOrDefault();
        }

        public void EditLayerFromDoubleClick()
        {
            if (ProfileViewModel.SelectedLayer?.LayerType == LayerType.Folder)
                return;

            EditLayer();
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
            var editorVm = _layerEditorVmFactory.CreateLayerEditorVm(_gameModel.DataModel, layer);
            dynamic settings = new ExpandoObject();
            var icon = ImageUtilities.GenerateWindowIcon();

            settings.Title = "Artemis | Edit " + layer.Name;
            settings.Icon = icon;

            manager.ShowDialog(editorVm, null, settings);

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

            if (ProfileViewModel.SelectedLayer != null)
                ProfileViewModel.SelectedLayer.InsertAfter(layer);
            else
            {
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
        ///     Clones the currently selected layer and adds it to the profile, after the original
        /// </summary>
        public void CloneLayer()
        {
            if (ProfileViewModel.SelectedLayer == null)
                return;

            CloneLayer(ProfileViewModel.SelectedLayer);
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
            if (_mainManager.DeviceManager.ActiveKeyboard == null)
            {
                DialogService.ShowMessageBox("Cannot add profile.",
                    "To add a profile, please select a keyboard in the options menu first.");
                return;
            }

            var name = await DialogService.ShowInputDialog("Add new profile",
                "Please provide a profile name unique to this game and keyboard.");

            // Null when the user cancelled
            if (name == null)
                return;

            if (name.Length < 2)
            {
                DialogService.ShowMessageBox("Invalid profile name", "Please provide a valid profile name");
                return;
            }

            var profile = new ProfileModel
            {
                Name = name,
                KeyboardSlug = _mainManager.DeviceManager.ActiveKeyboard.Slug,
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
            SelectedProfile.Name = await DialogService
                .ShowInputDialog("Rename profile", "Please enter a unique new profile name");

            // Null when the user cancelled
            if (string.IsNullOrEmpty(SelectedProfile.Name) || SelectedProfile.Name.Length < 2)
            {
                SelectedProfile.Name = oldName;
                return;
            }

            // Verify the name
            while (ProfileProvider.GetAll().Contains(SelectedProfile))
            {
                SelectedProfile.Name = await DialogService.
                    ShowInputDialog("Name already in use", "Please enter a unique new profile name");

                // Null when the user cancelled
                if (string.IsNullOrEmpty(SelectedProfile.Name) || SelectedProfile.Name.Length < 2)
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
            if (_mainManager.DeviceManager.ActiveKeyboard == null)
            {
                DialogService.ShowMessageBox("Cannot import profile.",
                    "To import a profile, please select a keyboard in the options menu first.");
                return;
            }
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
            if (profile.KeyboardSlug != _mainManager.DeviceManager.ActiveKeyboard.Slug)
            {
                var adjustKeyboard = await DialogService.ShowQuestionMessageBox("Profile not inteded for this keyboard",
                    $"Watch out, this profile wasn't ment for this keyboard, but for the {profile.KeyboardSlug}. " +
                    "You can still import it but you'll probably have to do some adjusting\n\n" +
                    "Continue?");
                if (!adjustKeyboard.Value)
                    return;

                profile.KeyboardSlug = _mainManager.DeviceManager.ActiveKeyboard.Slug;
                profile.IsDefault = false;
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