using System.Collections.Generic;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.KeyboardProviders;
using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class ProfileEditorViewModel : Screen
    {
        private readonly GameModel _gameModel;
        private readonly KeyboardProvider _keyboard;
        private List<ProfileModel> _profiles;

        public ProfileEditorViewModel(GameModel gameModel, KeyboardProvider keyboard)
        {
            _gameModel = gameModel;
            _keyboard = keyboard;

            GetProfiles();
        }

        public void GetProfiles()
        {
            _profiles = ProfileProvider.GetAll(_gameModel);
        }

        private ImageSource GenerateKeyboardImage()
        {
            return null;
        }
    }
}