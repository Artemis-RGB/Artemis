using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Utilities.DataReaders;
using Caliburn.Micro;
using Newtonsoft.Json;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchModel : GameModel
    {
        private DateTime _characterChange;
        private DateTime _ultimateReady;
        private DateTime _ultimateUsed;

        public OverwatchModel(IEventAggregator events, MainManager mainManager, OverwatchSettings settings)
            : base(mainManager, settings, new OverwatchDataModel())
        {
            Name = "Overwatch";
            ProcessName = "Overwatch";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            MmfReader = new MmfReader("overwatchMmf", MainManager.Logger);
            LoadOverwatchCharacters();
        }

        public OverwatchModel(MainManager mainManager, GameSettings settings, IDataModel dataModel)
            : base(mainManager, settings, dataModel)
        {
        }

        public List<CharacterColor> OverwatchCharacters { get; set; }

        public MmfReader MmfReader { get; set; }

        public int Scale { get; set; }

        private void LoadOverwatchCharacters()
        {
            OverwatchCharacters = new List<CharacterColor>
            {
                new CharacterColor {Character = OverwatchCharacter.Genji, Color = Color.FromRgb(13, 61, 0)},
                new CharacterColor {Character = OverwatchCharacter.Mccree, Color = Color.FromRgb(24, 1, 1)},
                new CharacterColor {Character = OverwatchCharacter.Pharah, Color = Color.FromRgb(0, 6, 32)},
                new CharacterColor {Character = OverwatchCharacter.Reaper, Color = Color.FromRgb(7, 0, 0)},
                new CharacterColor {Character = OverwatchCharacter.Soldier76, Color = Color.FromRgb(3, 5, 11)},
                new CharacterColor {Character = OverwatchCharacter.Tracer, Color = Color.FromRgb(46, 12, 0)},
                new CharacterColor {Character = OverwatchCharacter.Bastion, Color = Color.FromRgb(6, 10, 5)},
                new CharacterColor {Character = OverwatchCharacter.Hanzo, Color = Color.FromRgb(28, 24, 8)},
                new CharacterColor {Character = OverwatchCharacter.Junkrat, Color = Color.FromRgb(59, 28, 0)},
                new CharacterColor {Character = OverwatchCharacter.Mei, Color = Color.FromRgb(3, 20, 55)},
                new CharacterColor {Character = OverwatchCharacter.Torbjörn, Color = Color.FromRgb(31, 4, 3)},
                new CharacterColor {Character = OverwatchCharacter.Widowmaker, Color = Color.FromRgb(16, 3, 17)},
                new CharacterColor {Character = OverwatchCharacter.Dva, Color = Color.FromRgb(62, 12, 32)},
                new CharacterColor {Character = OverwatchCharacter.Reinhardt, Color = Color.FromRgb(12, 16, 16)},
                new CharacterColor {Character = OverwatchCharacter.Roadhog, Color = Color.FromRgb(26, 10, 0)},
                new CharacterColor {Character = OverwatchCharacter.Winston, Color = Color.FromRgb(17, 18, 26)},
                new CharacterColor {Character = OverwatchCharacter.Zarya, Color = Color.FromRgb(58, 7, 24)},
                new CharacterColor {Character = OverwatchCharacter.Lúcio, Color = Color.FromRgb(8, 35, 0)},
                new CharacterColor {Character = OverwatchCharacter.Mercy, Color = Color.FromRgb(60, 56, 26)},
                new CharacterColor {Character = OverwatchCharacter.Symmetra, Color = Color.FromRgb(11, 29, 37)},
                new CharacterColor {Character = OverwatchCharacter.Zenyatta, Color = Color.FromRgb(62, 54, 6)}
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
            var gameDataModel = (OverwatchDataModel) DataModel;
            var colors = MmfReader.GetColorArray();
            if (colors == null)
                return;

            MainManager.Logger.Trace("DataModel: \r\n{0}",
                JsonConvert.SerializeObject(gameDataModel, Formatting.Indented));

            // Determine general game state
            ParseGameSate(gameDataModel, colors);

            // Parse the lighting
            var characterMatch = ParseCharacter(gameDataModel, colors);

            // Ult can't possibly be ready within 2 seconds of changing, this avoids false positives.
            // Filtering on ultready and ultused removes false positives from the native ultimate effects
            if (_characterChange.AddSeconds(2) >= DateTime.Now ||
                _ultimateUsed.AddSeconds(2) >= DateTime.Now ||
                _ultimateReady.AddSeconds(2) >= DateTime.Now)
                return;

            ParseUltimate(gameDataModel, characterMatch, colors);
            ParseAbility1(gameDataModel, colors);
            ParseAbility2(gameDataModel, colors);
        }

        private void ParseGameSate(OverwatchDataModel gameDataModel, Color[,] colors)
        {
            if (_ultimateUsed.AddSeconds(5) >= DateTime.Now)
                return;
            gameDataModel.Status = colors[0, 0].Equals(Color.FromRgb(55, 30, 0))
                ? OverwatchStatus.InMainMenu
                : OverwatchStatus.Unkown;

            if (gameDataModel.Status != OverwatchStatus.InMainMenu)
                return;

            gameDataModel.Character = OverwatchCharacter.None;
            gameDataModel.Ability1Ready = false;
            gameDataModel.Ability2Ready = false;
            gameDataModel.UltimateReady = false;
            gameDataModel.UltimateUsed = false;
        }

        private CharacterColor? ParseCharacter(OverwatchDataModel gameDataModel, Color[,] colors)
        {
            var characterMatch = new CharacterColor {Character = OverwatchCharacter.None};

            // Scan an entire row of keys, minimizing the chance of misreading it due to animations
            // Could read entire set of arrays but this is cheaper
            for (var i = 0; i < 22; i++)
            {
                characterMatch = OverwatchCharacters.FirstOrDefault(c => c.Color == colors[1, i]);
                if (characterMatch.Character != OverwatchCharacter.None)
                    break;
            }

            if (_ultimateReady.AddSeconds(2) >= DateTime.Now && characterMatch.Character == OverwatchCharacter.None)
                return null;

            if (characterMatch.Character != gameDataModel.Character)
                _characterChange = DateTime.Now;

            if (characterMatch.Character == OverwatchCharacter.None)
                return characterMatch;

            // Only update the character if it's not none
            // Status will remain on MainMenu/Unknown if this code is not reached
            gameDataModel.Character = characterMatch.Character;
            gameDataModel.Status = OverwatchStatus.InGame;

            return characterMatch;
        }

        private void ParseUltimate(OverwatchDataModel gameDataModel, CharacterColor? characterMatch, Color[,] colors)
        {
            if (characterMatch == null)
                return;

            // Ultimate is ready when Q is blinking
            var ultReady = !characterMatch.Value.Color.Equals(colors[2, 2]);
            if (!gameDataModel.UltimateReady && ultReady && _ultimateUsed.AddSeconds(15) <= DateTime.Now)
            {
                _ultimateReady = DateTime.Now;
                gameDataModel.UltimateReady = true;
            }

            // If ult no longer ready but it was ready before, it was used.
            if (gameDataModel.UltimateReady && !ultReady)
            {
                gameDataModel.UltimateReady = false;
                if (_ultimateUsed.AddSeconds(15) <= DateTime.Now)
                    _ultimateUsed = DateTime.Now;
            }

            // UltimateUsed is true for 10 seconds after ultimate went on cooldown
            if (_ultimateUsed != DateTime.MinValue)
                gameDataModel.UltimateUsed = _ultimateUsed.AddSeconds(10) >= DateTime.Now;
        }

        private void ParseAbility1(OverwatchDataModel gameDataModel, Color[,] colors)
        {
            gameDataModel.Ability1Ready = colors[4, 1].Equals(Color.FromRgb(4, 141, 144));
        }

        private void ParseAbility2(OverwatchDataModel gameDataModel, Color[,] colors)
        {
            gameDataModel.Ability2Ready = colors[2, 4].Equals(Color.FromRgb(4, 141, 144));
        }

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return Profile.GetRenderLayers<OverwatchDataModel>(DataModel, renderMice, renderHeadsets);
        }
    }

    public struct CharacterColor
    {
        public OverwatchCharacter Character { get; set; }
        public Color Color { get; set; }
    }
}