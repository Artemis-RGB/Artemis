using System;
using System.Linq;
using System.Windows.Media;
using Artemis.Profiles.Layers.Abstract;
using Artemis.Profiles.Layers.Interfaces;
using Artemis.Utilities;
using Artemis.ViewModels;
using Artemis.ViewModels.Profiles;
using Caliburn.Micro;

namespace Artemis.Profiles.Layers.Types.AngularBrush
{
    public class AngularBrushPropertiesViewModel : LayerPropertiesViewModel
    {
        #region Properties & Fields

        private ILayerAnimation _selectedLayerAnimation;

        public BindableCollection<GeneralHelpers.PropertyCollection> DataModelProps { get; set; }
        public BindableCollection<ILayerAnimation> LayerAnimations { get; set; }
        public LayerDynamicPropertiesViewModel HeightProperties { get; set; }
        public LayerDynamicPropertiesViewModel WidthProperties { get; set; }
        public LayerDynamicPropertiesViewModel OpacityProperties { get; set; }
        public LayerTweenViewModel LayerTweenViewModel { get; set; }

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

        #endregion

        #region Constructors

        public AngularBrushPropertiesViewModel(LayerEditorViewModel editorVm)
            : base(editorVm)
        {
            LayerAnimations = new BindableCollection<ILayerAnimation>(editorVm.LayerAnimations);

            HeightProperties = new LayerDynamicPropertiesViewModel("Height", editorVm);
            WidthProperties = new LayerDynamicPropertiesViewModel("Width", editorVm);
            OpacityProperties = new LayerDynamicPropertiesViewModel("Opacity", editorVm);
            LayerTweenViewModel = new LayerTweenViewModel(editorVm);

            SelectedLayerAnimation =
                LayerAnimations.FirstOrDefault(l => l.Name == editorVm.ProposedLayer.LayerAnimation?.Name) ??
                LayerAnimations.First(l => l.Name == "None");
        }

        #endregion

        #region Methods

        public override void ApplyProperties()
        {
            HeightProperties.Apply(LayerModel);
            WidthProperties.Apply(LayerModel);
            OpacityProperties.Apply(LayerModel);

            ((AngularBrushPropertiesModel)LayerModel.Properties).GradientStops = GetGradientStops().Select(x => new Tuple<double, Color>(x.Offset, x.Color)).ToList();

            LayerModel.LayerAnimation = SelectedLayerAnimation;
        }

        private GradientStopCollection GetGradientStops()
        {
            LinearGradientBrush linearBrush = Brush as LinearGradientBrush;
            if (linearBrush != null)
                return linearBrush.GradientStops;

            RadialGradientBrush radialBrush = Brush as RadialGradientBrush;
            if (radialBrush != null)
                return radialBrush.GradientStops;

            SolidColorBrush solidBrush = Brush as SolidColorBrush;
            if (solidBrush != null)
                return new GradientStopCollection(new[] { new GradientStop(solidBrush.Color, 0), new GradientStop(solidBrush.Color, 1) });

            return null;
        }

        #endregion
    }
}
