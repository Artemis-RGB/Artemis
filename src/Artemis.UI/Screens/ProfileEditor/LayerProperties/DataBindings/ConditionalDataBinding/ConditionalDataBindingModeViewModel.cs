using Artemis.Core;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.DataBindings.ConditionalDataBinding
{
    public class ConditionalDataBindingModeViewModel<TLayerProperty, TProperty> : Screen, IDataBindingModeViewModel
    {
        public ConditionalDataBindingModeViewModel(ConditionalDataBinding<TLayerProperty, TProperty> conditionalDataBinding)
        {
            ConditionalDataBinding = conditionalDataBinding;
        }

        public ConditionalDataBinding<TLayerProperty, TProperty> ConditionalDataBinding { get; }

        public void Dispose()
        {
        }

        public void Update()
        {
        }

        public object GetTestValue()
        {
            return null;
        }
    }
}