using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;

namespace Artemis.UI.PropertyInput
{
    public class FloatPropertyInputViewModel : PropertyInputViewModel<float>
    {
        private readonly DataBindingRegistration<float, float> _registration;

        public FloatPropertyInputViewModel(LayerProperty<float> layerProperty, IProfileEditorService profileEditorService, IModelValidator<FloatPropertyInputViewModel> validator)
            : base(layerProperty, profileEditorService, validator)
        {
            _registration = layerProperty.GetDataBindingRegistration(value => value);
        }

        public bool IsEnabled => _registration.DataBinding == null;

        protected override void OnDataBindingsChanged()
        {
            NotifyOfPropertyChange(nameof(IsEnabled));
        }
    }

    public class FloatPropertyInputViewModelValidator : AbstractValidator<FloatPropertyInputViewModel>
    {
        public FloatPropertyInputViewModelValidator()
        {
            RuleFor(vm => vm.InputValue)
                .LessThanOrEqualTo(vm => (float) vm.LayerProperty.PropertyDescription.MaxInputValue)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is float);

            RuleFor(vm => vm.InputValue)
                .GreaterThanOrEqualTo(vm => (float) vm.LayerProperty.PropertyDescription.MinInputValue)
                .When(vm => vm.LayerProperty.PropertyDescription.MinInputValue is float);
        }
    }
}