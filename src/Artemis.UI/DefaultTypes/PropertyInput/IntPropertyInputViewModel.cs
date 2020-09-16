using Artemis.Core;
using Artemis.UI.Shared;
using Artemis.UI.Shared.Services;
using FluentValidation;
using Stylet;

namespace Artemis.UI.PropertyInput
{
    public class IntPropertyInputViewModel : PropertyInputViewModel<int>
    {
        private readonly DataBindingRegistration<int, int> _registration;

        public IntPropertyInputViewModel(LayerProperty<int> layerProperty, IProfileEditorService profileEditorService, IModelValidator<IntPropertyInputViewModel> validator)
            : base(layerProperty, profileEditorService, validator)
        {
            _registration = layerProperty.GetDataBindingRegistration(value => value);
        }

        public bool IsEnabled => _registration.DataBinding == null;

        protected override void OnDataBindingsChanged()
        {
            NotifyOfPropertyChange(nameof(IsEnabled));
        }
    }

    public class IntPropertyInputViewModelValidator : AbstractValidator<IntPropertyInputViewModel>
    {
        public IntPropertyInputViewModelValidator()
        {
            RuleFor(vm => vm.InputValue)
                .LessThanOrEqualTo(vm => (int) vm.LayerProperty.PropertyDescription.MaxInputValue)
                .When(vm => vm.LayerProperty.PropertyDescription.MaxInputValue is int);

            RuleFor(vm => vm.InputValue)
                .GreaterThanOrEqualTo(vm => (int) vm.LayerProperty.PropertyDescription.MinInputValue)
                .When(vm => vm.LayerProperty.PropertyDescription.MinInputValue is int);
        }
    }
}