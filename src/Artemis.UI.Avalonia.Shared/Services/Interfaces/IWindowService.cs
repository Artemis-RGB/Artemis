using System.Threading.Tasks;

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
        /// Given a ViewModel, show its corresponding View as a Dialog
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <param name="viewModel">ViewModel to show the View for</param>
        /// <returns>A task containing the return value of type <typeparamref name="T"/></returns>
        Task<T> ShowDialog<T>(object viewModel);
    }
}