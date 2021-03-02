using System.Collections.Generic;
using System.Linq;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.DefaultTypes.PropertyInput
{
    public class BoolPropertyInputViewModel : PropertyInputViewModel<bool>
    {
        private List<IDataBindingRegistration> _registrations;

        public BoolPropertyInputViewModel(LayerProperty<bool> layerProperty, IProfileEditorService profileEditorService) : base(layerProperty, profileEditorService)
        {
            _registrations = layerProperty.GetAllDataBindingRegistrations();
        }

        public bool IsEnabled => _registrations.Any(r => r.GetDataBinding() != null);

        protected override void OnDataBindingsChanged()
        {
            NotifyOfPropertyChange(nameof(IsEnabled));
        }
    }
}