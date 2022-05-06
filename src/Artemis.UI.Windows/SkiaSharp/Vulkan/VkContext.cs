using System;
using SharpVk;
using SharpVk.Khronos;
using SkiaSharp;

namespace Artemis.UI.Windows.SkiaSharp.Vulkan;

internal class VkContext : IDisposable
{
    public virtual Instance Instance { get; protected set; }

    public virtual PhysicalDevice PhysicalDevice { get; protected set; }

    public virtual Surface Surface { get; protected set; }

    public virtual Device Device { get; protected set; }

    public virtual Queue GraphicsQueue { get; protected set; }

    public virtual Queue PresentQueue { get; protected set; }

    public virtual uint GraphicsFamily { get; protected set; }

    public virtual uint PresentFamily { get; protected set; }

    public virtual GRVkGetProcedureAddressDelegate GetProc { get; protected set; }

    public virtual GRSharpVkGetProcedureAddressDelegate SharpVkGetProc { get; protected set; }

    public virtual void Dispose()
    {
        Instance?.Dispose();
    }
}