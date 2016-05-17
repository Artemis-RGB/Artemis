using System.ComponentModel;
using System.Linq;
using Artemis.Models.Profiles.Properties;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.LayerEditor
{
    public sealed class LayerDynamicPropertiesViewModel : PropertyChangedBase
    {
        private readonly string _property;
        private DynamicPropertiesModel _proposed;
        private LayerPropertyType _layerPropertyType;
        private string _name;
        private GeneralHelpers.PropertyCollection _selectedSource;
        private GeneralHelpers.PropertyCollection _selectedTarget;
        private bool _sourcesIsVisible;
        private bool _userSourceIsVisible;

        public LayerDynamicPropertiesViewModel(string property, BindableCollection<GeneralHelpers.PropertyCollection> dataModelProps, KeyboardPropertiesModel keyboardProperties)
        {
            _property = property;

            // Look for the existing property model
            Proposed = new DynamicPropertiesModel();
            var original = keyboardProperties.DynamicProperties.FirstOrDefault(lp => lp.LayerProperty == _property);
            if (original == null)
            {
                Proposed.LayerProperty = property;
                Proposed.LayerPropertyType = LayerPropertyType.None;
            }
            else
                GeneralHelpers.CopyProperties(Proposed, original);

            Name = property + ":";
            Targets = new BindableCollection<GeneralHelpers.PropertyCollection>();
            Targets.AddRange(dataModelProps.Where(p => p.Type == "Int32"));
            Sources = new BindableCollection<GeneralHelpers.PropertyCollection>();
            Sources.AddRange(dataModelProps.Where(p => p.Type == "Int32"));

            PropertyChanged += OnPropertyChanged;

            SelectedTarget = dataModelProps.FirstOrDefault(p => p.Path == Proposed.GameProperty);
            SelectedSource = dataModelProps.FirstOrDefault(p => p.Path == Proposed.PercentageProperty);
            LayerPropertyType = Proposed.LayerPropertyType;
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

        public DynamicPropertiesModel Proposed
        {
            get { return _proposed; }
            set
            {
                if (Equals(value, _proposed)) return;
                _proposed = value;
                NotifyOfPropertyChange(() => Proposed);
            }
        }

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

        public BindableCollection<GeneralHelpers.PropertyCollection> Targets { get; set; }

        public BindableCollection<GeneralHelpers.PropertyCollection> Sources { get; set; }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTarget")
                Proposed.GameProperty = SelectedTarget.Path;
            if (e.PropertyName == "SelectedSource")
                Proposed.PercentageProperty = SelectedSource.Path;
            if (e.PropertyName == "LayerPropertyType")
            {
                Proposed.LayerPropertyType = LayerPropertyType;
                UserSourceIsVisible = LayerPropertyType == LayerPropertyType.PercentageOf;
                SourcesIsVisible = LayerPropertyType == LayerPropertyType.PercentageOfProperty;
            }
        }

        public void Apply(KeyboardPropertiesModel keyboardProperties)
        {
            var original = keyboardProperties.DynamicProperties.FirstOrDefault(lp => lp.LayerProperty == _property);
            if (original != null)
                keyboardProperties.DynamicProperties.Remove(original);

            keyboardProperties.DynamicProperties.Add(Proposed);
        }
    }
}