using System.ComponentModel;
using System.Linq;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.LayerEditor
{
    public class LayerDynamicPropertiesViewModel : PropertyChangedBase
    {
        private readonly LayerModel _layer;
        private readonly string _property;
        private LayerDynamicPropertiesModel _layerDynamicPropertiesModelProposed;
        private LayerPropertyType _layerPropertyType;
        private string _name;
        private GeneralHelpers.PropertyCollection _selectedSource;
        private GeneralHelpers.PropertyCollection _selectedTarget;
        private bool _sourcesIsVisible;
        private bool _userSourceIsVisible;

        public LayerDynamicPropertiesViewModel(string property,
            BindableCollection<GeneralHelpers.PropertyCollection> dataModelProps, LayerModel layer)
        {
            _property = property;
            _layer = layer;

            // Look for the existing property model
            LayerDynamicPropertiesModelProposed = new LayerDynamicPropertiesModel();
            var original = _layer.LayerProperties.FirstOrDefault(lp => lp.LayerProperty == _property);
            if (original == null)
            {
                LayerDynamicPropertiesModelProposed.LayerProperty = property;
                LayerDynamicPropertiesModelProposed.LayerPropertyType = LayerPropertyType.None;
            }
            else
            {
                GeneralHelpers.CopyProperties(LayerDynamicPropertiesModelProposed, original);
            }

            Name = property + ":";
            Targets = new BindableCollection<GeneralHelpers.PropertyCollection>();
            Targets.AddRange(dataModelProps.Where(p => p.Type == "Int32"));
            Sources = new BindableCollection<GeneralHelpers.PropertyCollection>();
            Sources.AddRange(dataModelProps.Where(p => p.Type == "Int32"));

            PropertyChanged += OnPropertyChanged;

            SelectedTarget =
                dataModelProps.FirstOrDefault(p => p.Path == LayerDynamicPropertiesModelProposed.GameProperty);
            SelectedSource =
                dataModelProps.FirstOrDefault(p => p.Path == LayerDynamicPropertiesModelProposed.PercentageSource);
            LayerPropertyType = LayerDynamicPropertiesModelProposed.LayerPropertyType;
        }

        public LayerPropertyType LayerPropertyType
        {
            get { return _layerPropertyType; }
            set
            {
                if (value == _layerPropertyType) return;
                _layerPropertyType = value;
                NotifyOfPropertyChange(() => LayerPropertyType);
            }
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

        public LayerDynamicPropertiesModel LayerDynamicPropertiesModelProposed
        {
            get { return _layerDynamicPropertiesModelProposed; }
            set
            {
                if (Equals(value, _layerDynamicPropertiesModelProposed)) return;
                _layerDynamicPropertiesModelProposed = value;
                NotifyOfPropertyChange(() => LayerDynamicPropertiesModelProposed);
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

        public bool SourcesIsVisible
        {
            get { return _sourcesIsVisible; }
            set
            {
                if (value == _sourcesIsVisible) return;
                _sourcesIsVisible = value;
                NotifyOfPropertyChange(() => SourcesIsVisible);
            }
        }

        public bool UserSourceIsVisible
        {
            get { return _userSourceIsVisible; }
            set
            {
                if (value == _userSourceIsVisible) return;
                _userSourceIsVisible = value;
                NotifyOfPropertyChange(() => UserSourceIsVisible);
            }
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTarget")
                LayerDynamicPropertiesModelProposed.GameProperty = SelectedTarget.Path;
            if (e.PropertyName == "SelectedSource")
                LayerDynamicPropertiesModelProposed.PercentageSource = SelectedSource.Path;
            if (e.PropertyName == "LayerPropertyType")
            {
                LayerDynamicPropertiesModelProposed.LayerPropertyType = LayerPropertyType;
                UserSourceIsVisible = LayerPropertyType == LayerPropertyType.PercentageOf;
                SourcesIsVisible = LayerPropertyType == LayerPropertyType.PercentageOfProperty;
            }
        }

        public void Apply()
        {
            var original = _layer.LayerProperties.FirstOrDefault(lp => lp.LayerProperty == _property);
            if (original == null)
                _layer.LayerProperties.Add(LayerDynamicPropertiesModelProposed);
            else
                GeneralHelpers.CopyProperties(original, LayerDynamicPropertiesModelProposed);
        }
    }
}