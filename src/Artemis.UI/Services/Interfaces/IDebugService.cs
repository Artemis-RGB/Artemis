namespace Artemis.UI.Services.Interfaces
{
    public interface IDebugService : IArtemisUIService
    {
        void ShowDebugger();
        void ClearDebugger();
    }
}