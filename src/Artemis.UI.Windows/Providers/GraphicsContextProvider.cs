using System.Collections.Generic;
using Artemis.Core.Services;
using Artemis.Core.SkiaSharp;
using Artemis.UI.Windows.SkiaSharp;

namespace Artemis.UI.Windows.Providers;

public class GraphicsContextProvider : IGraphicsContextProvider
{
    private VulkanContext? _vulkanContext;

    /// <inheritdoc />
    public IReadOnlyCollection<string> GraphicsContextNames => new List<string> {"Vulkan"}.AsReadOnly();

    /// <inheritdoc />
    public IManagedGraphicsContext? GetGraphicsContext(string name)
    {
        if (name == "Vulkan")
        {
            _vulkanContext ??= new VulkanContext();
            return _vulkanContext;
        }

        return null;
    }
}