using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Artemis.UI.Shared.Services.Builders
{
    /// <summary>
    ///     Represents a builder that can create a <see cref="SaveFileDialog" />.
    /// </summary>
    public class SaveFileDialogBuilder
    {
        private readonly Window _parent;
        private readonly SaveFileDialog _saveFileDialog;

        /// <summary>
        ///     Creates a new instance of the <see cref="SaveFileDialogBuilder" /> class.
        /// </summary>
        /// <param name="parent">The parent window that will host the notification.</param>
        public SaveFileDialogBuilder(Window parent)
        {
            _parent = parent;
            _saveFileDialog = new SaveFileDialog();
        }

        /// <summary>
        ///     Set the title of the dialog
        /// </summary>
        public SaveFileDialogBuilder WithTitle(string? title)
        {
            _saveFileDialog.Title = title;
            return this;
        }

        /// <summary>
        ///     Set the initial directory of the dialog
        /// </summary>
        public SaveFileDialogBuilder WithDirectory(string? directory)
        {
            _saveFileDialog.Directory = directory;
            return this;
        }

        /// <summary>
        ///     Set the initial file name of the dialog
        /// </summary>
        public SaveFileDialogBuilder WithInitialFileName(string? initialFileName)
        {
            _saveFileDialog.InitialFileName = initialFileName;
            return this;
        }

        /// <summary>
        ///     Set the default extension of the dialog
        /// </summary>
        public SaveFileDialogBuilder WithDefaultExtension(string? defaultExtension)
        {
            _saveFileDialog.DefaultExtension = defaultExtension;
            return this;
        }

        /// <summary>
        ///     Add a filter to the dialog
        /// </summary>
        public SaveFileDialogBuilder HavingFilter(Action<FileDialogFilterBuilder> configure)
        {
            FileDialogFilterBuilder builder = new();
            configure(builder);
            _saveFileDialog.Filters.Add(builder.Build());

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
            return await _saveFileDialog.ShowAsync(_parent);
        }
    }
}