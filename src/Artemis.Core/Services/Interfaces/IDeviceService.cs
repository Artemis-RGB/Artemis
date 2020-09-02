namespace Artemis.Core.Services
{
    public interface IDeviceService : IArtemisService
    {
        /// <summary>
        ///     Identifies the device by making it blink white 5 times
        /// </summary>
        /// <param name="device"></param>
        void IdentifyDevice(ArtemisDevice device);
    }
}