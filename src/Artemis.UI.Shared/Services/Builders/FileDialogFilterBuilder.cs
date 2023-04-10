using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can create a <see cref="FileDialogFilter" />.
/// </summary>
public class FileDialogFilterBuilder
{
    private string _name;
    private readonly List<string> _extensions = new();

    internal FileDialogFilterBuilder()
    {
        _name = "Unknown";
    }

    /// <summary>
    ///     Sets the name of the filter
    /// </summary>
    public FileDialogFilterBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    /// <summary>
    ///     Adds the provided extension to the filter
    /// </summary>
    public FileDialogFilterBuilder WithExtension(string extension)
    {
        if (!_extensions.Contains(extension))
            _extensions.Add(extension);
        return this;
    }

    internal FilePickerFileType Build()
    {
        return new FilePickerFileType(_name)
        {
            Patterns = _extensions.Select(e => "*." + e).ToList()
        };
    }
}