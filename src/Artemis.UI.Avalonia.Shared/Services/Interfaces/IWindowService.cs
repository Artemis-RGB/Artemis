namespace Artemis.UI.Avalonia.Shared.Services.Interfaces
{
    public interface IWindowService : IArtemisSharedUIService
    {
        T ShowWindow<T>();

        /// <summary>
        ///     Given a ViewModel, show its corresponding View as a window
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        void ShowWindow(object viewModel);

        /// <summary>
        ///     Given a ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>DialogResult of the View</returns>
        bool? ShowDialog(object viewModel);
    }
}