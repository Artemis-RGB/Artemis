using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public class IntPropertyInputViewModel : PropertyInputViewModel<int>
    {
        public IntPropertyInputViewModel(LayerPropertyViewModel<int> layerPropertyViewModel, IModelValidator<IntPropertyInputViewModel> validator)
            : base(layerPropertyViewModel, validator)
        {
        }
    }

    public class IntPropertyInputViewModelValidator : AbstractValidator<IntPropertyInputViewModel>
    {
        public IntPropertyInputViewModelValidator()
        {
            RuleFor(vm => vm.InputValue)
                .LessThanOrEqualTo(vm => (int) vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is int);

            RuleFor(vm => vm.InputValue)
                .GreaterThanOrEqualTo(vm => (int) vm.LayerPropertyViewModel.PropertyDescription.MinInputValue)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MinInputValue is int);
        }
    }
}