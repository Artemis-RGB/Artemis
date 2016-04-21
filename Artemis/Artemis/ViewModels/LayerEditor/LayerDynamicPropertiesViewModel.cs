using System.ComponentModel;
using System.Linq;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.LayerEditor
{
    public class LayerDynamicPropertiesViewModel : Screen
    {
        private LayerDynamicPropertiesModel _layerDynamicPropertiesModel;
        private string _name;
        private GeneralHelpers.PropertyCollection _selectedSource;
        private GeneralHelpers.PropertyCollection _selectedTarget;

        public LayerDynamicPropertiesViewModel(string property,
            BindableCollection<GeneralHelpers.PropertyCollection> dataModelProps, LayerModel layer)
        {
            // Look for the existing property model
            LayerDynamicPropertiesModel = layer.LayerProperties.FirstOrDefault(lp => lp.LayerProperty == property);
            if (LayerDynamicPropertiesModel == null)
            {
                // If it doesn't exist, create a new one
                LayerDynamicPropertiesModel = new LayerDynamicPropertiesModel
                {
                    LayerProperty = property,
                    LayerPropertyType = LayerPropertyType.None
                };
                // Add it to the layer
                layer.LayerProperties.Add(LayerDynamicPropertiesModel);
            }

            Name = property + ":";
            Targets = new BindableCollection<GeneralHelpers.PropertyCollection>();
            Targets.AddRange(dataModelProps.Where(p => p.Type == "Int32"));
            Sources = new BindableCollection<GeneralHelpers.PropertyCollection>();
            Sources.AddRange(dataModelProps.Where(p => p.Type == "Int32"));

            PropertyChanged += OnPropertyChanged;
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (value == _name) return;
                _name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public LayerDynamicPropertiesModel LayerDynamicPropertiesModel
        {
            get { return _layerDynamicPropertiesModel; }
            set
            {
                if (Equals(value, _layerDynamicPropertiesModel)) return;
                _layerDynamicPropertiesModel = value;
                NotifyOfPropertyChange(() => LayerDynamicPropertiesModel);
            }
        }

        public BindableCollection<GeneralHelpers.PropertyCollection> Targets { get; set; }

        public GeneralHelpers.PropertyCollection SelectedTarget
        {
            get { return _selectedTarget; }
            set
            {
                if (value.Equals(_selectedTarget)) return;
                _selectedTarget = value;
                NotifyOfPropertyChange(() => SelectedTarget);
            }
        }

        public BindableCollection<GeneralHelpers.PropertyCollection> Sources { get; set; }

        public GeneralHelpers.PropertyCollection SelectedSource
        {
            get { return _selectedSource; }
            set
            {
                if (value.Equals(_selectedSource)) return;
                _selectedSource = value;
                NotifyOfPropertyChange(() => SelectedSource);
            }
        }

        /// <summary>
        ///     Updates the underlying model
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTarget")
                LayerDynamicPropertiesModel.GameProperty = SelectedTarget.Path;
            if (e.PropertyName == "SelectedSource")
                LayerDynamicPropertiesModel.PercentageSource = SelectedSource.Path;
        }
    }
}