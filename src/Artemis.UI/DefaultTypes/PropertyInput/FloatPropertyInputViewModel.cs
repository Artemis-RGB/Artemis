using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;

namespace Artemis.UI.DefaultTypes.PropertyInput
{
    public class FloatPropertyInputViewModel : PropertyInputViewModel<float>
    {
        public FloatPropertyInputViewModel(LayerProperty<float> layerProperty, IProfileEditorService profileEditorService, IModelValidator<FloatPropertyInputViewModel> validator)
            : base(layerProperty, profileEditorService, validator)
        {
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