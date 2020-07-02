using System;
using Artemis.Core.Extensions;
using Artemis.Core.Models.Profile.LayerProperties;
using Artemis.UI.Shared.PropertyInput;
using Artemis.UI.Shared.Services.Interfaces;
using FluentValidation;
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

        public float X
        {
            get => InputValue.X;
            set => InputValue = new SKPoint(value, Y);
        }

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
                .LessThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MaxInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue.IsNumber());
            RuleFor(vm => vm.X)
                .GreaterThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MinInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MinInputValue.IsNumber());

            RuleFor(vm => vm.Y)
                .LessThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MaxInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue.IsNumber());
            RuleFor(vm => vm.Y)
                .GreaterThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MinInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MinInputValue.IsNumber());
        }
    }
}