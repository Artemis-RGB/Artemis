using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can create a <see cref="SaveFileDialog" />.
/// </summary>
public class SaveFileDialogBuilder
{
    private readonly Window _parent;
    private readonly FilePickerSaveOptions _options;
    private List<FilePickerFileType>? _fileTypeFilters;
    
    /// <summary>
    ///     Creates a new instance of the <see cref="SaveFileDialogBuilder" /> class.
    /// </summary>
    /// <param name="parent">The parent window that will host the notification.</param>
    internal SaveFileDialogBuilder(Window parent)
    {
        _parent = parent;
        _options = new FilePickerSaveOptions();
    }

    /// <summary>
    ///     Set the title of the dialog
    /// </summary>
    public SaveFileDialogBuilder WithTitle(string? title)
    {
        _options.Title = title;
        return this;
    }

    /// <summary>
    ///     Set the initial directory of the dialog
    /// </summary>
    public SaveFileDialogBuilder WithDirectory(string? directory)
    {
        _options.SuggestedStartLocation = directory != null ? _parent.StorageProvider.TryGetFolderFromPathAsync(directory).GetAwaiter().GetResult() : null;
        return this;
    }

    /// <summary>
    ///     Set the initial file name of the dialog
    /// </summary>
    public SaveFileDialogBuilder WithInitialFileName(string? initialFileName)
    {
        _options.SuggestedFileName = initialFileName;
        return this;
    }

    /// <summary>
    ///     Add a filter to the dialog
    /// </summary>
    public SaveFileDialogBuilder HavingFilter(Action<FileDialogFilterBuilder> configure)
    {
        FileDialogFilterBuilder builder = new();
        configure(builder);

        _fileTypeFilters ??= new List<FilePickerFileType>();
        _fileTypeFilters.Add(builder.Build());
        _options.FileTypeChoices = _fileTypeFilters;

        return this;
    }

    /// <summary>
    ///     Asynchronously shows the save file dialog.
    /// </summary>
    /// <returns>
    ///     A task that on completion contains the full path of the save location, or null if the
    ///     dialog was canceled.
    /// </returns>
    public async Task<string?> ShowAsync()
    {
        IStorageFile? path = await _parent.StorageProvider.SaveFilePickerAsync(_options);
        return path?.Path.LocalPath;
    }
}