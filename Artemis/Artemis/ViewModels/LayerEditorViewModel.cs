using System.Linq;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Artemis.ViewModels.LayerEditor;
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
            DataModelProps.AddRange(GeneralHelpers.GenerateTypeMap<T>());

            LayerConditionVms =
                new BindableCollection<LayerConditionViewModel<T>>(
                    layer.LayerConditions.Select(c => new LayerConditionViewModel<T>(this, c, DataModelProps)));
            
            ProposedProperties = new LayerPropertiesModel();
            GeneralHelpers.CopyProperties(ProposedProperties, Layer.LayerUserProperties);
        }

        public BindableCollection<LayerConditionViewModel<T>> LayerConditionVms { get; set; }

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

        public void AddCondition()
        {
            var condition = new LayerConditionModel();
            Layer.LayerConditions.Add(condition);
            LayerConditionVms.Add(new LayerConditionViewModel<T>(this, condition, DataModelProps));
        }

        public void Apply()
        {
            GeneralHelpers.CopyProperties(Layer.LayerUserProperties, ProposedProperties);
        }

        public void DeleteCondition(LayerConditionViewModel<T> layerConditionViewModel, LayerConditionModel layerConditionModel)
        {
            LayerConditionVms.Remove(layerConditionViewModel);
            Layer.LayerConditions.Remove(layerConditionModel);
        }
    }
}