using Artemis.Core;
using Artemis.UI.DefaultTypes.DataModel.Display;
using Artemis.UI.DefaultTypes.DataModel.Input;
using Artemis.UI.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private bool _registeredBuiltInDataModelDisplays;
        private bool _registeredBuiltInDataModelInputs;
        private bool _registeredBuiltInPropertyEditors;

        public RegistrationService(IDataModelUIService dataModelUIService, IProfileEditorService profileEditorService)
        {
            _dataModelUIService = dataModelUIService;
            _profileEditorService = profileEditorService;
        }

        public void RegisterBuiltInDataModelDisplays()
        {
            if (_registeredBuiltInDataModelDisplays)
                return;

            _dataModelUIService.RegisterDataModelDisplay<SKColorDataModelDisplayViewModel>(Constants.CorePluginInfo);

            _registeredBuiltInDataModelDisplays = true;
        }

        public void RegisterBuiltInDataModelInputs()
        {
            if (_registeredBuiltInDataModelInputs)
                return;

            _dataModelUIService.RegisterDataModelInput<DoubleDataModelInputViewModel>(Constants.CorePluginInfo, Constants.FloatNumberTypes);
            _dataModelUIService.RegisterDataModelInput<IntDataModelInputViewModel>(Constants.CorePluginInfo, Constants.IntegralNumberTypes);
            _dataModelUIService.RegisterDataModelInput<SKColorDataModelInputViewModel>(Constants.CorePluginInfo, null);
            _dataModelUIService.RegisterDataModelInput<StringDataModelInputViewModel>(Constants.CorePluginInfo, null);

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
            _profileEditorService.RegisterPropertyInput(typeof(EnumPropertyInputViewModel<>), Constants.CorePluginInfo);

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