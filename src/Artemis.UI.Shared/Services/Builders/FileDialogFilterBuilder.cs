using Avalonia.Controls;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can create a <see cref="FileDialogFilter" />.
/// </summary>
public class FileDialogFilterBuilder
{
    private readonly FileDialogFilter _filter;

    internal FileDialogFilterBuilder()
    {
        _filter = new FileDialogFilter();
    }

    /// <summary>
    ///     Sets the name of the filter
    /// </summary>
    public FileDialogFilterBuilder WithName(string name)
    {
        _filter.Name = name;
        return this;
    }

    /// <summary>
    ///     Adds the provided extension to the filter
    /// </summary>
    public FileDialogFilterBuilder WithExtension(string extension)
    {
        _filter.Extensions.Add(extension);
        return this;
    }

    internal FileDialogFilter Build()
    {
        return _filter;
    }
}