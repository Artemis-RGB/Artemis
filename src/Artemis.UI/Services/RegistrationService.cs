using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Controllers;
using Artemis.UI.DefaultTypes.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.DefaultTypes.DataModel.Display;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Routing;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using Avalonia;
using DryIoc;

namespace Artemis.UI.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IDataModelUIService _dataModelUIService;
    private readonly IInputService _inputService;
    private readonly IContainer _container;
    private readonly IRouter _router;
    private readonly IPropertyInputService _propertyInputService;
    private readonly IWebServerService _webServerService;
    private bool _registeredBuiltInPropertyEditors;

    public RegistrationService(IContainer container,
        IRouter router,
        IInputService inputService,
        IPropertyInputService propertyInputService,
        IProfileEditorService profileEditorService,
        IDataModelUIService dataModelUIService,
        IWebServerService webServerService,
        IDeviceLayoutService deviceLayoutService // here to make sure it is instantiated
    )
    {
        _container = container;
        _router = router;
        _inputService = inputService;
        _propertyInputService = propertyInputService;
        _dataModelUIService = dataModelUIService;
        _webServerService = webServerService;

        CreateCursorResources();
        RegisterRoutes();
        RegisterControllers();
    }

    private void RegisterRoutes()
    {
        _router.Routes.AddRange(Routing.Routes.ArtemisRoutes);
    }

    private void CreateCursorResources()
    {
        ICursorProvider? cursorProvider = _container.Resolve<ICursorProvider>(IfUnresolved.ReturnDefault);
        if (cursorProvider == null)
            return;

        Application.Current?.Resources.Add("RotateCursor", cursorProvider.Rotate);
        Application.Current?.Resources.Add("DragCursor", cursorProvider.Drag);
        Application.Current?.Resources.Add("DragHorizontalCursor", cursorProvider.DragHorizontal);
    }

    public void RegisterBuiltInDataModelDisplays()
    {
        _dataModelUIService.RegisterDataModelDisplay<SKColorDataModelDisplayViewModel>(Constants.CorePlugin);
    }

    public void RegisterBuiltInDataModelInputs()
    {
    }

    public void RegisterBuiltInPropertyEditors()
    {
        if (_registeredBuiltInPropertyEditors)
            return;

        _propertyInputService.RegisterPropertyInput<BrushPropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<ColorGradientPropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<FloatPropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<IntPropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<SKColorPropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<SKPointPropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<SKSizePropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput(typeof(EnumPropertyInputViewModel<>), Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<BoolPropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<FloatRangePropertyInputViewModel>(Constants.CorePlugin);
        _propertyInputService.RegisterPropertyInput<IntRangePropertyInputViewModel>(Constants.CorePlugin);

        _registeredBuiltInPropertyEditors = true;
    }

    public void RegisterControllers()
    {
        _webServerService.AddController<RemoteController>(Constants.CorePlugin.Features.First().Instance!, "remote");
    }
}