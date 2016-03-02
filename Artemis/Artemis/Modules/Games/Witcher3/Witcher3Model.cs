using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text.RegularExpressions;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Utilities.Keyboard;

namespace Artemis.Modules.Games.Witcher3
{
    public class Witcher3Model : GameModel
    {
        private readonly Regex _signRegex;
        private readonly Stopwatch _updateSw;
        private KeyboardRectangle _signRect;
        private string _witcherSettings;

        public Witcher3Model(MainManager mainManager, Witcher3Settings settings) : base(mainManager)
        {
            Settings = settings;
            Name = "Witcher3";
            ProcessName = "witcher3";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            _updateSw = new Stopwatch();
            _signRegex = new Regex("ActiveSign=(.*)", RegexOptions.Compiled);
        }

        public Witcher3Settings Settings { get; set; }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            _witcherSettings = null;

            _updateSw.Reset();
        }

        public override void Enable()
        {
            Initialized = false;

            _signRect = new KeyboardRectangle(MainManager.KeyboardManager.ActiveKeyboard, 0, 0, new List<Color>(),
                LinearGradientMode.Horizontal)
            {
                Rotate = true,
                LoopSpeed = 0.5
            };

            // Ensure the config file is found
            var witcherSettings = Environment.GetFolderPath(Environment.SpecialFolder.Personal) +
                                  @"\The Witcher 3\user.settings";
            if (File.Exists(witcherSettings))
                _witcherSettings = witcherSettings;

            _updateSw.Start();

            Initialized = true;
        }

        public override void Update()
        {
            // Witcher effect is very static and reads from disk, don't want to update too often.
            if (_updateSw.ElapsedMilliseconds < 500)
                return;
            _updateSw.Restart();

            if (_witcherSettings == null)
                return;

            var reader = new StreamReader(File.Open(_witcherSettings,
                FileMode.Open,
                FileAccess.Read,
                FileShare.ReadWrite));
            var configContent = reader.ReadToEnd();
            reader.Close();

            var signRes = _signRegex.Match(configContent);
            if (signRes.Groups.Count < 2)
                return;
            var sign = signRes.Groups[1].Value;

            switch (sign)
            {
                case "ST_Aard\r":
                    _signRect.Colors = new List<Color> {Color.DeepSkyBlue, Color.Blue};
                    break;
                case "ST_Yrden\r":
                    _signRect.Colors = new List<Color> {Color.Purple, Color.DeepPink};
                    break;
                case "ST_Igni\r":
                    _signRect.Colors = new List<Color> {Color.DarkOrange, Color.Red};
                    break;
                case "ST_Quen\r":
                    _signRect.Colors = new List<Color> {Color.DarkOrange, Color.FromArgb(232, 193, 0)};
                    break;
                case "ST_Axii\r":
                    _signRect.Colors = new List<Color> {Color.LawnGreen, Color.DarkGreen};
                    break;
            }
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = MainManager.KeyboardManager.ActiveKeyboard.KeyboardBitmap(Scale);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                _signRect.Draw(g);
            }
            return bitmap;
        }
    }
}