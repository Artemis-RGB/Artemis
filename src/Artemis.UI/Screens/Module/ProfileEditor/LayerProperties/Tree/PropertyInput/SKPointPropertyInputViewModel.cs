using Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput.Abstract;
using FluentValidation;
using PropertyChanged;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.Screens.Module.ProfileEditor.LayerProperties.Tree.PropertyInput
{
    public class SKPointPropertyInputViewModel : PropertyInputViewModel<SKPoint>
    {
        public SKPointPropertyInputViewModel(LayerPropertyViewModel<SKPoint> layerPropertyViewModel, IModelValidator<SKPointPropertyInputViewModel> validator) 
            : base(layerPropertyViewModel, validator)
        {
        }

        // Since SKPoint is immutable we need to create properties that replace the SKPoint entirely
        [DependsOn(nameof(InputValue))]
        public float X
        {
            get => InputValue.X;
            set => InputValue = new SKPoint(value, Y);
        }

        [DependsOn(nameof(InputValue))]
        public float Y
        {
            get => InputValue.Y;
            set => InputValue = new SKPoint(X, value);
        }

        protected override void OnInputValueChanged()
        {
            NotifyOfPropertyChange(nameof(X));
            NotifyOfPropertyChange(nameof(Y));
        }
    }

    public class SKPointPropertyInputViewModelValidator : AbstractValidator<SKPointPropertyInputViewModel>
    {
        public SKPointPropertyInputViewModelValidator()
        {
            RuleFor(vm => vm.X)
                .LessThanOrEqualTo(vm => ((SKPoint) vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue).X)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is SKPoint);
            RuleFor(vm => vm.X)
                .GreaterThanOrEqualTo(vm => ((SKPoint) vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue).X)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is SKPoint);

            RuleFor(vm => vm.Y)
                .LessThanOrEqualTo(vm => ((SKPoint) vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue).Y)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is SKPoint);
            RuleFor(vm => vm.Y)
                .GreaterThanOrEqualTo(vm => ((SKPoint) vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue).Y)
                .When(vm => vm.LayerPropertyViewModel.PropertyDescription.MaxInputValue is SKPoint);
        }
    }
}