using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Modules.Games.RocketLeague;
using Artemis.Modules.Games.TheDivision;
using Caliburn.Micro;
using Color = System.Drawing.Color;

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
            if (ProfileProvider.GetAll().Contains(profile))
            {
                var overwrite =
                    await
                        _mainManager.DialogService.ShowQuestionMessageBox("Overwrite existing profile",
                            "A profile with this name already exists for this game. Would you like to overwrite it?");
                if (!overwrite.Value)
                    return;
            }

            // Test
            profile.Layers = new List<LayerModel>();
            var layerFolder = new LayerModel("[VM TEST] Folder 1", LayerType.Folder);

            var layer1 = new LayerModel("[VM TEST] Rectangle 1", LayerType.Rectangle);
            layer1.LayerConditions.Add(new LayerConditionModel {Field = "Boost", Operator = ">", Value = "0"});
            layer1.LayerProperties.Add(new LayerDynamicPropertiesModel
            {
                LayerProperty = "Width",
                LayerPopertyType = LayerPopertyType.PercentageOf,
                GameProperty = "Boost",
                PercentageSource = "100"
            });
            layer1.LayerUserProperties = new LayerPropertiesModel
            {
                Colors = new List<Color> {Color.Red, Color.OrangeRed},
                ContainedBrush = true,
                GradientMode = LinearGradientMode.Vertical,
                Width = 21,
                Height = 7,
                Opacity = 100,
                Rotate = true,
                RotateSpeed = 1,
                X = 0,
                Y = 0
            };
            layerFolder.Children.Add(layer1);
            layerFolder.Children.Add(new LayerModel("[VM TEST] Ellipse 1", LayerType.Ellipse));

            var testData = new RocketLeagueDataModel {Boost = 20};
            var bitmap = _mainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(4);
            using (var g = Graphics.FromImage(bitmap))
            {
                layerFolder.Draw<RocketLeagueDataModel>(testData, g);
            }
            // End test

            profile.Layers.Add(layerFolder);
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