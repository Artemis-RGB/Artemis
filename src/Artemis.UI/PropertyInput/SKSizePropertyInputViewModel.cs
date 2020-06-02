using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;
using FluentValidation;
using PropertyChanged;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.PropertyInput
{
    public class SKSizePropertyInputViewModel : PropertyInputViewModel<SKSize>
    {
        public SKSizePropertyInputViewModel(LayerProperty<SKSize> layerProperty, IProfileEditorService profileEditorService,
            IModelValidator<SKSizePropertyInputViewModel> validator) : base(layerProperty, profileEditorService, validator)
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
                .LessThanOrEqualTo(vm => ((SKSize) vm.LayerProperty.PropertyDescription.MaxInputValue).Width)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is SKSize);
            RuleFor(vm => vm.Width)
                .GreaterThanOrEqualTo(vm => ((SKSize) vm.LayerProperty.PropertyDescription.MaxInputValue).Width)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is SKSize);

            RuleFor(vm => vm.Height)
                .LessThanOrEqualTo(vm => ((SKSize) vm.LayerProperty.PropertyDescription.MaxInputValue).Height)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is SKSize);
            RuleFor(vm => vm.Height)
                .GreaterThanOrEqualTo(vm => ((SKSize) vm.LayerProperty.PropertyDescription.MaxInputValue).Height)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is SKSize);
        }
    }
}