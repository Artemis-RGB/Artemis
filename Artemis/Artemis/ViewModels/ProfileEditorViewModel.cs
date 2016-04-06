using System.Dynamic;
using System.Linq;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class ProfileEditorViewModel<T> : Screen
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

        public LayerModel SelectedLayer { get; set; }

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
            _editorVm = new LayerEditorViewModel<T>(SelectedProfileModel, layer);
            dynamic settings = new ExpandoObject();

            settings.Title = "Artemis | Edit " + layer.Name;
            manager.ShowDialog(_editorVm, null, settings);
        }

        public void SetSelectedLayer(LayerModel layer)
        {
            SelectedLayer = layer;
        }

        public void AddLayer()
        {
            _selectedProfileModel.Layers.Add(new LayerModel
            {
                Name = "Layer " + (_selectedProfileModel.Layers.Count + 1),
                LayerType = LayerType.KeyboardRectangle
            });
            NotifyOfPropertyChange();
        }

        private ImageSource GenerateKeyboardImage()
        {
            return null;
        }
    }
}