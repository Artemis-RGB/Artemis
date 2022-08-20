using Artemis.Core.Providers;
using Artemis.Core.SkiaSharp;
using Artemis.UI.Windows.SkiaSharp;

namespace Artemis.UI.Windows.Providers;

public class GraphicsContextProvider : IGraphicsContextProvider
{
    private VulkanContext? _vulkanContext;

    public string GraphicsContextName => "Vulkan";

    public IManagedGraphicsContext? GetGraphicsContext()
    {
        _vulkanContext ??= new VulkanContext();
        return _vulkanContext;
    }
}