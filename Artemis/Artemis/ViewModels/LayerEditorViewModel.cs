using System;
using System.Collections;
using System.Diagnostics;
using System.Reflection;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels
{
    public class LayerEditorViewModel<T> : Screen
    {
        private LayerModel _layer;

        public LayerEditorViewModel(LayerModel layer)
        {
            Layer = layer;

            DataModelProps = new BindableCollection<GeneralHelpers.PropertyCollection>();
            DataModelProps.AddRange(GeneralHelpers.GetPropertyMap((T)Activator.CreateInstance(typeof(T), new object[] { })));
            ProposedProperties = new LayerPropertiesModel();
            GeneralHelpers.CopyProperties(ProposedProperties, Layer.LayerUserProperties);
        }

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

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }

        public LayerPropertiesModel ProposedProperties { get; set; }

        public void Apply()
        {
            GeneralHelpers.CopyProperties(Layer.LayerUserProperties, ProposedProperties);
        }
    }
}