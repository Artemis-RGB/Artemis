using System;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentValidation;
using SkiaSharp;
using Stylet;

// using PropertyChanged;

namespace Artemis.UI.PropertyInput
{
    public class SKSizePropertyInputViewModel : PropertyInputViewModel<SKSize>
    {
        private readonly DataBindingRegistration<SKSize, float> _heightRegistration;
        private readonly DataBindingRegistration<SKSize, float> _widthRegistration;

        public SKSizePropertyInputViewModel(LayerProperty<SKSize> layerProperty, IProfileEditorService profileEditorService,
            IModelValidator<SKSizePropertyInputViewModel> validator) : base(layerProperty, profileEditorService, validator)
        {
            _widthRegistration = layerProperty.GetDataBindingRegistration(size => size.Width);
            _heightRegistration = layerProperty.GetDataBindingRegistration(size => size.Height);
        }

        // Since SKSize is immutable we need to create properties that replace the SKSize entirely
        public float Width
        {
            get => InputValue.Width;
            set => InputValue = new SKSize(value, Height);
        }

        public float Height
        {
            get => InputValue.Height;
            set => InputValue = new SKSize(Width, value);
        }

        public bool IsWidthEnabled => _widthRegistration.DataBinding == null;
        public bool IsHeightEnabled => _heightRegistration.DataBinding == null;

        protected override void OnInputValueChanged()
        {
            NotifyOfPropertyChange(nameof(Width));
            NotifyOfPropertyChange(nameof(Height));
        }

        protected override void OnDataBindingsChanged()
        {
            NotifyOfPropertyChange(nameof(IsWidthEnabled));
            NotifyOfPropertyChange(nameof(IsHeightEnabled));
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