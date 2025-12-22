using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can create a <see cref="OpenFileDialog" />.
/// </summary>
public class OpenFileDialogBuilder
{
    private readonly Window _parent;
    private readonly FilePickerOpenOptions _options;
    private List<FilePickerFileType>? _fileTypeFilters;

    /// <summary>
    ///     Creates a new instance of the <see cref="OpenFileDialogBuilder" /> class.
    /// </summary>
    /// <param name="parent">The parent window that will host the dialog.</param>
    internal OpenFileDialogBuilder(Window parent)
    {
        _parent = parent;
        _options = new FilePickerOpenOptions();
    }

    /// <summary>
    ///     Indicate that the user can select multiple files.
    /// </summary>
    public OpenFileDialogBuilder WithAllowMultiple()
    {
        _options.AllowMultiple = true;
        return this;
    }

    /// <summary>
    ///     Set the title of the dialog
    /// </summary>
    public OpenFileDialogBuilder WithTitle(string? title)
    {
        _options.Title = title;
        return this;
    }

    /// <summary>
    ///     Set the initial directory of the dialog
    /// </summary>
    public OpenFileDialogBuilder WithDirectory(string? directory)
    {
        _options.SuggestedStartLocation = directory != null ? _parent.StorageProvider.TryGetFolderFromPathAsync(directory).GetAwaiter().GetResult() : null;
        return this;
    }

    /// <summary>
    ///     Add a filter to the dialog
    /// </summary>
    public OpenFileDialogBuilder HavingFilter(Action<FileDialogFilterBuilder> configure)
    {
        FileDialogFilterBuilder builder = new();
        configure(builder);

        _fileTypeFilters ??= [];
        _fileTypeFilters.Add(builder.Build());
        _options.FileTypeFilter = _fileTypeFilters;

        return this;
    }

    /// <summary>
    ///     Asynchronously shows the file dialog.
    /// </summary>
    /// <returns>
    ///     A task that on completion returns an array containing the full path to the selected
    ///     files, or null if the dialog was canceled.
    /// </returns>
    public async Task<string[]?> ShowAsync()
    {
        IReadOnlyList<IStorageFile> files = await _parent.StorageProvider.OpenFilePickerAsync(_options);
        return files.Count == 0 ? null : files.Select(f => f.Path.LocalPath).ToArray();
    }
}