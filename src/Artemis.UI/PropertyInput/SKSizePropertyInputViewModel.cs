using System;
using Artemis.Core.Extensions;
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
                .LessThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MaxInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue.IsNumber());
            RuleFor(vm => vm.Width)
                .GreaterThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MinInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MinInputValue.IsNumber());

            RuleFor(vm => vm.Height)
                .LessThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MaxInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue.IsNumber());
            RuleFor(vm => vm.Height)
                .GreaterThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MinInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MinInputValue.IsNumber());
        }
    }
}