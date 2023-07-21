using System;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Artemis.UI.Shared.Pagination;

public partial class Pagination : TemplatedControl
{
    /// <summary>
    ///     Defines the <see cref="Value" /> property
    /// </summary>
    public static readonly StyledProperty<int> ValueProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(Value), 1, enableDataValidation: true, coerce: (p, v) => Math.Clamp(v, 1, ((Pagination) p).Maximum));

    /// <summary>
    ///     Defines the <see cref="Maximum" /> property
    /// </summary>
    public static readonly StyledProperty<int> MaximumProperty =
        AvaloniaProperty.Register<Pagination, int>(nameof(Maximum), 10, coerce: (_, v) => Math.Max(1, v));

    /// <summary>
    ///     Gets or sets the numeric value of a NumberBox.
    /// </summary>
    public int Value
    {
        get => GetValue(ValueProperty);
        set => SetValue(ValueProperty, value);
    }

    /// <summary>
    ///     Gets or sets the numerical maximum for Value.
    /// </summary>
    public int Maximum
    {
        get => GetValue(MaximumProperty);
        set => SetValue(MaximumProperty, value);
    }
}