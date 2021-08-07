using System;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Controllers;
using Artemis.UI.DefaultTypes.DataModel.Display;
using Artemis.UI.DefaultTypes.DataModel.Input;
using Artemis.UI.DefaultTypes.PropertyInput;
using Artemis.UI.Ninject;
using Artemis.UI.Providers;
using Artemis.UI.Shared.Services;
using Artemis.UI.SkiaSharp;
using Artemis.VisualScripting.Nodes;
using Serilog;

namespace Artemis.UI.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ILogger _logger;
        private readonly ICoreService _coreService;
        private readonly IDataModelUIService _dataModelUIService;
        private readonly IProfileEditorService _profileEditorService;
        private readonly IPluginManagementService _pluginManagementService;
        private readonly IInputService _inputService;
        private readonly IMessageService _messageService;
        private readonly IWebServerService _webServerService;
        private readonly IRgbService _rgbService;
        private readonly INodeService _nodeService;
        private readonly ISettingsService _settingsService;
        private bool _registeredBuiltInDataModelDisplays;
        private bool _registeredBuiltInDataModelInputs;
        private bool _registeredBuiltInPropertyEditors;
        private VulkanContext _vulkanContext;

        public RegistrationService(ILogger logger,
            ICoreService coreService,
            IDataModelUIService dataModelUIService,
            IProfileEditorService profileEditorService,
            IPluginManagementService pluginManagementService,
            IInputService inputService,
            IMessageService messageService,
            IWebServerService webServerService,
            IRgbService rgbService,
            INodeService nodeService,
            ISettingsService settingsService)
        {
            _logger = logger;
            _coreService = coreService;
            _dataModelUIService = dataModelUIService;
            _profileEditorService = profileEditorService;
            _pluginManagementService = pluginManagementService;
            _inputService = inputService;
            _messageService = messageService;
            _webServerService = webServerService;
            _rgbService = rgbService;
            _nodeService = nodeService;
            _settingsService = settingsService;

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

        public void RegisterProviders()
        {
            _inputService.AddInputProvider(new NativeWindowInputProvider(_logger, _inputService));
            _messageService.SetNotificationProvider(new ToastNotificationProvider());
        }

        public void RegisterControllers()
        {
            _webServerService.AddController<RemoteController>(Constants.CorePlugin.Features.First().Instance!);
        }

        public void RegisterBuiltInNodeTypes()
        {
            foreach (Type nodeType in typeof(SumIntegersNode).Assembly.GetTypes().Where(t => typeof(INode).IsAssignableFrom(t) && t.IsPublic && !t.IsAbstract && !t.IsInterface))
                _nodeService.RegisterNodeType(Constants.CorePlugin, nodeType);
        }

        /// <inheritdoc />
        public void ApplyPreferredGraphicsContext()
        {
            if (_coreService.StartupArguments.Contains("--force-software-render"))
            {
                _logger.Warning("Startup argument '--force-software-render' is applied, forcing software rendering.");
                _rgbService.UpdateGraphicsContext(null);
                return;
            }

            PluginSetting<string> preferredGraphicsContext = _settingsService.GetSetting("Core.PreferredGraphicsContext", "Vulkan");

            try
            {
                switch (preferredGraphicsContext.Value)
                {
                    case "Software":
                        _rgbService.UpdateGraphicsContext(null);
                        break;
                    case "Vulkan":
                        _vulkanContext ??= new VulkanContext();
                        _rgbService.UpdateGraphicsContext(_vulkanContext);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
            catch (Exception e)
            {
                _logger.Warning(e, "Failed to apply preferred graphics context {preferred}", preferredGraphicsContext.Value);
                _rgbService.UpdateGraphicsContext(null);
            }
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
        void RegisterProviders();
        void RegisterControllers();
        void ApplyPreferredGraphicsContext();
        void RegisterBuiltInNodeTypes();
    }
}