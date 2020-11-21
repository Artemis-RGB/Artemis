using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DefaultTypes.DataModel.Display;
using Artemis.UI.DefaultTypes.DataModel.Input;
using Artemis.UI.InputProviders;
using Artemis.UI.Ninject;
using Artemis.UI.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Services;

namespace Artemis.UI.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly ISurfaceService _surfaceService;
        private readonly IInputService _inputService;
        private bool _registeredBuiltInDataModelDisplays;
        private bool _registeredBuiltInDataModelInputs;
        private bool _registeredBuiltInPropertyEditors;

        public RegistrationService(IDataModelUIService dataModelUIService, 
            IProfileEditorService profileEditorService, 
            IPluginManagementService pluginManagementService,
            ISurfaceService surfaceService,
            IInputService inputService)
        {
            _dataModelUIService = dataModelUIService;
            _profileEditorService = profileEditorService;
            _pluginManagementService = pluginManagementService;
            _surfaceService = surfaceService;
            _inputService = inputService;

            LoadPluginModules();
            pluginManagementService.PluginEnabling += PluginServiceOnPluginEnabling;
        }

        public void RegisterBuiltInDataModelDisplays()
        {
            if (_registeredBuiltInDataModelDisplays)
                return;

            _dataModelUIService.RegisterDataModelDisplay<SKColorDataModelDisplayViewModel>(Constants.CorePlugin);

            _registeredBuiltInDataModelDisplays = true;
        }

        public void RegisterBuiltInDataModelInputs()
        {
            if (_registeredBuiltInDataModelInputs)
                return;

            _dataModelUIService.RegisterDataModelInput<DoubleDataModelInputViewModel>(Constants.CorePlugin, Constants.FloatNumberTypes);
            _dataModelUIService.RegisterDataModelInput<IntDataModelInputViewModel>(Constants.CorePlugin, Constants.IntegralNumberTypes);
            _dataModelUIService.RegisterDataModelInput<SKColorDataModelInputViewModel>(Constants.CorePlugin, null);
            _dataModelUIService.RegisterDataModelInput<StringDataModelInputViewModel>(Constants.CorePlugin, null);
            _dataModelUIService.RegisterDataModelInput<EnumDataModelInputViewModel>(Constants.CorePlugin, null);
            _dataModelUIService.RegisterDataModelInput<BoolDataModelInputViewModel>(Constants.CorePlugin, null);

            _registeredBuiltInDataModelInputs = true;
        }

        public void RegisterBuiltInPropertyEditors()
        {
            if (_registeredBuiltInPropertyEditors)
                return;

            _profileEditorService.RegisterPropertyInput<BrushPropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<ColorGradientPropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<FloatPropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<IntPropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<SKColorPropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<SKPointPropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<SKSizePropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput(typeof(EnumPropertyInputViewModel<>), Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<BoolPropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<FloatRangePropertyInputViewModel>(Constants.CorePlugin);
            _profileEditorService.RegisterPropertyInput<IntRangePropertyInputViewModel>(Constants.CorePlugin);

            _registeredBuiltInPropertyEditors = true;
        }

        public void RegisterInputProvider()
        {
            _inputService.AddInputProvider(new NativeWindowInputProvider(_surfaceService));
        }

        private void PluginServiceOnPluginEnabling(object sender, PluginEventArgs e)
        {
            e.Plugin.Kernel.Load(new[] {new PluginUIModule(e.Plugin)});
        }

        private void LoadPluginModules()
        {
            foreach (Plugin plugin in _pluginManagementService.GetAllPlugins().Where(p => p.IsEnabled)) 
                plugin.Kernel.Load(new[] {new PluginUIModule(plugin)});
        }
    }

    public interface IRegistrationService : IArtemisUIService
    {
        void RegisterBuiltInDataModelDisplays();
        void RegisterBuiltInDataModelInputs();
        void RegisterBuiltInPropertyEditors();
        void RegisterInputProvider();
    }
}