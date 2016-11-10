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

    //  set up/save the whole LED color structure

    public struct COLOR_MATRIX
    {
        [MarshalAsAttribute(UnmanagedType.ByValArray, SizeConst = 132)]
        public KEY_COLOR[,] KeyColor;
    }

    //Enumeration of device list
    public enum DEVICE_INDEX
    {
        DEV_MKeys_L = 0,
        DEV_MKeys_S = 1,
        DEV_MKeys_L_White = 2,
        DEV_MKeys_M_White = 3,
        DEV_MMouse_L = 4
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
        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void SetControlDevice(DEVICE_INDEX devIndex);

        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool EnableLedControl(bool bEnable);

        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetLedColor(int iRow, int iColumn, byte r, byte g, byte b);

        [DllImport("lib\\SDKDLL ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool SetAllLedColor(COLOR_MATRIX colorMatrix);
    }
}