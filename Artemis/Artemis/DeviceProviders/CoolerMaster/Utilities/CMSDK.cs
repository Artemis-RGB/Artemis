using System.Runtime.InteropServices;

namespace Artemis.DeviceProviders.CoolerMaster.Utilities
{
    public struct KEY_COLOR
    {
        public byte r;
        public byte g;
        public byte b;

        public KEY_COLOR(byte colR, byte colG, byte colB)
        {
            r = colR;
            g = colG;
            b = colB;
        }
    }

    public struct COLOR_MATRIX
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 132)] public KEY_COLOR[,] KeyColor;
    }

    //Enumeration of device list
    public enum DEVICE_INDEX
    {
        DEV_MKeys_L = 0,
        DEV_MKeys_S = 1,
        DEV_MKeys_L_White = 2,
        DEV_MKeys_M_White = 3,
        DEV_MMouse_L = 4,
        DEV_MMouse_S = 5,
        DEV_MKeys_M = 6,
        DEV_MKeys_S_White = 7
    }

    //Enumeration of device layout
    public enum LAYOUT_KEYBOARD
    {
        LAYOUT_UNINIT = 0,
        LAYOUT_US = 1,
        LAYOUT_EU = 2
    }

    public static class CmSdk
    {
        /// <summary>
        ///     Sets the control device which all following actions are targetted to
        /// </summary>
        /// <param name="devIndex"></param>
        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetControlDevice(DEVICE_INDEX devIndex);

        /// <summary>
        ///     Obtain current device layout
        /// </summary>
        /// <returns></returns>
        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern LAYOUT_KEYBOARD GetDeviceLayout();

        /// <summary>
        ///     Verify if the currently conrolled device is plugged in
        /// </summary>
        /// <returns></returns>
        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool IsDevicePlug();

        /// <summary>
        ///     Enables led control on the currently controlled device
        /// </summary>
        /// <param name="bEnable"></param>
        /// <returns></returns>
        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool EnableLedControl(bool bEnable);

        /// <summary>
        ///     Sets the LED of the currently controlled device
        /// </summary>
        /// <param name="iRow"></param>
        /// <param name="iColumn"></param>
        /// <param name="r"></param>
        /// <param name="g"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetLedColor(int iRow, int iColumn, byte r, byte g, byte b);

        /// <summary>
        ///     Sets all LEDS using the given color matrix
        /// </summary>
        /// <param name="colorMatrix"></param>
        /// <returns></returns>
        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        public static extern bool SetAllLedColor(COLOR_MATRIX colorMatrix);
    }
}
