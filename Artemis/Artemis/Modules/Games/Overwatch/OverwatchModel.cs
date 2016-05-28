using System.Drawing;
using System.IO;
using System.IO.MemoryMappedFiles;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchModel : GameModel
    {
        public OverwatchModel(MainManager mainManager, OverwatchSettings settings)
            : base(mainManager, settings, new OverwatchDataModel())
        {
            Name = "Overwatch";
            ProcessName = "notepad";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
            Initialized = true;
        }

        public override void Update()
        {
            var gameDataModel = (OverwatchDataModel) GameDataModel;
            var mffData = ReadMmf("overwatchMmf");
            if (mffData == null)
                return;
            var data = mffData.Split(' ');
        }

        private string ReadMmf(string overwatchmff)
        {
            try
            {
                // opening not-persistent, pagefile-based memory-mapped file
                using (var mmf = MemoryMappedFile.OpenExisting(overwatchmff))
                {
                    // open the stream to read from the file
                    using (var stream = mmf.CreateViewStream())
                    {
                        // Read from the shared memory, just for this example we know there is a string
                        var reader = new BinaryReader(stream);
                        var res = string.Empty;
                        string str;
                        do
                        {
                            str = reader.ReadString();
                            if (!string.IsNullOrEmpty(str) && str[0] != 0)
                                res = res + str;
                        } while (!string.IsNullOrEmpty(str));
                        return res;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                return null;
                //ignored
            }
        }

        public override Bitmap GenerateBitmap()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            var keyboardRect = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(Scale);
            return Profile.GenerateBitmap<OverwatchDataModel>(keyboardRect, GameDataModel, false, true);
        }

        public override Brush GenerateMouseBrush()
        {
            return Profile?.GenerateBrush<OverwatchDataModel>(GameDataModel, LayerType.Mouse, false, true);
        }

        public override Brush GenerateHeadsetBrush()
        {
            return Profile?.GenerateBrush<OverwatchDataModel>(GameDataModel, LayerType.Headset, false, true);
        }
    }
}