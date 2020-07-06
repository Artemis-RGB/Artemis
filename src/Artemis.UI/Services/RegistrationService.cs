using Artemis.Core;
using Artemis.UI.DataModelVisualization.Display;
using Artemis.UI.DataModelVisualization.Input;
using Artemis.UI.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IDataModelVisualizationService _dataModelVisualizationService;
        private readonly IProfileEditorService _profileEditorService;
        private bool _registeredBuiltInDataModelDisplays;
        private bool _registeredBuiltInDataModelInputs;
        private bool _registeredBuiltInPropertyEditors;

        public RegistrationService(IDataModelVisualizationService dataModelVisualizationService, IProfileEditorService profileEditorService)
        {
            _dataModelVisualizationService = dataModelVisualizationService;
            _profileEditorService = profileEditorService;
        }

        public void RegisterBuiltInDataModelDisplays()
        {
            if (_registeredBuiltInDataModelDisplays)
                return;

            _dataModelVisualizationService.RegisterDataModelDisplay<SKColorDataModelDisplayViewModel>(Constants.CorePluginInfo);

            _registeredBuiltInDataModelDisplays = true;
        }

        public void RegisterBuiltInDataModelInputs()
        {
            if (_registeredBuiltInDataModelInputs)
                return;

            _dataModelVisualizationService.RegisterDataModelInput<StringDataModelInputViewModel>(Constants.CorePluginInfo);

            _registeredBuiltInDataModelInputs = true;
        }

        public void RegisterBuiltInPropertyEditors()
        {
            if (_registeredBuiltInPropertyEditors)
                return;

            _profileEditorService.RegisterPropertyInput<BrushPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<ColorGradientPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<FloatPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<IntPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<SKColorPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<SKPointPropertyInputViewModel>(Constants.CorePluginInfo);
            _profileEditorService.RegisterPropertyInput<SKSizePropertyInputViewModel>(Constants.CorePluginInfo);

            _registeredBuiltInPropertyEditors = true;
        }
    }

    public interface IRegistrationService : IArtemisUIService
    {
        void RegisterBuiltInDataModelDisplays();
        void RegisterBuiltInDataModelInputs();
        void RegisterBuiltInPropertyEditors();
    }
}