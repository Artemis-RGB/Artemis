using System.Collections.Generic;
using System.Linq;
using Artemis.Models.Interfaces;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.Profiles.Layers.Types.Headset
{
    public class HeadsetPropertiesViewModel : LayerPropertiesViewModel
    {
        private ILayerAnimation _selectedLayerAnimation;

        public HeadsetPropertiesViewModel(LayerModel layerModel, IDataModel dataModel,
            IEnumerable<ILayerAnimation> layerAnimations) : base(layerModel, dataModel)
        {
            LayerAnimations = new BindableCollection<ILayerAnimation>(layerAnimations);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity",
                new BindableCollection<GeneralHelpers.PropertyCollection>(GeneralHelpers.GenerateTypeMap(dataModel)),
                layerModel.Properties);

            SelectedLayerAnimation = LayerAnimations.FirstOrDefault(l => l.Name == layerModel.LayerAnimation?.Name) ??
                                     LayerAnimations.First(l => l.Name == "None");
        }

        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }
        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }

        public ILayerAnimation SelectedLayerAnimation
        {
            get { return _selectedLayerAnimation; }
            set
            {
                if (Equals(value, _selectedLayerAnimation)) return;
                _selectedLayerAnimation = value;
                NotifyOfPropertyChange(() => SelectedLayerAnimation);
            }
        }

        public override void ApplyProperties()
        {
            OpacityProperties.Apply(LayerModel);
            LayerModel.Properties.Brush = Brush;
            LayerModel.LayerAnimation = SelectedLayerAnimation;
        }
    }
}