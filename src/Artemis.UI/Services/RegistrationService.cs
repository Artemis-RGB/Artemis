using System;
using System.Collections;
using System.Linq;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.Controllers;
using Artemis.UI.DefaultTypes.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.DefaultTypes.DataModel.Display;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Services;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using Artemis.VisualScripting.Nodes.Mathematics;
using Avalonia;
using Ninject;
using SkiaSharp;

namespace Artemis.UI.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IDataModelUIService _dataModelUIService;
    private readonly IInputService _inputService;
    private readonly IKernel _kernel;
    private readonly INodeService _nodeService;
    private readonly IPropertyInputService _propertyInputService;
    private readonly IWebServerService _webServerService;
    private bool _registeredBuiltInPropertyEditors;

    public RegistrationService(IKernel kernel,
        IInputService inputService,
        IPropertyInputService propertyInputService,
        IProfileEditorService profileEditorService,
        INodeService nodeService,
        IDataModelUIService dataModelUIService,
        IWebServerService webServerService,
        IDeviceLayoutService deviceLayoutService // here to make sure it is instantiated
    )
    {
        _kernel = kernel;
        _inputService = inputService;
        _propertyInputService = propertyInputService;
        _nodeService = nodeService;
        _dataModelUIService = dataModelUIService;
        _webServerService = webServerService;

        CreateCursorResources();
        RegisterBuiltInNodeTypes();
        RegisterControllers();
    }

    private void CreateCursorResources()
    {
        ICursorProvider? cursorProvider = _kernel.TryGet<ICursorProvider>();
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
        _webServerService.AddController<RemoteController>(Constants.CorePlugin.Features.First().Instance!);
    }

    public void RegisterBuiltInNodeTypes()
    {
        _nodeService.RegisterTypeColor(Constants.CorePlugin, typeof(bool), new SKColor(0xFFCD3232));
        _nodeService.RegisterTypeColor(Constants.CorePlugin, typeof(string), new SKColor(0xFFFFD700));
        _nodeService.RegisterTypeColor(Constants.CorePlugin, typeof(Numeric), new SKColor(0xFF32CD32));
        _nodeService.RegisterTypeColor(Constants.CorePlugin, typeof(float), new SKColor(0xFFFF7C00));
        _nodeService.RegisterTypeColor(Constants.CorePlugin, typeof(SKColor), new SKColor(0xFFAD3EED));
        _nodeService.RegisterTypeColor(Constants.CorePlugin, typeof(IList), new SKColor(0xFFED3E61));
        _nodeService.RegisterTypeColor(Constants.CorePlugin, typeof(Enum), new SKColor(0xFF1E90FF));

        foreach (Type nodeType in typeof(SumNumericsNode).Assembly.GetTypes().Where(t => typeof(INode).IsAssignableFrom(t) && t.IsPublic && !t.IsAbstract && !t.IsInterface))
            _nodeService.RegisterNodeType(Constants.CorePlugin, nodeType);
    }
}