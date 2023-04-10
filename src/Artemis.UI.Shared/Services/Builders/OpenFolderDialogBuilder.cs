using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can create a <see cref="OpenFolderDialog" />.
/// </summary>
public class OpenFolderDialogBuilder
{
    private readonly Window _parent;
    private readonly FolderPickerOpenOptions _options;

    /// <summary>
    ///     Creates a new instance of the <see cref="OpenFolderDialogBuilder" /> class.
    /// </summary>
    /// <param name="parent">The parent window that will host the dialog.</param>
    internal OpenFolderDialogBuilder(Window parent)
    {
        _parent = parent;
        _options = new FolderPickerOpenOptions {AllowMultiple = false};
    }

    /// <summary>
    ///     Set the title of the dialog
    /// </summary>
    public OpenFolderDialogBuilder WithTitle(string? title)
    {
        _options.Title = title;
        return this;
    }

    /// <summary>
    ///     Set the initial directory of the dialog
    /// </summary>
    public OpenFolderDialogBuilder WithDirectory(string? directory)
    {
        _options.SuggestedStartLocation = directory != null ? _parent.StorageProvider.TryGetFolderFromPathAsync(directory).GetAwaiter().GetResult() : null;
        return this;
    }

    /// <summary>
    ///     Asynchronously shows the folder dialog.
    /// </summary>
    /// <returns>
    ///     A task that on completion returns an array containing the full path to the selected
    ///     folder, or null if the dialog was canceled.
    /// </returns>
    public async Task<string?> ShowAsync()
    {
        IReadOnlyList<IStorageFolder> folder = await _parent.StorageProvider.OpenFolderPickerAsync(_options);
        return folder.FirstOrDefault()?.Path.AbsolutePath;
    }
}