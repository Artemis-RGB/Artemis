using FluentValidation;

namespace Artemis.UI.Screens.Settings.Device.Tabs
{
    public class DevicePropertiesTabViewModelValidator : AbstractValidator<DevicePropertiesTabViewModel>
    {
        public DevicePropertiesTabViewModelValidator()
        {
            RuleFor(m => m.X).GreaterThanOrEqualTo(0).WithMessage("X-coordinate must be 0 or greater");

            RuleFor(m => m.Y).GreaterThanOrEqualTo(0).WithMessage("Y-coordinate must be 0 or greater");

            RuleFor(m => m.Scale).GreaterThanOrEqualTo(0.2).WithMessage("Scale must be 0.2 or greater");

            RuleFor(m => m.Rotation).GreaterThanOrEqualTo(0).WithMessage("Rotation must be 0 or greater");
            RuleFor(m => m.Rotation).LessThanOrEqualTo(359).WithMessage("Rotation must be 359 or less");
        }
    }
}