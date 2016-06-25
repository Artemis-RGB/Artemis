using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Interfaces;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;
using Caliburn.Micro;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchModel : GameModel
    {
        private readonly IEventAggregator _events;
        private DateTime _characterChange;
        // Using sticky values on these since they can cause flickering
        private StickyValue<OverwatchStatus> _stickyStatus;
        private StickyValue<bool> _stickyUltimateReady;
        private StickyValue<bool> _stickyUltimateUsed;
        private DateTime _ultimateReady;
        private DateTime _ultimateUsed;

        public OverwatchModel(IEventAggregator events, MainManager mainManager, OverwatchSettings settings)
            : base(mainManager, settings, new OverwatchDataModel())
        {
            _events = events;
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
                new CharacterColor {Character = OverwatchCharacter.Genji, Color = Color.FromRgb(55, 245, 0)},
                new CharacterColor {Character = OverwatchCharacter.Mccree, Color = Color.FromRgb(97, 5, 5)},
                new CharacterColor {Character = OverwatchCharacter.Pharah, Color = Color.FromRgb(0, 24, 128)},
                new CharacterColor {Character = OverwatchCharacter.Reaper, Color = Color.FromRgb(28, 0, 2)},
                new CharacterColor {Character = OverwatchCharacter.Soldier76, Color = Color.FromRgb(14, 21, 45)},
                new CharacterColor {Character = OverwatchCharacter.Tracer, Color = Color.FromRgb(186, 49, 0)},
                new CharacterColor {Character = OverwatchCharacter.Bastion, Color = Color.FromRgb(26, 43, 20)},
                new CharacterColor {Character = OverwatchCharacter.Hanzo, Color = Color.FromRgb(113, 99, 33)},
                new CharacterColor {Character = OverwatchCharacter.Junkrat, Color = Color.FromRgb(237, 113, 2)},
                new CharacterColor {Character = OverwatchCharacter.Mei, Color = Color.FromRgb(15, 82, 222)},
                new CharacterColor {Character = OverwatchCharacter.Torbjörn, Color = Color.FromRgb(125, 18, 12)},
                new CharacterColor {Character = OverwatchCharacter.Widowmaker, Color = Color.FromRgb(65, 12, 70)},
                new CharacterColor {Character = OverwatchCharacter.Dva, Color = Color.FromRgb(248, 48, 129)},
                new CharacterColor {Character = OverwatchCharacter.Reinhardt, Color = Color.FromRgb(51, 65, 66)},
                new CharacterColor {Character = OverwatchCharacter.Roadhog, Color = Color.FromRgb(107, 40, 2)},
                new CharacterColor {Character = OverwatchCharacter.Winston, Color = Color.FromRgb(70, 73, 107)},
                new CharacterColor {Character = OverwatchCharacter.Zarya, Color = Color.FromRgb(235, 28, 97)},
                new CharacterColor {Character = OverwatchCharacter.Lúcio, Color = Color.FromRgb(34, 142, 2)},
                new CharacterColor {Character = OverwatchCharacter.Mercy, Color = Color.FromRgb(243, 226, 106)},
                new CharacterColor {Character = OverwatchCharacter.Symmetra, Color = Color.FromRgb(46, 116, 148)},
                new CharacterColor {Character = OverwatchCharacter.Zenyatta, Color = Color.FromRgb(248, 218, 26)}
            };
        }

        public override void Enable()
        {
            _stickyStatus = new StickyValue<OverwatchStatus>(300);
            _stickyUltimateReady = new StickyValue<bool>(350);
            _stickyUltimateUsed = new StickyValue<bool>(350);
            Initialized = true;
        }

        public override void Dispose()
        {
            _stickyStatus.Dispose();
            _stickyUltimateReady.Dispose();
            _stickyUltimateUsed.Dispose();
            Initialized = false;
        }

        public override void Update()
        {
            UpdateOverwatch();
            ApplyStickyValues();
        }

        private void ApplyStickyValues()
        {
            var gameDataModel = (OverwatchDataModel) DataModel;
            gameDataModel.Status = _stickyStatus.Value;
            gameDataModel.UltimateReady = _stickyUltimateReady.Value;
            gameDataModel.UltimateUsed = _stickyUltimateUsed.Value;
        }

        public void UpdateOverwatch()
        {
            var gameDataModel = (OverwatchDataModel) DataModel;
            var colors = MmfReader.GetColorArray();
            if (colors == null)
                return;

            _events.PublishOnUIThread(new RazerColorArrayChanged(colors));
            //MainManager.Logger.Trace("DataModel: \r\n{0}",
            //    JsonConvert.SerializeObject(gameDataModel, Formatting.Indented));

            // Determine general game state
            ParseGameSate(gameDataModel, colors);

            // Parse the lighting
            var characterMatch = ParseCharacter(gameDataModel, colors);

            // Ult can't possibly be ready within 2 seconds of changing, this avoids false positives.
            // Filtering on ultReady and ultUsed removes false positives from the native ultimate effects
            // The control keys don't show during character select, so don't continue on those either.
            if (_characterChange.AddSeconds(2) >= DateTime.Now ||
                _ultimateUsed.AddSeconds(2) >= DateTime.Now ||
                _ultimateReady.AddSeconds(2) >= DateTime.Now ||
                _stickyStatus.Value == OverwatchStatus.InCharacterSelect)
                return;

            ParseSpecialKeys(gameDataModel, characterMatch, colors);
            ParseAbilities(gameDataModel, colors);
        }

        private void ParseGameSate(OverwatchDataModel gameDataModel, Color[,] colors)
        {
            if (_ultimateUsed.AddSeconds(5) >= DateTime.Now)
                return;

            if (colors[0, 0].Equals(Color.FromRgb(55, 30, 0)))
                _stickyStatus.Value = OverwatchStatus.InMainMenu;

            if (_stickyStatus.Value != OverwatchStatus.InMainMenu)
                return;

            gameDataModel.Character = OverwatchCharacter.None;
            gameDataModel.Ability1Ready = false;
            gameDataModel.Ability2Ready = false;
            _stickyUltimateReady.Value = false;
            _stickyUltimateUsed.Value = false;
        }

        private CharacterColor? ParseCharacter(OverwatchDataModel gameDataModel, Color[,] colors)
        {
            var characterMatch = OverwatchCharacters.FirstOrDefault(c => c.Color == colors[0, 20]);
            // If a new character was chosen, let the other methods know
            if (characterMatch.Character != gameDataModel.Character)
                _characterChange = DateTime.Now;

            // If no character was found, this method shouldn't continue
            if (characterMatch.Character == OverwatchCharacter.None)
                return characterMatch;

            // If WASD isn't orange (any of them will do), player is in character select
            _stickyStatus.Value = ControlsShown(colors) ? OverwatchStatus.InGame : OverwatchStatus.InCharacterSelect;

            // Update the datamodel
            gameDataModel.Character = characterMatch.Character;
            return characterMatch;
        }

        private bool ControlsShown(Color[,] colors)
        {
            var keyColor = Color.FromRgb(222, 153, 0);
            return colors[2, 3] == keyColor || colors[3, 2] == keyColor ||
                   colors[3, 3] == keyColor || colors[3, 4] == keyColor;
        }

        private void ParseSpecialKeys(OverwatchDataModel gameDataModel, CharacterColor? characterMatch, Color[,] colors)
        {
            if (characterMatch == null || characterMatch.Value.Character == OverwatchCharacter.None)
                return;

            // Ultimate is ready when Q is blinking
            var charCol = characterMatch.Value.Color;
            var backlidColor = Color.FromRgb((byte) (charCol.R*0.25), (byte) (charCol.G*0.25), (byte) (charCol.B*0.25));
            var ultReady = !backlidColor.Equals(colors[2, 2]);

            if (_ultimateUsed.AddSeconds(15) <= DateTime.Now)
            {
                // Player can change hero if H is blinking
                gameDataModel.CanChangeHero = !colors[3, 7].Equals(backlidColor);

                if (!_stickyUltimateReady.Value && ultReady && ControlsShown(colors))
                {
                    _ultimateReady = DateTime.Now;
                    _stickyUltimateReady.Value = true;
                }
            }

            // If ult no longer ready but it was ready before, it was used.
            if (_stickyUltimateReady.Value && !ultReady)
            {
                _stickyUltimateReady.Value = false;
                if (_ultimateUsed.AddSeconds(15) <= DateTime.Now)
                    _ultimateUsed = DateTime.Now;
            }

            // UltimateUsed is true for 10 seconds after ultimate went on cooldown
            if (_ultimateUsed != DateTime.MinValue)
                _stickyUltimateUsed.Value = _ultimateUsed.AddSeconds(10) >= DateTime.Now;
        }

        private void ParseAbilities(OverwatchDataModel gameDataModel, Color[,] colors)
        {
            gameDataModel.Ability1Ready = colors[4, 1].Equals(Color.FromRgb(4, 141, 144));
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