using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Services;
using Artemis.UI.DefaultTypes.PropertyInput;
using Artemis.UI.Services.Interfaces;
using Artemis.UI.Shared.Providers;
using Artemis.UI.Shared.Services.ProfileEditor;
using Artemis.UI.Shared.Services.PropertyInput;
using Avalonia;
using DynamicData;
using Ninject;

namespace Artemis.UI.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IKernel _kernel;
    private readonly IInputService _inputService;
    private readonly IPropertyInputService _propertyInputService;
    private bool _registeredBuiltInPropertyEditors;

    public RegistrationService(IKernel kernel, IInputService inputService, IPropertyInputService propertyInputService, IProfileEditorService profileEditorService, IEnumerable<IToolViewModel> toolViewModels)
    {
        _kernel = kernel;
        _inputService = inputService;
        _propertyInputService = propertyInputService;

        profileEditorService.Tools.AddRange(toolViewModels);
        CreateCursorResources();
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
    }

    public void ApplyPreferredGraphicsContext()
    {
    }
}