using System.Runtime.InteropServices;
using System.Text;

namespace Artemis.DeviceProviders.Logitech.Utilities
{
    public class LogitechGSDK
    {
        //LED SDK
        private const int LOGI_DEVICETYPE_MONOCHROME_ORD = 0;
        private const int LOGI_DEVICETYPE_RGB_ORD = 1;
        private const int LOGI_DEVICETYPE_PERKEY_RGB_ORD = 2;

        public const int LOGI_DEVICETYPE_MONOCHROME = 1 << LOGI_DEVICETYPE_MONOCHROME_ORD;
        public const int LOGI_DEVICETYPE_RGB = 1 << LOGI_DEVICETYPE_RGB_ORD;
        public const int LOGI_DEVICETYPE_PERKEY_RGB = 1 << LOGI_DEVICETYPE_PERKEY_RGB_ORD;
        public const int LOGI_LED_BITMAP_WIDTH = 21;
        public const int LOGI_LED_BITMAP_HEIGHT = 6;
        public const int LOGI_LED_BITMAP_BYTES_PER_KEY = 4;

        public const int LOGI_LED_BITMAP_SIZE =
            LOGI_LED_BITMAP_WIDTH*LOGI_LED_BITMAP_HEIGHT*LOGI_LED_BITMAP_BYTES_PER_KEY;

        public const int LOGI_LED_DURATION_INFINITE = 0;

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedInit();

        //Config option functions
        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionNumber([MarshalAs(UnmanagedType.LPWStr)] string configPath,
            ref double defaultNumber);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionBool([MarshalAs(UnmanagedType.LPWStr)] string configPath,
            ref bool defaultRed);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionColor([MarshalAs(UnmanagedType.LPWStr)] string configPath,
            ref int defaultRed, ref int defaultGreen, ref int defaultBlue);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetConfigOptionKeyInput([MarshalAs(UnmanagedType.LPWStr)] string configPath,
            StringBuilder buffer, int bufsize);

        /////////////////////

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetTargetDevice(int targetDevice);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedGetSdkVersion(ref int majorNum, ref int minorNum, ref int buildNum);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSaveCurrentLighting();

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLighting(int redPercentage, int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedRestoreLighting();

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedFlashLighting(int redPercentage, int greenPercentage, int bluePercentage,
            int milliSecondsDuration, int milliSecondsInterval);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedPulseLighting(int redPercentage, int greenPercentage, int bluePercentage,
            int milliSecondsDuration, int milliSecondsInterval);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedStopEffects();

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedExcludeKeysFromBitmap(KeyboardNames[] keyList, int listCount);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingFromBitmap(byte[] bitmap);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithScanCode(int keyCode, int redPercentage,
            int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithHidCode(int keyCode, int redPercentage,
            int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithQuartzCode(int keyCode, int redPercentage,
            int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSetLightingForKeyWithKeyName(KeyboardNames keyCode, int redPercentage,
            int greenPercentage, int bluePercentage);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedSaveLightingForKey(KeyboardNames keyName);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedRestoreLightingForKey(KeyboardNames keyName);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedFlashSingleKey(KeyboardNames keyName, int redPercentage, int greenPercentage,
            int bluePercentage, int msDuration, int msInterval);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedPulseSingleKey(KeyboardNames keyName, int startRedPercentage,
            int startGreenPercentage, int startBluePercentage, int finishRedPercentage, int finishGreenPercentage,
            int finishBluePercentage, int msDuration, bool isInfinite);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern bool LogiLedStopEffectsOnKey(KeyboardNames keyName);

        [DllImport("LogitechLedEnginesWrapper ", CallingConvention = CallingConvention.Cdecl)]
        public static extern void LogiLedShutdown();
    }
}