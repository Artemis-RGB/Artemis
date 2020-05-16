using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using FluentValidation;
using PropertyChanged;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public class SKSizePropertyInputViewModel : PropertyInputViewModel<SKSize>
    {
        public SKSizePropertyInputViewModel(LayerPropertyViewModel<SKSize> layerPropertyViewModel, IModelValidator validator) : base(layerPropertyViewModel, validator)
        {
        }

        // Since SKSize is immutable we need to create properties that replace the SKSize entirely
        [DependsOn(nameof(InputValue))]
        public float Width
        {
            get => InputValue.Width;
            set => InputValue = new SKSize(value, Height);
        }

        [DependsOn(nameof(InputValue))]
        public float Height
        {
            get => InputValue.Height;
            set => InputValue = new SKSize(Width, value);
        }

        protected override void OnInputValueChanged()
        {
            NotifyOfPropertyChange(nameof(Width));
            NotifyOfPropertyChange(nameof(Height));
        }
    }

    public class SKSizePropertyInputViewModelValidator : AbstractValidator<SKSizePropertyInputViewModel>
    {
        public SKSizePropertyInputViewModelValidator()
        {
            RuleFor(vm => vm.Width)
                .LessThanOrEqualTo(vm => ((SKSize)vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue).Width)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is SKSize);
            RuleFor(vm => vm.Width)
                .GreaterThanOrEqualTo(vm => ((SKSize)vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue).Width)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is SKSize);

            RuleFor(vm => vm.Height)
                .LessThanOrEqualTo(vm => ((SKSize)vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue).Height)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is SKSize);
            RuleFor(vm => vm.Height)
                .GreaterThanOrEqualTo(vm => ((SKSize)vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue).Height)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is SKSize);
        }
    }
}