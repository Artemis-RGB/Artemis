using System;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentValidation;
using SkiaSharp;
using Stylet;

namespace Artemis.UI.PropertyInput
{
    public class SKPointPropertyInputViewModel : PropertyInputViewModel<SKPoint>
    {
        private readonly DataBindingRegistration<SKPoint, float> _xRegistration;
        private readonly DataBindingRegistration<SKPoint, float> _yRegistration;

        public SKPointPropertyInputViewModel(LayerProperty<SKPoint> layerProperty, IProfileEditorService profileEditorService,
            IModelValidator<SKPointPropertyInputViewModel> validator) : base(layerProperty, profileEditorService, validator)
        {
            _xRegistration = layerProperty.GetDataBindingRegistration(point => point.X);
            _yRegistration = layerProperty.GetDataBindingRegistration(point => point.Y);
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

        public bool IsXEnabled => _xRegistration.DataBinding == null;
        public bool IsYEnabled => _yRegistration.DataBinding == null;

        protected override void OnInputValueChanged()
        {
            NotifyOfPropertyChange(nameof(X));
            NotifyOfPropertyChange(nameof(Y));
        }

        protected override void OnDataBindingsChanged()
        {
            NotifyOfPropertyChange(nameof(IsXEnabled));
            NotifyOfPropertyChange(nameof(IsYEnabled));
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