using System;
using Artemis.Core.SkiaSharp;
using Artemis.UI.Exceptions;
using Artemis.UI.SkiaSharp.Vulkan;
using SkiaSharp;

namespace Artemis.UI.SkiaSharp
{
    public class VulkanContext : IManagedGraphicsContext
    {
        private readonly GRVkBackendContext _vulkanBackendContext;
        private readonly Win32VkContext _vulkanContext;

        public VulkanContext()
        {
            // Try everything in separate try-catch blocks to provide some accuracy in error reporting
            try
            {
                _vulkanContext = new Win32VkContext();
            }
            catch (Exception e)
            {
                throw new ArtemisGraphicsContextException("Failed to create Vulkan context", e);
            }

            try
            {
                _vulkanBackendContext = new GRVkBackendContext
                {
                    VkInstance = (IntPtr) _vulkanContext.Instance.RawHandle.ToUInt64(),
                    VkPhysicalDevice = (IntPtr) _vulkanContext.PhysicalDevice.RawHandle.ToUInt64(),
                    VkDevice = (IntPtr) _vulkanContext.Device.RawHandle.ToUInt64(),
                    VkQueue = (IntPtr) _vulkanContext.GraphicsQueue.RawHandle.ToUInt64(),
                    GraphicsQueueIndex = _vulkanContext.GraphicsFamily,
                    GetProcedureAddress = _vulkanContext.GetProc
                };
            }
            catch (Exception e)
            {
                throw new ArtemisGraphicsContextException("Failed to create Vulkan backend context", e);
            }

            try
            {
                GraphicsContext = GRContext.CreateVulkan(_vulkanBackendContext);
                if (GraphicsContext == null)
                    throw new ArtemisGraphicsContextException("GRContext.CreateVulkan returned null");
            }
            catch (Exception e)
            {
                throw new ArtemisGraphicsContextException("Failed to create Vulkan graphics context", e);
            }

            GraphicsContext.Flush();
        }

        /// <inheritdoc />
        public void Dispose()
        {
        }

        public GRContext GraphicsContext { get; }
    }
}