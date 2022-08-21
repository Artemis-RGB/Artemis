using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Artemis.UI.Shared.Services.Builders;

/// <summary>
///     Represents a builder that can create a <see cref="OpenFileDialog" />.
/// </summary>
public class OpenFileDialogBuilder
{
    private readonly OpenFileDialog _openFileDialog;
    private readonly Window _parent;

    /// <summary>
    ///     Creates a new instance of the <see cref="OpenFileDialogBuilder" /> class.
    /// </summary>
    /// <param name="parent">The parent window that will host the dialog.</param>
    internal OpenFileDialogBuilder(Window parent)
    {
        _parent = parent;
        _openFileDialog = new OpenFileDialog();
    }

    /// <summary>
    ///     Indicate that the user can select multiple files.
    /// </summary>
    public OpenFileDialogBuilder WithAllowMultiple()
    {
        _openFileDialog.AllowMultiple = true;
        return this;
    }

    /// <summary>
    ///     Set the title of the dialog
    /// </summary>
    public OpenFileDialogBuilder WithTitle(string? title)
    {
        _openFileDialog.Title = title;
        return this;
    }

    /// <summary>
    ///     Set the initial directory of the dialog
    /// </summary>
    public OpenFileDialogBuilder WithDirectory(string? directory)
    {
        _openFileDialog.Directory = directory;
        return this;
    }

    /// <summary>
    ///     Set the initial file name of the dialog
    /// </summary>
    public OpenFileDialogBuilder WithInitialFileName(string? initialFileName)
    {
        _openFileDialog.InitialFileName = initialFileName;
        return this;
    }

    /// <summary>
    ///     Add a filter to the dialog
    /// </summary>
    public OpenFileDialogBuilder HavingFilter(Action<FileDialogFilterBuilder> configure)
    {
        FileDialogFilterBuilder builder = new();
        configure(builder);

        _openFileDialog.Filters ??= new List<FileDialogFilter>();
        _openFileDialog.Filters.Add(builder.Build());

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
        return await _openFileDialog.ShowAsync(_parent);
    }
}