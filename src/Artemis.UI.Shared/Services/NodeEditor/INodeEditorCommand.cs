namespace Artemis.UI.Shared.Services.NodeEditor
{
    /// <summary>
    ///     Represents a command that can be executed and if needed, undone
    /// </summary>
    public interface INodeEditorCommand
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