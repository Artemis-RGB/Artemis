using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Artemis.Core;
using Artemis.Core.DataModelExpansions;
using Artemis.Core.Services;
using SkiaSharp;

namespace Artemis.Plugins.Modules.General.DataModels.Windows
{
    public class WindowDataModel
    {
        public WindowDataModel(Process process)
        {
            Process = process;
            WindowTitle = process.MainWindowTitle;
            ProcessName = process.ProcessName;

            // Accessing MainModule requires admin privileges, this way does not
            ProgramLocation = process.GetProcessFilename();

            // Get Icon colors
            if(File.Exists(ProgramLocation)) 
            {
                MemoryStream mem = new MemoryStream();
                Icon.ExtractAssociatedIcon(ProgramLocation).Save(mem);
                mem.Seek(0, SeekOrigin.Begin);
                var skbm = SKBitmap.Decode(mem);
                mem.Close();

                IColorQuantizerService colorQuantizer = new ColorQuantizerService();
                var skClrs = colorQuantizer.Quantize(skbm.Pixels.ToList(), 256).ToList();
                Colors = new IconColorsDataModel 
                {
                    Vibrant = colorQuantizer.FindColorVariation(skClrs, ColorType.Vibrant, true),
                    LightVibrant = colorQuantizer.FindColorVariation(skClrs, ColorType.LightVibrant, true),
                    DarkVibrant = colorQuantizer.FindColorVariation(skClrs, ColorType.DarkVibrant, true),
                    Muted = colorQuantizer.FindColorVariation(skClrs, ColorType.Muted, true),
                    LightMuted = colorQuantizer.FindColorVariation(skClrs, ColorType.LightMuted, true),
                    DarkMuted = colorQuantizer.FindColorVariation(skClrs, ColorType.DarkMuted, true),
                };
            }
        }

        [DataModelIgnore]
        public Process Process { get; }

        public string WindowTitle { get; set; }
        public string ProcessName { get; set; }
        public string ProgramLocation { get; set; }
        public IconColorsDataModel Colors { get; set; }
    }
}