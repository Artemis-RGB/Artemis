using System;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Artemis.UI.Shared.Services;
using Castle.Core.Internal;
using FluentValidation;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.LayerProperties.Timeline.Dialogs
{
    public class TimelineSegmentDialogViewModel : DialogViewModelBase
    {
        private string _inputValue;

        public TimelineSegmentDialogViewModel(IModelValidator<TimelineSegmentDialogViewModel> validator, TimelineSegmentViewModel segment)
            : base(validator)
        {
            Segment = segment;
            InputValue = $"{Math.Floor(Segment.SegmentLength.TotalSeconds):00}.{Segment.SegmentLength.Milliseconds:000}";
        }

        public TimelineSegmentViewModel Segment { get; }

        public string InputValue
        {
            get => _inputValue;
            set => SetAndNotify(ref _inputValue, value);
        }

        public async Task Accept()
        {
            await ValidateAsync();

            if (HasErrors)
                return;

            Segment.UpdateLength(TimelineSegmentDialogViewModelValidator.CreateTime(InputValue));
            Session.Close();
        }
    }

    public class TimelineSegmentDialogViewModelValidator : AbstractValidator<TimelineSegmentDialogViewModel>
    {
        private readonly Regex _inputRegex = new("^[.][-|0-9]+$|^-?[0-9]*[.]{0,1}[0-9]*$");

        public TimelineSegmentDialogViewModelValidator()
        {
            CascadeMode = CascadeMode.Stop;

            RuleFor(m => m.InputValue)
                .NotNull()
                .WithMessage("A timeline length is required");
            RuleFor(m => m.InputValue)
                .Must(ValidateTime)
                .WithMessage("Input cannot be converted to a time");
            RuleFor(m => m.InputValue)
                .Transform(CreateTime)
                .GreaterThanOrEqualTo(TimeSpan.FromMilliseconds(100))
                .WithMessage("Minimum timeline length is 100ms");
            RuleFor(m => m.InputValue)
                .Transform(CreateTime)
                .LessThanOrEqualTo(TimeSpan.FromHours(24))
                .WithMessage("Maximum timeline length is 24 hours");
        }

        public static TimeSpan CreateTime(string s)
        {
            string[] parts = s.Split(".");

            // Only seconds provided
            if (parts.Length == 1)
                return TimeSpan.FromSeconds(double.Parse(parts[0]));
            // Only milliseconds provided with a leading .
            if (parts[0].IsNullOrEmpty())
            {
                // Add trailing zeros so 2.5 becomes 2.500, can't seem to make double.Parse do that
                while (parts[0].Length < 3) parts[0] += "0";
                return TimeSpan.FromMilliseconds(double.Parse(parts[1], CultureInfo.InvariantCulture));
            }

            // Seconds and milliseconds provided
            // Add trailing zeros so 2.5 becomes 2.500, can't seem to make double.Parse do that
            while (parts[1].Length < 3) parts[1] += "0";
            return TimeSpan.FromSeconds(double.Parse(parts[0])).Add(TimeSpan.FromMilliseconds(double.Parse(parts[1])));
        }

        private bool ValidateTime(string arg)
        {
            return _inputRegex.IsMatch(arg);
        }
    }
}