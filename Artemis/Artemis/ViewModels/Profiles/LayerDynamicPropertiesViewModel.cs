using System.ComponentModel;
using System.Linq;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities;
using Caliburn.Micro;
using Castle.Core.Internal;
using NClone;

namespace Artemis.ViewModels.Profiles
{
    public sealed class LayerDynamicPropertiesViewModel : PropertyChangedBase
    {
        private readonly string _property;
        private BindableCollection<LayerPropertyOptions> _layerPropertyOptions;
        private LayerPropertyType _layerPropertyType;
        private string _name;
        private DynamicPropertiesModel _proposed;
        private GeneralHelpers.PropertyCollection _selectedSource;
        private GeneralHelpers.PropertyCollection _selectedTarget;
        private bool _sourcesIsVisible;
        private bool _userSourceIsVisible;

        public LayerDynamicPropertiesViewModel(string property,
            BindableCollection<GeneralHelpers.PropertyCollection> dataModelProps,
            LayerPropertiesModel layerPropertiesModel)
        {
            _property = property;

            // Look for the existing property model
            Proposed = new DynamicPropertiesModel();
            var original = layerPropertiesModel.DynamicProperties.FirstOrDefault(lp => lp.LayerProperty == _property);
            if (original == null)
            {
                Proposed.LayerProperty = property;
                Proposed.LayerPropertyType = LayerPropertyType.PercentageOf;
            }
            else
                Proposed = Clone.ObjectGraph(original);

            PropertyChanged += OnPropertyChanged;
            SetupControls(dataModelProps);
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
                NotifyOfPropertyChange(() => ControlsEnabled);
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

        public BindableCollection<LayerPropertyOptions> LayerPropertyOptions
        {
            get { return _layerPropertyOptions; }
            set
            {
                if (Equals(value, _layerPropertyOptions)) return;
                _layerPropertyOptions = value;
                NotifyOfPropertyChange(() => LayerPropertyOptions);
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

        public bool ControlsEnabled => SelectedTarget.Display != "None" && SelectedTarget.Path != null;

        public BindableCollection<GeneralHelpers.PropertyCollection> Targets { get; set; }

        public BindableCollection<GeneralHelpers.PropertyCollection> Sources { get; set; }

        private void SetupControls(BindableCollection<GeneralHelpers.PropertyCollection> dataModelProps)
        {
            Name = _property + ":";

            // Populate target combobox
            Targets = new BindableCollection<GeneralHelpers.PropertyCollection>
            {
                new GeneralHelpers.PropertyCollection {Display = "None"}
            };
            Targets.AddRange(dataModelProps.Where(p => p.Type == "Int32"));

            // Populate sources combobox
            Sources = new BindableCollection<GeneralHelpers.PropertyCollection>();
            Sources.AddRange(dataModelProps.Where(p => p.Type == "Int32"));

            // Preselect according to the model
            SelectedTarget = dataModelProps.FirstOrDefault(p => p.Path == Proposed.GameProperty);
            SelectedSource = dataModelProps.FirstOrDefault(p => p.Path == Proposed.PercentageProperty);
            LayerPropertyType = Proposed.LayerPropertyType;

            // Populate the extra options combobox
            switch (_property)
            {
                case "Width":
                    LayerPropertyOptions = new BindableCollection<LayerPropertyOptions>
                    {
                        Artemis.Profiles.Layers.Models.LayerPropertyOptions.LeftToRight,
                        Artemis.Profiles.Layers.Models.LayerPropertyOptions.RightToLeft
                    };
                    break;
                case "Height":
                    LayerPropertyOptions = new BindableCollection<LayerPropertyOptions>
                    {
                        Artemis.Profiles.Layers.Models.LayerPropertyOptions.Downwards,
                        Artemis.Profiles.Layers.Models.LayerPropertyOptions.Upwards
                    };
                    break;
                case "Opacity":
                    LayerPropertyOptions = new BindableCollection<LayerPropertyOptions>
                    {
                        Artemis.Profiles.Layers.Models.LayerPropertyOptions.Increase,
                        Artemis.Profiles.Layers.Models.LayerPropertyOptions.Decrease
                    };
                    break;
            }

            UserSourceIsVisible = LayerPropertyType == LayerPropertyType.PercentageOf;
            SourcesIsVisible = LayerPropertyType == LayerPropertyType.PercentageOfProperty;

            // Set up a default for SelectedTarget if it was null
            if (SelectedTarget.Display == null)
                SelectedTarget = Targets.FirstOrDefault();
            // Set up a default for the extra option if it fell outside the range
            if (!LayerPropertyOptions.Contains(Proposed.LayerPropertyOptions))
                Proposed.LayerPropertyOptions = LayerPropertyOptions.FirstOrDefault();
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "SelectedTarget")
                Proposed.GameProperty = SelectedTarget.Path;
            else if (e.PropertyName == "SelectedSource")
                Proposed.PercentageProperty = SelectedSource.Path;
            else if (e.PropertyName == "LayerPropertyType")
            {
                Proposed.LayerPropertyType = LayerPropertyType;
                UserSourceIsVisible = LayerPropertyType == LayerPropertyType.PercentageOf;
                SourcesIsVisible = LayerPropertyType == LayerPropertyType.PercentageOfProperty;
            }
        }

        public void Apply(LayerModel layerModel)
        {
            var original = layerModel.Properties.DynamicProperties.FirstOrDefault(lp => lp.LayerProperty == _property);
            if (original != null)
                layerModel.Properties.DynamicProperties.Remove(original);

            if (!Proposed.GameProperty.IsNullOrEmpty())
                layerModel.Properties.DynamicProperties.Add(Proposed);
        }
    }
}