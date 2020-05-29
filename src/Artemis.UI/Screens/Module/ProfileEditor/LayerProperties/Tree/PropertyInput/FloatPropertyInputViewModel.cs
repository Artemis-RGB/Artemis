using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public class FloatPropertyInputViewModel : PropertyInputViewModel<float>
    {
        public FloatPropertyInputViewModel(LayerPropertyViewModel<float> layerPropertyViewModel, IModelValidator<FloatPropertyInputViewModel> validator)
            : base(layerPropertyViewModel, validator)
        {
        }
    }

    public class FloatPropertyInputViewModelValidator : AbstractValidator<FloatPropertyInputViewModel>
    {
        public FloatPropertyInputViewModelValidator()
        {
            RuleFor(vm => vm.InputValue)
                .LessThanOrEqualTo(vm => (float) vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is float);

            RuleFor(vm => vm.InputValue)
                .GreaterThanOrEqualTo(vm => (float) vm.LayerPropertyViewModel.PropertyDescription.MinInputValue)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MinInputValue is float);
        }
    }
}