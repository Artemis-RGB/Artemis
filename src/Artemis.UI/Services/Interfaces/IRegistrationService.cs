namespace Artemis.UI.Services.Interfaces
{
    public interface IRegistrationService : IArtemisUIService
    {
        void RegisterBuiltInDataModelDisplays();
        void RegisterBuiltInDataModelInputs();
        void RegisterBuiltInPropertyEditors();
        void RegisterControllers();
        void ApplyPreferredGraphicsContext();
        void RegisterBuiltInNodeTypes();
    }
}