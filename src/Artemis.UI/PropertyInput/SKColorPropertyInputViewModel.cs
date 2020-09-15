using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using SkiaSharp;

namespace Artemis.UI.PropertyInput
{
    public class SKColorPropertyInputViewModel : PropertyInputViewModel<SKColor>
    {
        private readonly DataBindingRegistration<SKColor, SKColor> _registration;

        public SKColorPropertyInputViewModel(LayerProperty<SKColor> layerProperty, IProfileEditorService profileEditorService) : base(layerProperty, profileEditorService)
        {
            _registration = layerProperty.GetDataBindingRegistration(value => value);
        }

        public bool IsEnabled => _registration.DataBinding == null;

        protected override void OnDataBindingsChanged()
        {
            NotifyOfPropertyChange(nameof(IsEnabled));
        }
    }
}