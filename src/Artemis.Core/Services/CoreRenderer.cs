using Artemis.Core.Services.Core;
using SkiaSharp;

namespace Artemis.Core.Services;

internal class CoreRenderer : IRenderer
{
    private readonly IModuleService _moduleService;
    private readonly IProfileService _profileService;

    public CoreRenderer(IModuleService moduleService, IProfileService profileService)
    {
        _moduleService = moduleService;
        _profileService = profileService;
    }

    /// <inheritdoc />
    public void Render(SKCanvas canvas, double delta)
    {
        _moduleService.UpdateActiveModules(delta);

        if (!_profileService.ProfileRenderingDisabled)
        {
            _profileService.UpdateProfiles(delta);
            _profileService.RenderProfiles(canvas);
        }
    }

    /// <inheritdoc />
    public void PostRender(SKTexture texture)
    {
    }
}