using System.ComponentModel;
using System.IO;
using System.Linq;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Models.Profiles.Properties;
using Artemis.Services;
using Artemis.Utilities;
using Artemis.ViewModels.LayerEditor.Properties;
using Caliburn.Micro;
using Ninject;

namespace Artemis.ViewModels.LayerEditor
{
    public sealed class LayerEditorViewModel : Screen
    {
        private readonly IGameDataModel _gameDataModel;
        private LayerModel _layer;
        private LayerPropertiesViewModel _layerPropertiesViewModel;
        private LayerType _layerType;
        private LayerModel _proposedLayer;
        private LayerPropertiesModel _proposedProperties;

        public LayerEditorViewModel(IGameDataModel gameDataModel, LayerModel layer)
        {
            _gameDataModel = gameDataModel;

            Layer = layer;
            if (Layer.Properties == null)
                Layer.SetupProperties();

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            DataModelProps.AddRange(GeneralHelpers.GenerateTypeMap(gameDataModel));
            LayerConditionVms = new BindableCollection<LayerConditionViewModel>(layer.Properties.Conditions
                .Select(c => new LayerConditionViewModel(this, c, DataModelProps)));

            PropertyChanged += PropertiesViewModelHandler;
            PreSelect();
        }

        [Inject]
        public MetroDialogService DialogService { get; set; }

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }

        public BindableCollection<string> LayerTypes => new BindableCollection<string>();

        public BindableCollection<LayerConditionViewModel> LayerConditionVms { get; set; }

        public LayerModel Layer
        {
            get { return _layer; }
            set
            {
                if (Equals(value, _layer)) return;
                _layer = value;
                NotifyOfPropertyChange(() => Layer);
            }
        }

        public LayerModel ProposedLayer
        {
            get { return _proposedLayer; }
            set
            {
                if (Equals(value, _proposedLayer)) return;
                _proposedLayer = value;
                NotifyOfPropertyChange(() => ProposedLayer);
            }
        }

        public LayerPropertiesModel ProposedProperties
        {
            get { return _proposedProperties; }
            set
            {
                if (Equals(value, _proposedProperties)) return;
                _proposedProperties = value;
                NotifyOfPropertyChange(() => ProposedProperties);
            }
        }

        public LayerType LayerType
        {
            get { return _layerType; }
            set
            {
                if (value == _layerType) return;
                _layerType = value;
                NotifyOfPropertyChange(() => LayerType);
            }
        }

        public LayerPropertiesViewModel LayerPropertiesViewModel
        {
            get { return _layerPropertiesViewModel; }
            set
            {
                if (Equals(value, _layerPropertiesViewModel)) return;
                _layerPropertiesViewModel = value;
                NotifyOfPropertyChange(() => LayerPropertiesViewModel);
            }
        }

        public bool KeyboardGridIsVisible => Layer.LayerType == LayerType.Keyboard;
        public bool GifGridIsVisible => Layer.LayerType == LayerType.KeyboardGif;

        public void PreSelect()
        {
            LayerType = Layer.LayerType;

            if (LayerType == LayerType.Folder && !(LayerPropertiesViewModel is FolderPropertiesViewModel))
                LayerPropertiesViewModel = new FolderPropertiesViewModel(_gameDataModel, Layer.Properties);

            ProposedProperties = GeneralHelpers.Clone(Layer.Properties);
        }

        private void PropertiesViewModelHandler(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "LayerType")
                return;

            // Update the model
            if (Layer.LayerType != LayerType)
            {
                Layer.LayerType = LayerType;
                Layer.SetupProperties();
            }

            // Update the KeyboardPropertiesViewModel if it's being used
            var model = LayerPropertiesViewModel as KeyboardPropertiesViewModel;
            if (model != null)
                model.IsGif = LayerType == LayerType.KeyboardGif;

            // Apply the proper PropertiesViewModel
            if ((LayerType == LayerType.Keyboard || LayerType == LayerType.KeyboardGif) &&
                !(LayerPropertiesViewModel is KeyboardPropertiesViewModel))
            {
                LayerPropertiesViewModel = new KeyboardPropertiesViewModel(_gameDataModel, Layer.Properties)
                {
                    IsGif = LayerType == LayerType.KeyboardGif
                };
            }
            else if (LayerType == LayerType.Mouse && !(LayerPropertiesViewModel is MousePropertiesViewModel))
                LayerPropertiesViewModel = new MousePropertiesViewModel(_gameDataModel, Layer.Properties);
            else if (LayerType == LayerType.Headset && !(LayerPropertiesViewModel is HeadsetPropertiesViewModel))
                LayerPropertiesViewModel = new HeadsetPropertiesViewModel(_gameDataModel, Layer.Properties);
            else if (LayerType == LayerType.Folder && !(LayerPropertiesViewModel is FolderPropertiesViewModel))
                LayerPropertiesViewModel = new FolderPropertiesViewModel(_gameDataModel, Layer.Properties);

            NotifyOfPropertyChange(() => LayerPropertiesViewModel);
        }

        public void AddCondition()
        {
            var condition = new LayerConditionModel();
            LayerConditionVms.Add(new LayerConditionViewModel(this, condition, DataModelProps));
        }

        public void Apply()
        {
            if (LayerPropertiesViewModel != null)
                Layer.Properties = LayerPropertiesViewModel.GetAppliedProperties();
            Layer.Properties.Conditions.Clear();
            foreach (var conditionViewModel in LayerConditionVms)
                Layer.Properties.Conditions.Add(conditionViewModel.LayerConditionModel);

            if (Layer.LayerType != LayerType.KeyboardGif)
                return; // Don't bother checking for a GIF path unless the type is GIF
            if (!File.Exists(((KeyboardPropertiesModel) Layer.Properties).GifFile))
                DialogService.ShowErrorMessageBox("Couldn't find or access the provided GIF file.");
        }

        public void DeleteCondition(LayerConditionViewModel layerConditionViewModel,
            LayerConditionModel layerConditionModel)
        {
            LayerConditionVms.Remove(layerConditionViewModel);
            Layer.Properties.Conditions.Remove(layerConditionModel);
        }
    }
}