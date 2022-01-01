namespace Artemis.UI.Services.ProfileEditor
{
    /// <summary>
    ///     Represents a command that can be executed and if needed, undone
    /// </summary>
    public interface IProfileEditorCommand
    {
        /// <summary>
        ///     Gets the name of the command
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///     Executes the command
        /// </summary>
        void Execute();

        /// <summary>
        ///     Undoes the command
        /// </summary>
        void Undo();
    }
}