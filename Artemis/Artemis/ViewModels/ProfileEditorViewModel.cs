using System.Linq;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class ProfileEditorViewModel : Screen
    {
        private readonly GameModel _gameModel;
        private readonly MainManager _mainManager;
        private BindableCollection<ProfileModel> _profileModels;
        private ProfileModel _selectedProfileModel;

        public ProfileEditorViewModel(MainManager mainManager, GameModel gameModel)
        {
            _mainManager = mainManager;
            _gameModel = gameModel;

            ProfileModels = new BindableCollection<ProfileModel>();
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
                NotifyOfPropertyChange();
            }
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

            var profile = new ProfileModel(name, _mainManager.KeyboardManager.ActiveKeyboard.Name, _gameModel.Name);
            var test = ProfileProvider.GetAll();
            if (test.Contains(profile))
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

        private ImageSource GenerateKeyboardImage()
        {
            return null;
        }
    }
}