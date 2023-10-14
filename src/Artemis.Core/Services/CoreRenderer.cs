using Artemis.Core.ScriptingProviders;
using Artemis.Core.Services.Core;
using SkiaSharp;

namespace Artemis.Core.Services;

internal class CoreRenderer : IRenderer
{
    private readonly IModuleService _moduleService;
    private readonly IScriptingService _scriptingService;
    private readonly IProfileService _profileService;

    public CoreRenderer(IModuleService moduleService, IScriptingService scriptingService, IProfileService profileService)
    {
        _moduleService = moduleService;
        _scriptingService = scriptingService;
        _profileService = profileService;
    }

    /// <inheritdoc />
    public void Render(SKCanvas canvas, double delta)
    {
        foreach (GlobalScript scriptingServiceGlobalScript in _scriptingService.GlobalScripts)
            scriptingServiceGlobalScript.OnCoreUpdating(delta);
        
        _moduleService.UpdateActiveModules(delta);

        if (!_profileService.ProfileRenderingDisabled)
        {
            _profileService.UpdateProfiles(delta);
            _profileService.RenderProfiles(canvas);
        }

        foreach (GlobalScript scriptingServiceGlobalScript in _scriptingService.GlobalScripts)
            scriptingServiceGlobalScript.OnCoreUpdated(delta);
    }

    /// <inheritdoc />
    public void PostRender(SKTexture texture)
    {
    }
}