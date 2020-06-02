using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;
using FluentValidation;
using PropertyChanged;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.PropertyInput
{
    public class SKPointPropertyInputViewModel : PropertyInputViewModel<SKPoint>
    {
        public SKPointPropertyInputViewModel(LayerProperty<SKPoint> layerProperty, IProfileEditorService profileEditorService,
            IModelValidator<SKPointPropertyInputViewModel> validator) : base(layerProperty, profileEditorService, validator)
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
                .LessThanOrEqualTo(vm => ((SKPoint) vm.LayerProperty.PropertyDescription.MaxInputValue).X)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is SKPoint);
            RuleFor(vm => vm.X)
                .GreaterThanOrEqualTo(vm => ((SKPoint) vm.LayerProperty.PropertyDescription.MaxInputValue).X)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is SKPoint);

            RuleFor(vm => vm.Y)
                .LessThanOrEqualTo(vm => ((SKPoint) vm.LayerProperty.PropertyDescription.MaxInputValue).Y)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is SKPoint);
            RuleFor(vm => vm.Y)
                .GreaterThanOrEqualTo(vm => ((SKPoint) vm.LayerProperty.PropertyDescription.MaxInputValue).Y)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is SKPoint);
        }
    }
}