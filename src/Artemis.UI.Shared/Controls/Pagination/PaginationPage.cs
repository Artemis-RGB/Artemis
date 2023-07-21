using System.Windows.Input;
using Avalonia;
using Avalonia.Controls.Primitives;

namespace Artemis.UI.Shared.Pagination;

/// <summary>
/// Represents a pagination page control that indicates a page in pagination.
/// </summary>
public class PaginationPage : TemplatedControl
{
    /// <summary>
    ///     Defines the <see cref="Page" /> property.
    /// </summary>
    public static readonly StyledProperty<int> PageProperty = AvaloniaProperty.Register<PaginationPage, int>(nameof(Page));

    /// <summary>
    /// Gets or sets the page that is being represented.
    /// </summary>
    public int Page
    {
        get => GetValue(PageProperty);
        set => SetValue(PageProperty, value);
    }
    
    /// <summary>
    ///     Defines the <see cref="Command" /> property.
    /// </summary>
    public static readonly StyledProperty<ICommand> CommandProperty = AvaloniaProperty.Register<PaginationPage, ICommand>(nameof(Command));

    /// <summary>
    /// Gets or sets the command to invoke when the page is clicked.
    /// </summary>
    public ICommand Command
    {
        get => GetValue(CommandProperty);
        set => SetValue(CommandProperty, value);
    }
}