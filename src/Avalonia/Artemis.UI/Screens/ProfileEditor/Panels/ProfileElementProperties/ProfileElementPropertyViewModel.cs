using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services.ProfileEditor;

namespace Artemis.UI.Screens.ProfileEditor.ProfileElementProperties;

public class ProfileElementPropertyViewModel
{
    private readonly ILayerPropertyVmFactory _layerPropertyVmFactory;
    private readonly IProfileEditorService _profileEditorService;

    public ProfileElementPropertyViewModel(ILayerProperty layerProperty, IProfileEditorService profileEditorService, ILayerPropertyVmFactory layerPropertyVmFactory)
    {
        LayerProperty = layerProperty;
        _profileEditorService = profileEditorService;
        _layerPropertyVmFactory = layerPropertyVmFactory;
    }

    public ILayerProperty LayerProperty { get; }
}