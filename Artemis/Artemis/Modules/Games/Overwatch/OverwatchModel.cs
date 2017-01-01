using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Services;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;
using Artemis.ViewModels;
using Microsoft.Win32;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchModel : GameModel
    {
        private readonly DebugViewModel _debugViewModel;
        private readonly MetroDialogService _dialogService;
        private readonly PipeServer _pipeServer;
        private DateTime _characterChange;
        private string _lastMessage;
        // Using sticky values on these since they can cause flickering
        private StickyValue<OverwatchStatus> _stickyStatus;
        private StickyValue<bool> _stickyUltimateReady;
        private StickyValue<bool> _stickyUltimateUsed;
        private DateTime _ultimateReady;
        private DateTime _ultimateUsed;

        public OverwatchModel(DeviceManager deviceManager, LuaManager luaManager, PipeServer pipeServer,
            MetroDialogService dialogService, DebugViewModel debugViewModel)
            : base(deviceManager, luaManager, SettingsProvider.Load<OverwatchSettings>(), new OverwatchDataModel())
        {
            _pipeServer = pipeServer;
            _dialogService = dialogService;
            _debugViewModel = debugViewModel;
            Name = "Overwatch";
            ProcessName = "Overwatch";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            LoadOverwatchCharacters();
            FindOverwatch();
        }

        public List<CharacterColor> OverwatchCharacters { get; set; }

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
                new CharacterColor {Character = OverwatchCharacter.Zenyatta, Color = Color.FromRgb(248, 218, 26)},
                new CharacterColor {Character = OverwatchCharacter.Ana, Color = Color.FromRgb(16, 36, 87)},
                new CharacterColor {Character = OverwatchCharacter.Sombra, Color = Color.FromRgb(20, 5, 101)}
            };
        }

        public override void Enable()
        {
            base.Enable();

            _stickyStatus = new StickyValue<OverwatchStatus>(300);
            _stickyUltimateReady = new StickyValue<bool>(350);
            _stickyUltimateUsed = new StickyValue<bool>(350);
            _pipeServer.PipeMessage += PipeServerOnPipeMessage;

            Initialized = true;
        }

        public override void Dispose()
        {
            Initialized = false;

            _stickyStatus?.Dispose();
            _stickyUltimateReady?.Dispose();
            _stickyUltimateUsed?.Dispose();

            _pipeServer.PipeMessage -= PipeServerOnPipeMessage;
            base.Dispose();
        }

        private void PipeServerOnPipeMessage(string message)
        {
            _lastMessage = message;
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

            if (_lastMessage == null)
                return;

            var colors = ParseColorArray(_lastMessage);

            if (colors == null)
                return;

            _debugViewModel.UpdateRazerDisplay(colors);

            // Determine general game state
            ParseGameSate(gameDataModel, colors);

            // Parse the lighting
            var characterMatch = ParseCharacter(gameDataModel, colors);

            // Ult can't possibly be ready within 2 seconds of changing, this avoids false positives.
            // Filtering on ultReady and ultUsed removes false positives from the native ultimate effects
            // The control keys don't show during character select, so don't continue on those either.
            if ((_characterChange.AddSeconds(2) >= DateTime.Now) ||
                (_ultimateUsed.AddSeconds(2) >= DateTime.Now) ||
                (_ultimateReady.AddSeconds(2) >= DateTime.Now) ||
                (_stickyStatus.Value == OverwatchStatus.InCharacterSelect))
                return;

            ParseSpecialKeys(gameDataModel, characterMatch, colors);
            ParseAbilities(gameDataModel, colors);
        }

        public Color[,] ParseColorArray(string arrayString)
        {
            if (string.IsNullOrEmpty(arrayString))
                return null;
            var intermediateArray = arrayString.Split('|');
            if ((intermediateArray[0] == "1") || (intermediateArray.Length < 2))
                return null;
            var array = intermediateArray[1].Substring(1).Split(' ');
            if (!array.Any())
                return null;

            try
            {
                var colors = new Color[6, 22];
                foreach (var intermediate in array)
                {
                    if (intermediate.Length > 16)
                        continue;

                    // Can't parse to a byte directly since it may contain values >254
                    var parts = intermediate.Split(',').Select(int.Parse).ToArray();
                    if ((parts[0] >= 5) && (parts[1] >= 21))
                        continue;

                    colors[parts[0], parts[1]] = Color.FromRgb((byte) parts[2], (byte) parts[3], (byte) parts[4]);
                }
                return colors;
            }
            catch (FormatException e)
            {
                Logger?.Trace(e, "Failed to parse to color array");
                return null;
            }
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
            return (colors[2, 3] == keyColor) || (colors[3, 2] == keyColor) ||
                   (colors[3, 3] == keyColor) || (colors[3, 4] == keyColor);
        }

        private void ParseSpecialKeys(OverwatchDataModel gameDataModel, CharacterColor? characterMatch, Color[,] colors)
        {
            if ((characterMatch == null) || (characterMatch.Value.Character == OverwatchCharacter.None))
                return;

            // Ultimate is ready when Q is blinking
            var charCol = characterMatch.Value.Color;
            var backlidColor = Color.FromRgb((byte) (charCol.R * 0.25), (byte) (charCol.G * 0.25),
                (byte) (charCol.B * 0.25));
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

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }

        public void FindOverwatch()
        {
            var gameSettings = Settings as OverwatchSettings;
            if (gameSettings == null)
                return;

            // If already propertly set up, don't do anything
            if ((gameSettings.GameDirectory != null) && File.Exists(gameSettings.GameDirectory + "Overwatch.exe") &&
                File.Exists(gameSettings.GameDirectory + "RzChromaSDK64.dll"))
                return;

            var key = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall\Overwatch");
            var path = key?.GetValue("DisplayIcon")?.ToString();

            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return;

            gameSettings.GameDirectory = path.Substring(0, path.Length - 14);
            gameSettings.Save();
            PlaceDll();
        }

        public void PlaceDll()
        {
            var settings = (OverwatchSettings) Settings;
            var path = settings.GameDirectory;

            if (!File.Exists(path + @"\Overwatch.exe"))
            {
                _dialogService.ShowErrorMessageBox("Please select a valid Overwatch directory\n\n" +
                                                   @"By default Overwatch is in C:\Program Files (x86)\Overwatch");

                settings.GameDirectory = string.Empty;
                settings.Save();
                return;
            }

            DllManager.PlaceRazerDll(path);
        }
    }

    public struct CharacterColor
    {
        public OverwatchCharacter Character { get; set; }
        public Color Color { get; set; }
    }
}