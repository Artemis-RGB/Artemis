using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Profiles;
using Artemis.Profiles.Layers.Models;
using Artemis.Profiles.Layers.Types.Folder;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Utilities;
using Artemis.ViewModels;
using Ninject.Parameters;

namespace Artemis.Models
{
    public class ProfileEditorModel : IDisposable
    {
        private readonly DeviceManager _deviceManager;
        private readonly LuaManager _luaManager;
        private readonly DialogService _dialogService;
        private readonly WindowService _windowService;
        private FileSystemWatcher _watcher;
        private ProfileModel _luaProfile;

        public ProfileEditorModel(WindowService windowService, MetroDialogService dialogService,
            DeviceManager deviceManager, LuaManager luaManager)
        {
            _windowService = windowService;
            _dialogService = dialogService;
            _deviceManager = deviceManager;
            _luaManager = luaManager;
        }

        #region Layers

        /// <summary>
        ///     Opens a new LayerEditorView for the given layer
        /// </summary>
        /// <param name="layer">The layer to open the view for</param>
        /// <param name="dataModel">The datamodel to bind the editor to</param>
        public void EditLayer(LayerModel layer, ModuleDataModel dataModel)
        {
            IParameter[] args =
            {
                new ConstructorArgument("dataModel", dataModel),
                new ConstructorArgument("layer", layer)
            };
            _windowService.ShowDialog<LayerEditorViewModel>(args);

            // If the layer was a folder, but isn't anymore, assign it's children to it's parent.
            if (layer.LayerType is FolderType || !layer.Children.Any())
                return;

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

        /// <summary>
        ///     Removes the given layer from the profile
        /// </summary>
        /// <param name="layer">The layer to remove</param>
        /// <param name="profileModel">The profile to remove it from</param>
        public void RemoveLayer(LayerModel layer, ProfileModel profileModel)
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
            if (profileModel.Layers.Contains(layer))
                profileModel.Layers.Remove(layer);
        }

        #endregion

        #region Profiles

        public async Task<ProfileModel> AddProfile(ModuleModel moduleModel)
        {
            if (_deviceManager.ActiveKeyboard == null)
            {
                _dialogService.ShowMessageBox("Cannot add profile.",
                    "To add a profile, please select a keyboard in the options menu first.");
                return null;
            }

            var name = await GetValidProfileName("Name profile", "Please enter a unique name for your new profile");
            // User cancelled
            if (name == null)
                return null;

            var profile = new ProfileModel
            {
                Name = name,
                KeyboardSlug = _deviceManager.ActiveKeyboard.Slug,
                Width = _deviceManager.ActiveKeyboard.Width,
                Height = _deviceManager.ActiveKeyboard.Height,
                GameName = moduleModel.Name
            };

            if (!ProfileProvider.IsProfileUnique(profile))
            {
                var overwrite = await _dialogService.ShowQuestionMessageBox("Overwrite existing profile",
                    "A profile with this name already exists for this game. Would you like to overwrite it?");
                if (!overwrite.Value)
                    return null;
            }

            ProfileProvider.AddOrUpdate(profile);
            return profile;
        }

        public async Task RenameProfile(ProfileModel profileModel)
        {
            var name = await GetValidProfileName("Rename profile", "Please enter a unique new profile name");
            // User cancelled
            if (name == null)
                return;
            var doRename = await MakeProfileUnique(profileModel, name, profileModel.Name);
            if (!doRename)
                return;

            ProfileProvider.RenameProfile(profileModel, profileModel.Name);
        }

        public async Task<ProfileModel> DuplicateProfile(ProfileModel selectedProfile)
        {
            var newProfile = GeneralHelpers.Clone(selectedProfile);
            var name = await GetValidProfileName("Duplicate profile", "Please enter a unique new profile name");
            // User cancelled
            if (name == null)
                return null;
            var doRename = await MakeProfileUnique(newProfile, name, newProfile.Name);
            if (!doRename)
                return null;

            // Make sure it's not default, in case of copying a default profile
            newProfile.IsDefault = false;
            ProfileProvider.AddOrUpdate(newProfile);

            return newProfile;
        }

        public async Task<bool> DeleteProfile(ProfileModel selectedProfile, ModuleModel moduleModel)
        {
            var confirm = await _dialogService.ShowQuestionMessageBox("Delete profile",
                $"Are you sure you want to delete the profile named: {selectedProfile.Name}?\n\n" +
                "This cannot be undone.");
            if (!confirm.Value)
                return false;

            var defaultProfile = ProfileProvider.GetProfile(_deviceManager.ActiveKeyboard, moduleModel, "Default");
            var deleteProfile = selectedProfile;

            moduleModel.ChangeProfile(defaultProfile);
            ProfileProvider.DeleteProfile(deleteProfile);

            return true;
        }

        public async Task<ProfileModel> ImportProfile(ModuleModel moduleModel)
        {
            var dialog = new OpenFileDialog {Filter = "Artemis profile (*.json)|*.json"};
            var result = dialog.ShowDialog();
            if (result != DialogResult.OK)
                return null;

            var profileModel = ProfileProvider.LoadProfileIfValid(dialog.FileName);
            if (profileModel == null)
            {
                _dialogService.ShowErrorMessageBox("Oh noes, the profile you provided is invalid. " +
                                                   "If this keeps happening, please make an issue on GitHub and provide the profile.");
                return null;
            }

            // Verify the game
            if (profileModel.GameName != moduleModel.Name)
            {
                _dialogService.ShowErrorMessageBox(
                    $"Oh oops! This profile is ment for {profileModel.GameName}, not {moduleModel.Name} :c");
                return null;
            }

            // Verify the keyboard
            var deviceManager = _deviceManager;
            if (profileModel.KeyboardSlug != deviceManager.ActiveKeyboard.Slug)
            {
                var adjustKeyboard = await _dialogService.ShowQuestionMessageBox("Profile not made for this keyboard",
                    $"Watch out, this profile wasn't ment for this keyboard, but for the {profileModel.KeyboardSlug}. " +
                    "You can still import it but you'll probably have to do some adjusting\n\n" +
                    "Continue?");
                if (!adjustKeyboard.Value)
                    return null;

                // Resize layers that are on the full keyboard width
                profileModel.ResizeLayers(deviceManager.ActiveKeyboard);
                // Put layers back into the canvas if they fell outside it
                profileModel.FixBoundaries(deviceManager.ActiveKeyboard.KeyboardRectangle(1));

                // Setup profile metadata to match the new keyboard
                profileModel.KeyboardSlug = deviceManager.ActiveKeyboard.Slug;
                profileModel.Width = deviceManager.ActiveKeyboard.Width;
                profileModel.Height = deviceManager.ActiveKeyboard.Height;
            }

            var name = await GetValidProfileName("Rename profile", "Please enter a unique new profile name");
            // User cancelled
            if (name == null)
                return null;
            var doRename = await MakeProfileUnique(profileModel, name, profileModel.Name);
            if (!doRename)
                return null;

            profileModel.IsDefault = false;
            ProfileProvider.AddOrUpdate(profileModel);
            return profileModel;
        }

        public void ChangeProfileByName(ModuleModel moduleModel, string profileName)
        {
            if (string.IsNullOrEmpty(profileName))
                profileName = "Default";

            moduleModel.ChangeProfile(ProfileProvider.GetProfile(_deviceManager.ActiveKeyboard, moduleModel, profileName));
        }

        private async Task<string> GetValidProfileName(string title, string text)
        {
            var name = await _dialogService.ShowInputDialog(title, text);

            // Null when the user cancelled
            if (name == null)
                return null;

            if (name.Length >= 2)
                return name;

            _dialogService.ShowMessageBox("Invalid profile name",
                "Please provide a valid profile name that's longer than 2 symbols");

            return await GetValidProfileName(title, text);
        }

        private async Task<bool> MakeProfileUnique(ProfileModel profileModel, string name, string oldName)
        {
            profileModel.Name = name;
            if (ProfileProvider.IsProfileUnique(profileModel))
                return true;

            name = await GetValidProfileName("Rename profile", "Please enter a unique new profile name");
            if (name != null)
                return await MakeProfileUnique(profileModel, name, oldName);

            // If cancelled, restore old name and stop
            profileModel.Name = oldName;
            return false;
        }

        #endregion

        #region LUA

        public void OpenLuaEditor(ProfileModel profileModel)
        {
            // Clean up old environment
            DisposeLuaWatcher();

            // Create a temp file
            var fileName = Guid.NewGuid() + ".lua";
            var file = File.Create(Path.GetTempPath() + fileName);
            file.Dispose();

            // Add instructions to LUA script if it's a new file
            if (string.IsNullOrEmpty(profileModel.LuaScript))
                profileModel.LuaScript = Encoding.UTF8.GetString(Resources.lua_placeholder);
            File.WriteAllText(Path.GetTempPath() + fileName, profileModel.LuaScript);

            // Watch the file for changes
            _luaProfile = profileModel;
            _watcher = new FileSystemWatcher(Path.GetTempPath(), fileName);
            _watcher.Changed += LuaFileChanged;
            _watcher.EnableRaisingEvents = true;
            _watcher.Path = Path.GetTempPath();
            _watcher.Filter = fileName;

            // Open the temp file with the default editor
            System.Diagnostics.Process.Start(Path.GetTempPath() + fileName);
        }

        private void LuaFileChanged(object sender, FileSystemEventArgs args)
        {
            if (_luaProfile == null)
            {
                DisposeLuaWatcher();
                return;
            }

            if (args.ChangeType != WatcherChangeTypes.Changed)
                return;

            lock (_luaProfile)
            {
                using (var fs = new FileStream(args.FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var sr = new StreamReader(fs))
                    {
                        _luaProfile.LuaScript = sr.ReadToEnd();
                    }
                }

                ProfileProvider.AddOrUpdate(_luaProfile);
                _luaManager.SetupLua(_luaProfile);
            }
        }

        private void DisposeLuaWatcher()
        {
            if (_watcher == null) return;
            _watcher.Changed -= LuaFileChanged;
            _watcher.Dispose();
            _watcher = null;
        }

        public void Dispose()
        {
            DisposeLuaWatcher();
        }

        #endregion

        #region Rendering

        

        #endregion
    }
}