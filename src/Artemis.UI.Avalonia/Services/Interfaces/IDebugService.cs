namespace Artemis.UI.Avalonia.Services.Interfaces
{
    public interface IDebugService : IArtemisUIService
    {
        void ShowDebugger();
        void ClearDebugger();
    }
}