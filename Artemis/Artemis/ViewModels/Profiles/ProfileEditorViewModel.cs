using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.DeviceProviders;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Folder;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Styles.DropTargetAdorners;
using Artemis.Utilities;
using Caliburn.Micro;
using GongSolutions.Wpf.DragDrop;
using MahApps.Metro.Controls.Dialogs;
using Ninject.Parameters;
using NuGet;
using DragDropEffects = System.Windows.DragDropEffects;
using IDropTarget = GongSolutions.Wpf.DragDrop.IDropTarget;
using MouseEventArgs = System.Windows.Input.MouseEventArgs;
using Screen = Caliburn.Micro.Screen;
using Timer = System.Timers.Timer;

namespace Artemis.ViewModels.Profiles
{
    public sealed class ProfileEditorViewModel : Screen, IDropTarget, IDisposable
    {
        private readonly DeviceManager _deviceManager;
        private readonly MetroDialogService _dialogService;
        private readonly LuaManager _luaManager;
        private readonly ModuleModel _moduleModel;
        private readonly Timer _saveTimer;
        private readonly WindowService _windowService;
        private ImageSource _keyboardPreview;
        private ObservableCollection<LayerModel> _layers;
        private ObservableCollection<string> _profileNames;
        private bool _saving;
        private FileSystemWatcher _watcher;

        public ProfileEditorViewModel(DeviceManager deviceManager, LuaManager luaManager, ModuleModel moduleModel,
            ProfileViewModel profileViewModel, MetroDialogService dialogService, WindowService windowService)
        {
            _deviceManager = deviceManager;
            _luaManager = luaManager;
            _moduleModel = moduleModel;
            _dialogService = dialogService;
            _windowService = windowService;

            ProfileNames = new ObservableCollection<string>();
            Layers = new ObservableCollection<LayerModel>();
            ProfileViewModel = profileViewModel;

            ProfileViewModel.ModuleModel = _moduleModel;

            PropertyChanged += EditorStateHandler;
            ProfileViewModel.PropertyChanged += LayerSelectedHandler;
            _deviceManager.OnKeyboardChanged += DeviceManagerOnOnKeyboardChanged;
            _moduleModel.ProfileChanged += ModuleModelOnProfileChanged;
            LoadProfiles();

            _saveTimer = new Timer(5000);
            _saveTimer.Elapsed += ProfileSaveHandler;
            _saveTimer.Start();
        }

        public ProfileViewModel ProfileViewModel { get; set; }

        public bool EditorEnabled
            =>
                SelectedProfile != null && !SelectedProfile.IsDefault &&
                _deviceManager.ActiveKeyboard != null;

        public ProfileModel SelectedProfile => _moduleModel?.ProfileModel;

        public ObservableCollection<string> ProfileNames
        {
            get { return _profileNames; }
            set
            {
                if (Equals(value, _profileNames)) return;
                _profileNames = value;
                NotifyOfPropertyChange(() => ProfileNames);
            }
        }

        public ObservableCollection<LayerModel> Layers
        {
            get { return _layers; }
            set
            {
                if (Equals(value, _layers)) return;
                _layers = value;
                NotifyOfPropertyChange(() => Layers);
            }
        }

        public string SelectedProfileName
        {
            get { return SelectedProfile?.Name; }
            set
            {
                if (value == SelectedProfile?.Name)
                    return;

                _moduleModel.ChangeProfile(ProfileProvider.GetProfile(_deviceManager.ActiveKeyboard, _moduleModel, value));
                NotifyOfPropertyChange(() => SelectedProfileName);
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

        public PreviewSettings? PreviewSettings => _deviceManager.ActiveKeyboard?.PreviewSettings;

        public bool ProfileSelected => SelectedProfile != null;
        public bool LayerSelected => SelectedProfile != null && ProfileViewModel.SelectedLayer != null;

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
            NotifyOfPropertyChange(() => ProfileViewModel.SelectedProfile);
        }

        /// <summary>
        ///     Handles chaning the active keyboard, updating the preview image and profiles collection
        /// </summary>
        private void DeviceManagerOnOnKeyboardChanged(object sender, KeyboardChangedEventArgs e)
        {
            NotifyOfPropertyChange(() => PreviewSettings);
            LoadProfiles();
        }

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


        public void EditLayerFromDoubleClick()
        {
            if (ProfileViewModel.SelectedLayer?.LayerType is FolderType)
                return;

            EditLayer();
        }

        public void EditLayer()
        {
            if (ProfileViewModel.SelectedLayer == null)
                return;

            var selectedLayer = ProfileViewModel.SelectedLayer;
            EditLayer(selectedLayer);
        }

        /// <summary>
        ///     Opens a new LayerEditorView for the given layer
        /// </summary>
        /// <param name="layer">The layer to open the view for</param>
        public void EditLayer(LayerModel layer)
        {
            if (layer == null)
                return;

            IParameter[] args =
            {
                new ConstructorArgument("dataModel", _moduleModel.DataModel),
                new ConstructorArgument("layer", layer)
            };
            _windowService.ShowDialog<LayerEditorViewModel>(args);

            // If the layer was a folder, but isn't anymore, assign it's children to it's parent.
            if (!(layer.LayerType is FolderType) && layer.Children.Any())
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

            UpdateLayerList(layer);
        }

        /// <summary>
        ///     Adds a new layer to the profile and selects it
        /// </summary>
        public LayerModel AddLayer()
        {
            if (SelectedProfile == null)
                return null;

            var layer = SelectedProfile.AddLayer(ProfileViewModel.SelectedLayer);
            UpdateLayerList(layer);

            return layer;
        }

        public LayerModel AddFolder()
        {
            var layer = AddLayer();
            if (layer == null)
                return null;

            layer.Name = "New folder";
            layer.LayerType = new FolderType();
            layer.LayerType.SetupProperties(layer);

            return layer;
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
                    _dialogService.ShowInputDialog("Rename layer", "Please enter a name for the layer",
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
            ProfileViewModel.SelectedLayer = null;

            if (SelectedProfile != null)
                Layers.AddRange(SelectedProfile.Layers);

            if (selectModel == null)
                return;

            // A small delay to allow the profile list to rebuild
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(100);
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
            if (_deviceManager.ActiveKeyboard == null)
            {
                _dialogService.ShowMessageBox("Cannot add profile.",
                    "To add a profile, please select a keyboard in the options menu first.");
                return;
            }

            var name = await _dialogService.ShowInputDialog("Add new profile",
                "Please provide a profile name unique to this game and keyboard.");

            // Null when the user cancelled
            if (name == null)
                return;

            if (name.Length < 2)
            {
                _dialogService.ShowMessageBox("Invalid profile name", "Please provide a valid profile name");
                return;
            }

            var profile = new ProfileModel
            {
                Name = name,
                KeyboardSlug = _deviceManager.ActiveKeyboard.Slug,
                Width = _deviceManager.ActiveKeyboard.Width,
                Height = _deviceManager.ActiveKeyboard.Height,
                GameName = _moduleModel.Name
            };

            if (!ProfileProvider.IsProfileUnique(profile))
            {
                var overwrite = await _dialogService.ShowQuestionMessageBox("Overwrite existing profile",
                    "A profile with this name already exists for this game. Would you like to overwrite it?");
                if (!overwrite.Value)
                    return;
            }

            ProfileProvider.AddOrUpdate(profile);

            LoadProfiles();
        }

        public async void RenameProfile()
        {
            if (SelectedProfile == null)
                return;

            var oldName = SelectedProfile.Name;
            var name = await _dialogService.ShowInputDialog("Rename profile", "Please enter a unique new profile name");

            // Null when the user cancelled
            if (string.IsNullOrEmpty(name) || name.Length < 2)
                return;

            SelectedProfile.Name = name;

            // Verify the name
            while (!ProfileProvider.IsProfileUnique(SelectedProfile))
            {
                name = await _dialogService
                    .ShowInputDialog("Name already in use", "Please enter a unique new profile name");

                // Null when the user cancelled
                if (string.IsNullOrEmpty(name) || name.Length < 2)
                {
                    SelectedProfile.Name = oldName;
                    return;
                }
                SelectedProfile.Name = name;
            }

            var profile = SelectedProfile;
            ProfileProvider.RenameProfile(profile, name);

            LoadProfiles();
        }

        public async void DuplicateProfile()
        {
            if (SelectedProfile == null)
                return;

            var newProfile = GeneralHelpers.Clone(SelectedProfile);
            newProfile.Name = await _dialogService
                .ShowInputDialog("Duplicate profile", "Please enter a unique profile name");

            // Null when the user cancelled
            if (string.IsNullOrEmpty(newProfile.Name))
                return;

            // Verify the name
            while (!ProfileProvider.IsProfileUnique(newProfile))
            {
                newProfile.Name = await _dialogService
                    .ShowInputDialog("Name already in use", "Please enter a unique profile name");

                // Null when the user cancelled
                if (string.IsNullOrEmpty(newProfile.Name))
                    return;
            }

            newProfile.IsDefault = false;
            ProfileProvider.AddOrUpdate(newProfile);
            LoadProfiles();
        }

        public async void DeleteProfile()
        {
            if (SelectedProfile == null)
                return;

            var confirm = await
                _dialogService.ShowQuestionMessageBox("Delete profile",
                    $"Are you sure you want to delete the profile named: {SelectedProfile.Name}?\n\n" +
                    "This cannot be undone.");
            if (!confirm.Value)
                return;

            var defaultProfile = ProfileProvider.GetProfile(_deviceManager.ActiveKeyboard, _moduleModel, "Default");
            var deleteProfile = SelectedProfile;

            _moduleModel.ChangeProfile(defaultProfile);
            ProfileProvider.DeleteProfile(deleteProfile);

            LoadProfiles();
        }

        public async void ImportProfile()
        {
            if (_deviceManager.ActiveKeyboard == null)
            {
                _dialogService.ShowMessageBox("Cannot import profile.",
                    "To import a profile, please select a keyboard in the options menu first.");
                return;
            }
            var dialog = new OpenFileDialog {Filter = "Artemis profile (*.json)|*.json"};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return;

            var profile = ProfileProvider.LoadProfileIfValid(dialog.FileName);
            if (profile == null)
            {
                _dialogService.ShowErrorMessageBox("Oh noes, the profile you provided is invalid. " +
                                                   "If this keeps happening, please make an issue on GitHub and provide the profile.");
                return;
            }

            // Verify the game
            if (profile.GameName != _moduleModel.Name)
            {
                _dialogService.ShowErrorMessageBox(
                    $"Oh oops! This profile is ment for {profile.GameName}, not {_moduleModel.Name} :c");
                return;
            }

            // Verify the keyboard
            var deviceManager = _deviceManager;
            if (profile.KeyboardSlug != deviceManager.ActiveKeyboard.Slug)
            {
                var adjustKeyboard = await _dialogService.ShowQuestionMessageBox(
                    "Profile not inteded for this keyboard",
                    $"Watch out, this profile wasn't ment for this keyboard, but for the {profile.KeyboardSlug}. " +
                    "You can still import it but you'll probably have to do some adjusting\n\n" +
                    "Continue?");
                if (!adjustKeyboard.Value)
                    return;

                // Resize layers that are on the full keyboard width
                profile.ResizeLayers(deviceManager.ActiveKeyboard);
                // Put layers back into the canvas if they fell outside it
                profile.FixBoundaries(deviceManager.ActiveKeyboard.KeyboardRectangle(1));

                // Setup profile metadata to match the new keyboard
                profile.KeyboardSlug = deviceManager.ActiveKeyboard.Slug;
                profile.Width = deviceManager.ActiveKeyboard.Width;
                profile.Height = deviceManager.ActiveKeyboard.Height;
            }

            profile.IsDefault = false;

            // Verify the name
            while (!ProfileProvider.IsProfileUnique(profile))
            {
                profile.Name = await _dialogService.ShowInputDialog("Rename imported profile",
                    "A profile with this name already exists for this game. Please enter a new name");

                // Null when the user cancelled
                if (string.IsNullOrEmpty(profile.Name))
                    return;
            }

            ProfileProvider.AddOrUpdate(profile);
            LoadProfiles();
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

        public void EditLua()
        {
            if (SelectedProfile == null)
                return;
            try
            {
                OpenEditor();
            }
            catch (Exception e)
            {
                _dialogService.ShowMessageBox("Couldn't open LUA file",
                    "Please make sure you have a text editor associated with the .lua extension.\n\n" +
                    "Windows error message: \n" + e.Message);
            }
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

        private void LayerSelectedHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "SelectedLayer")
                return;

            NotifyOfPropertyChange(() => LayerSelected);
        }

        private void ProfileSaveHandler(object sender, ElapsedEventArgs e)
        {
            SaveSelectedProfile();
        }

        public void SaveSelectedProfile()
        {
            if (_saving || SelectedProfile == null || _deviceManager.ChangingKeyboard)
                return;

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

        #region LUA Editor

        public void OpenEditor()
        {
            if (SelectedProfile == null)
                return;

            // Create a temp file
            var fileName = Guid.NewGuid() + ".lua";
            var file = File.Create(Path.GetTempPath() + fileName);
            file.Dispose();

            // Add instructions to LUA script if it's a new file
            if (string.IsNullOrEmpty(SelectedProfile.LuaScript))
                SelectedProfile.LuaScript = Encoding.UTF8.GetString(Resources.lua_placeholder);
            File.WriteAllText(Path.GetTempPath() + fileName, SelectedProfile.LuaScript);

            // Watch the file for changes
            SetupWatcher(Path.GetTempPath(), fileName);

            // Open the temp file with the default editor
            System.Diagnostics.Process.Start(Path.GetTempPath() + fileName);
        }

        private void SetupWatcher(string path, string fileName)
        {
            if (_watcher == null)
            {
                _watcher = new FileSystemWatcher(Path.GetTempPath(), fileName);
                _watcher.Changed += LuaFileChanged;
                _watcher.EnableRaisingEvents = true;
            }

            _watcher.Path = path;
            _watcher.Filter = fileName;
        }

        private void LuaFileChanged(object sender, FileSystemEventArgs args)
        {
            if (args.ChangeType != WatcherChangeTypes.Changed)
                return;

            if (SelectedProfile == null)
                return;

            lock (SelectedProfile)
            {
                using (var fs = new FileStream(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        SelectedProfile.LuaScript = sr.ReadToEnd();
                    }
                }

                ProfileProvider.AddOrUpdate(SelectedProfile);
                _luaManager.SetupLua(SelectedProfile);
            }
        }

        public void Dispose()
        {
            ProfileViewModel.Dispose();
            _saveTimer?.Stop();
            _saveTimer?.Dispose();
            _watcher?.Dispose();
        }

        #endregion
    }
}