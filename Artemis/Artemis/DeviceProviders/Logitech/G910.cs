using System;
using System.Drawing;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using Artemis.DAL;
using Artemis.DeviceProviders.Logitech.Utilities;
using Artemis.Properties;
using Artemis.Settings;

namespace Artemis.DeviceProviders.Logitech
{
    public class G910 : LogitechKeyboard
    {
        private readonly GeneralSettings _generalSettings;

        public G910()
        {
            Name = "Logitech G910 RGB";
            Slug = "logitech-g910";
            CantEnableText = "Couldn't connect to your Logitech G910.\n" +
                             "Please check your cables and updating the Logitech Gaming Software\n" +
                             "A minimum version of 8.81.15 is required.\n\n" +
                             "If needed, you can select a different keyboard in Artemis under settings.";
            Height = 7;
            Width = 22;
            PreviewSettings = new PreviewSettings(new Rect(34, 18, 916, 272), Resources.g910);
            _generalSettings = SettingsProvider.Load<GeneralSettings>();
        }

        public override KeyMatch? GetKeyPosition(Keys keyCode)
        {
            KeyMatch value;
            switch (_generalSettings.Layout)
            {
                case "Qwerty":
                    value = KeyMap.QwertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                    break;
                case "Qwertz":
                    value = KeyMap.QwertzLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                    break;
                default:
                    value = KeyMap.AzertyLayout.FirstOrDefault(k => k.KeyCode == keyCode);
                    break;
            }

            // Adjust the distance by 1 on both x and y for the G910
            return new KeyMatch(value.KeyCode, value.X + 1, value.Y + 1);
        }

        /// <summary>
        ///     The G910 also updates the G-logo, G-badge and G-keys
        /// </summary>
        /// <param name="bitmap"></param>
        public override void DrawBitmap(Bitmap bitmap)
        {
            using (var croppedBitmap = new Bitmap(21 * 4, 6 * 4))
            {
                // Deal with non-standard DPI
                croppedBitmap.SetResolution(96, 96);
                // Don't forget that the image is upscaled 4 times
                using (var g = Graphics.FromImage(croppedBitmap))
                {
                    g.DrawImage(bitmap, new Rectangle(0, 0, 84, 24), new Rectangle(4, 4, 84, 24), GraphicsUnit.Pixel);
                }

                LogitechGSDK.LogiLedSetTargetDevice(LogitechGSDK.LOGI_DEVICETYPE_PERKEY_RGB);
                LogitechGSDK.LogiLedSetLightingFromBitmap(OrionUtilities.BitmapToByteArray(bitmap, G910Keymappings));
            }

            using (var resized = OrionUtilities.ResizeImage(bitmap, 22, 7))
            {
                // Color the extra keys on the left side of the keyboard
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_LOGO, 0, 1);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_1, 0, 2);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_2, 0, 3);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_3, 0, 4);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_4, 0, 5);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_5, 0, 6);

                // Color the extra keys on the top of the keyboard
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_6, 3, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_7, 4, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_8, 5, 0);
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_9, 6, 0);

                // Color the G-badge
                SetLogitechColorFromCoordinates(resized, KeyboardNames.G_BADGE, 5, 6);
            }
        }

        // These mappings are used by the G910 to fix alignments 
        public static OrionUtilities.KeyMapping[] G910Keymappings =
        {
            // First row
            new OrionUtilities.KeyMapping(0, 0),
            new OrionUtilities.KeyMapping(1, 1),
            new OrionUtilities.KeyMapping(2, 1),
            new OrionUtilities.KeyMapping(3, 2),
            new OrionUtilities.KeyMapping(4, 3),
            new OrionUtilities.KeyMapping(5, 4),
            new OrionUtilities.KeyMapping(6, 5),
            new OrionUtilities.KeyMapping(7, 6),
            new OrionUtilities.KeyMapping(8, 7),
            new OrionUtilities.KeyMapping(9, 8),
            new OrionUtilities.KeyMapping(10, 9),
            new OrionUtilities.KeyMapping(11, 9),
            new OrionUtilities.KeyMapping(12, 10),
            new OrionUtilities.KeyMapping(13, 11),
            new OrionUtilities.KeyMapping(13, 12),
            new OrionUtilities.KeyMapping(14, 13),
            new OrionUtilities.KeyMapping(15, 14),
            new OrionUtilities.KeyMapping(16, 15),
            new OrionUtilities.KeyMapping(17, 16),
            new OrionUtilities.KeyMapping(18, 17),
            new OrionUtilities.KeyMapping(19, 18),

            // Second row
            new OrionUtilities.KeyMapping(21, 21),
            new OrionUtilities.KeyMapping(22, 22),
            new OrionUtilities.KeyMapping(23, 23),
            new OrionUtilities.KeyMapping(24, 24),
            new OrionUtilities.KeyMapping(25, 25),
            new OrionUtilities.KeyMapping(26, 26),
            new OrionUtilities.KeyMapping(27, 27),
            new OrionUtilities.KeyMapping(28, 28),
            new OrionUtilities.KeyMapping(29, 29),
            new OrionUtilities.KeyMapping(30, 30),
            new OrionUtilities.KeyMapping(31, 31),
            new OrionUtilities.KeyMapping(32, 32),
            new OrionUtilities.KeyMapping(33, 33),
            new OrionUtilities.KeyMapping(34, 34),
            new OrionUtilities.KeyMapping(35, 35),
            new OrionUtilities.KeyMapping(36, 36),
            new OrionUtilities.KeyMapping(37, 37),
            new OrionUtilities.KeyMapping(38, 38),
            new OrionUtilities.KeyMapping(39, 39),
            new OrionUtilities.KeyMapping(40, 40),
            new OrionUtilities.KeyMapping(41, 41),

            // Third row
            new OrionUtilities.KeyMapping(42, 42),
            new OrionUtilities.KeyMapping(43, 43),
            new OrionUtilities.KeyMapping(44, 44),
            new OrionUtilities.KeyMapping(45, 45),
            new OrionUtilities.KeyMapping(46, 46),
            new OrionUtilities.KeyMapping(47, 46),
            new OrionUtilities.KeyMapping(48, 47),
            new OrionUtilities.KeyMapping(49, 48),
            new OrionUtilities.KeyMapping(50, 49),
            new OrionUtilities.KeyMapping(51, 50),
            new OrionUtilities.KeyMapping(52, 51),
            new OrionUtilities.KeyMapping(53, 52),
            new OrionUtilities.KeyMapping(54, 53),
            new OrionUtilities.KeyMapping(54, 54),
            new OrionUtilities.KeyMapping(55, 55),
            new OrionUtilities.KeyMapping(56, 56),
            new OrionUtilities.KeyMapping(57, 57),
            new OrionUtilities.KeyMapping(58, 58),
            new OrionUtilities.KeyMapping(59, 59),
            new OrionUtilities.KeyMapping(60, 60),
            new OrionUtilities.KeyMapping(61, 61),
            new OrionUtilities.KeyMapping(62, 62),

            // Fourth row
            new OrionUtilities.KeyMapping(63, 63),
            new OrionUtilities.KeyMapping(64, 64),
            new OrionUtilities.KeyMapping(65, 65),
            new OrionUtilities.KeyMapping(66, 65),
            new OrionUtilities.KeyMapping(67, 66),
            new OrionUtilities.KeyMapping(68, 67),
            new OrionUtilities.KeyMapping(69, 68),
            new OrionUtilities.KeyMapping(70, 69),
            new OrionUtilities.KeyMapping(71, 70),
            new OrionUtilities.KeyMapping(72, 71),
            new OrionUtilities.KeyMapping(73, 72),
            new OrionUtilities.KeyMapping(74, 73),
            new OrionUtilities.KeyMapping(75, 74),
            new OrionUtilities.KeyMapping(76, 75),
            new OrionUtilities.KeyMapping(76, 76),
            new OrionUtilities.KeyMapping(78, 77),
            new OrionUtilities.KeyMapping(79, 78),
            new OrionUtilities.KeyMapping(79, 79),
            new OrionUtilities.KeyMapping(80, 80),
            new OrionUtilities.KeyMapping(81, 81),
            new OrionUtilities.KeyMapping(82, 82),

            // Fifth row
            new OrionUtilities.KeyMapping(84, 84),
            new OrionUtilities.KeyMapping(85, 85),
            new OrionUtilities.KeyMapping(86, 86),
            new OrionUtilities.KeyMapping(87, 87),
            new OrionUtilities.KeyMapping(88, 88),
            new OrionUtilities.KeyMapping(89, 89),
            new OrionUtilities.KeyMapping(90, 90),
            new OrionUtilities.KeyMapping(91, 91),
            new OrionUtilities.KeyMapping(92, 92),
            new OrionUtilities.KeyMapping(93, 93),
            new OrionUtilities.KeyMapping(94, 94),
            new OrionUtilities.KeyMapping(95, 95),
            new OrionUtilities.KeyMapping(96, 96),
            new OrionUtilities.KeyMapping(97, 97),
            new OrionUtilities.KeyMapping(98, 98),
            new OrionUtilities.KeyMapping(99, 99),
            new OrionUtilities.KeyMapping(100, 100),
            new OrionUtilities.KeyMapping(101, 101),
            new OrionUtilities.KeyMapping(102, 102),
            new OrionUtilities.KeyMapping(103, 103),
            new OrionUtilities.KeyMapping(104, 104),

            // Sixth row
            new OrionUtilities.KeyMapping(105, 105),
            new OrionUtilities.KeyMapping(106, 106),
            new OrionUtilities.KeyMapping(107, 107),
            new OrionUtilities.KeyMapping(108, 107),
            new OrionUtilities.KeyMapping(109, 109),
            new OrionUtilities.KeyMapping(110, 110),
            new OrionUtilities.KeyMapping(111, 110),
            new OrionUtilities.KeyMapping(112, 111),
            new OrionUtilities.KeyMapping(113, 112),
            new OrionUtilities.KeyMapping(114, 113),
            new OrionUtilities.KeyMapping(115, 114),
            new OrionUtilities.KeyMapping(116, 115),
            new OrionUtilities.KeyMapping(115, 116), // ALTGR
            new OrionUtilities.KeyMapping(116, 117),
            new OrionUtilities.KeyMapping(117, 118),
            new OrionUtilities.KeyMapping(118, 119),
            new OrionUtilities.KeyMapping(119, 120),
            new OrionUtilities.KeyMapping(120, 121),
            new OrionUtilities.KeyMapping(121, 122),
            new OrionUtilities.KeyMapping(122, 123),
            new OrionUtilities.KeyMapping(124, 124)
        };
    }
}
