using System;
using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;

namespace Artemis.UI.PropertyInput
{
    public class FloatRangePropertyInputViewModel : PropertyInputViewModel<FloatRange>
    {
        private readonly DataBindingRegistration<FloatRange, float> _startRegistration;
        private readonly DataBindingRegistration<FloatRange, float> _endRegistration;

        public FloatRangePropertyInputViewModel(LayerProperty<FloatRange> layerProperty,
            IProfileEditorService profileEditorService,
            IModelValidator<FloatRangePropertyInputViewModel> validator) : base(layerProperty, profileEditorService, validator)
        {
            _startRegistration = layerProperty.GetDataBindingRegistration(range => range.Start);
            _endRegistration = layerProperty.GetDataBindingRegistration(range => range.End);
        }

        public float Start
        {
            get => InputValue?.Start ?? 0f;
            set
            {
                if (InputValue == null)
                    InputValue = new FloatRange(value, value + 1f);
                else
                    InputValue.Start = value;

                NotifyOfPropertyChange(nameof(Start));
            }
        }

        public float End
        {
            get => InputValue?.End ?? 0f;
            set
            {
                if (InputValue == null)
                    InputValue = new FloatRange(value - 1f, value);
                else
                    InputValue.End = value;

                NotifyOfPropertyChange(nameof(End));
            }
        }

        public bool IsStartEnabled => _startRegistration.DataBinding == null;
        public bool IsEndEnabled => _endRegistration.DataBinding == null;

        protected override void OnInputValueChanged()
        {
            NotifyOfPropertyChange(nameof(Start));
            NotifyOfPropertyChange(nameof(End));
        }

        protected override void OnDataBindingsChanged()
        {
            NotifyOfPropertyChange(nameof(IsStartEnabled));
            NotifyOfPropertyChange(nameof(IsEndEnabled));
        }
    }

    public class FloatRangePropertyInputViewModelValidator : AbstractValidator<FloatRangePropertyInputViewModel>
    {
        public FloatRangePropertyInputViewModelValidator()
        {
            RuleFor(vm => vm.Start).LessThanOrEqualTo(vm => vm.End);
            RuleFor(vm => vm.End).GreaterThanOrEqualTo(vm => vm.Start);

            RuleFor(vm => vm.Start)
                .GreaterThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MinInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MinInputValue.IsNumber());
            RuleFor(vm => vm.End)
                .LessThanOrEqualTo(vm => Convert.ToSingle(vm.LayerProperty.PropertyDescription.MaxInputValue))
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue.IsNumber());
        }
    }
}