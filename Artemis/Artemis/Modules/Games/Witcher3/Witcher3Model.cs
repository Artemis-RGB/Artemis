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

        public Witcher3Model(MainManager mainManager, Witcher3Settings settings)
            : base(mainManager, settings, new TheWitcherDataModel())
        {
            Name = "Witcher3";
            ProcessName = "witcher3";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            _updateSw = new Stopwatch();
            _signRegex = new Regex("ActiveSign=(.*)", RegexOptions.Compiled);
        }

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
            var gameDataModel = (TheWitcherDataModel) GameDataModel;
            // Witcher effect is very static and reads from disk, don't want to update too often.
            if (_updateSw.ElapsedMilliseconds < 500)
                return;
            _updateSw.Restart();

            if (_witcherSettings == null)
                return;

            var reader = new StreamReader(
                File.Open(_witcherSettings, FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            var configContent = reader.ReadToEnd();
            reader.Close();

            var signRes = _signRegex.Match(configContent);
            if (signRes.Groups.Count < 2)
                return;
            var sign = signRes.Groups[1].Value;

            switch (sign)
            {
                case "ST_Aard\r":
                    gameDataModel.WitcherSign = WitcherSign.Aard;
                    break;
                case "ST_Yrden\r":
                    gameDataModel.WitcherSign = WitcherSign.Yrden;
                    break;
                case "ST_Igni\r":
                    gameDataModel.WitcherSign = WitcherSign.Igni;
                    break;
                case "ST_Quen\r":
                    gameDataModel.WitcherSign = WitcherSign.Quen;
                    break;
                case "ST_Axii\r":
                    gameDataModel.WitcherSign = WitcherSign.Axii;
                    break;
            }
        }

        public override Bitmap GenerateBitmap()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            var keyboardRect = MainManager.KeyboardManager.ActiveKeyboard.KeyboardRectangle(Scale);
            return Profile.GenerateBitmap<TheWitcherDataModel>(keyboardRect, GameDataModel);
        }
    }
}