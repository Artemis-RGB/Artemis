using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;
using Caliburn.Micro;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchModel : GameModel
    {
        private readonly IEventAggregator _events;

        public OverwatchModel(IEventAggregator events, MainManager mainManager, OverwatchSettings settings)
            : base(mainManager, settings, new OverwatchDataModel())
        {
            _events = events;
            Name = "Overwatch";
            ProcessName = "Overwatch";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            MmfReader = new MmfReader("overwatchMmf");
            LoadOverwatchCharacters();
        }

        public List<CharacterColor> OverwatchCharacters { get; set; }

        public MmfReader MmfReader { get; set; }

        public int Scale { get; set; }

        private void LoadOverwatchCharacters()
        {
            OverwatchCharacters = new List<CharacterColor>
            {
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Genji, Color = Color.FromRgb(13, 61, 0)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Mccree, Color = Color.FromRgb(24, 1, 1)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Pharah, Color = Color.FromRgb(0, 6, 32)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Reaper, Color = Color.FromRgb(7, 0, 0)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Soldier76, Color = Color.FromRgb(3, 5, 11)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Tracer, Color = Color.FromRgb(46, 12, 0)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Bastion, Color = Color.FromRgb(6, 10, 5)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Hanzo, Color = Color.FromRgb(28, 24, 8)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Junkrat, Color = Color.FromRgb(59, 28, 0)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Mei, Color = Color.FromRgb(3, 20, 55)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Torbjörn, Color = Color.FromRgb(31, 4, 3)},
                new CharacterColor
                {
                    OverwatchCharacter = OverwatchCharacter.Widowmaker,
                    Color = Color.FromRgb(16, 3, 17)
                },
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Dva, Color = Color.FromRgb(62, 12, 32)},
                new CharacterColor
                {
                    OverwatchCharacter = OverwatchCharacter.Reinhardt,
                    Color = Color.FromRgb(12, 16, 16)
                },
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Roadhog, Color = Color.FromRgb(26, 10, 0)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Winston, Color = Color.FromRgb(17, 18, 26)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Zarya, Color = Color.FromRgb(58, 7, 24)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Lúcio, Color = Color.FromRgb(8, 35, 0)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Mercy, Color = Color.FromRgb(60, 56, 26)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Symmetra, Color = Color.FromRgb(11, 29, 37)},
                new CharacterColor {OverwatchCharacter = OverwatchCharacter.Zenyatta, Color = Color.FromRgb(62, 54, 6)}
            };
        }

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
            var colors = MmfReader.GetColorArray();
            if (colors == null)
                return;

            var bitmap = new Bitmap(22, 6);

            using (var g = Graphics.FromImage(bitmap))
            {
                for (var y = 0; y < 6; y++)
                {
                    for (var x = 0; x < 22; x++)
                    {
                        g.DrawRectangle(new Pen(ColorHelpers.ToDrawingColor(colors[y, x])), y, x, 1, 1);
                    }
                }
            }
            _events.PublishOnUIThread(new ChangeBitmap(bitmap));

            // Determine general game state
            gameDataModel.Status = colors[0, 0].Equals(Color.FromRgb(55, 30, 0))
                ? OverwatchStatus.InMainMenu
                : OverwatchStatus.Unkown;

            if (gameDataModel.Status == OverwatchStatus.InMainMenu)
                return;

            // If ingame, look for a character
            var characterMatch = OverwatchCharacters.FirstOrDefault(c => c.Color == colors[0, 0]);
            if (characterMatch.OverwatchCharacter == OverwatchCharacter.None)
                return;

            gameDataModel.Status = OverwatchStatus.InGame;
            gameDataModel.Character = characterMatch.OverwatchCharacter;

            // Ability1 is ready when LShift is lid
            gameDataModel.Ability1Ready = colors[4, 1].Equals(Color.FromRgb(4, 141, 144));
            // Ability2 is ready when E is lid
            gameDataModel.Ability2Ready = colors[2, 4].Equals(Color.FromRgb(4, 141, 144));
            // Ultimate is ready when Q is blinking
            gameDataModel.UltimateReady = !characterMatch.Color.Equals(colors[2, 2]);
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
            if (Profile == null || GameDataModel == null)
                return null;

            return Profile.GenerateBrush<OverwatchDataModel>(GameDataModel, LayerType.Mouse, false, true);
        }

        public override Brush GenerateHeadsetBrush()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            return Profile.GenerateBrush<OverwatchDataModel>(GameDataModel, LayerType.Headset, false, true);
        }
    }

    public struct CharacterColor
    {
        public OverwatchCharacter OverwatchCharacter { get; set; }
        public Color Color { get; set; }
    }
}