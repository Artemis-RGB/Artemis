using System.Threading.Tasks;
using Avalonia.Controls;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can create a <see cref="OpenFolderDialog" />.
/// </summary>
public class OpenFolderDialogBuilder
{
    private readonly OpenFolderDialog _openFolderDialog;
    private readonly Window _parent;

    /// <summary>
    ///     Creates a new instance of the <see cref="OpenFolderDialogBuilder" /> class.
    /// </summary>
    /// <param name="parent">The parent window that will host the dialog.</param>
    internal OpenFolderDialogBuilder(Window parent)
    {
        _parent = parent;
        _openFolderDialog = new OpenFolderDialog();
    }


    /// <summary>
    ///     Set the title of the dialog
    /// </summary>
    public OpenFolderDialogBuilder WithTitle(string? title)
    {
        _openFolderDialog.Title = title;
        return this;
    }

    /// <summary>
    ///     Set the initial directory of the dialog
    /// </summary>
    public OpenFolderDialogBuilder WithDirectory(string? directory)
    {
        _openFolderDialog.Directory = directory;
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
        return await _openFolderDialog.ShowAsync(_parent);
    }
}