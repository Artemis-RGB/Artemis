using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Modules.Effects.TypeWave;
using Artemis.Utilities;
using Artemis.Utilities.Keyboard;
using Artemis.Utilities.LogitechDll;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Modules.Games.TheDivision
{
    public class TheDivisionModel : GameModel
    {
        private Wave _ammoWave;
        private KeyboardRectangle _hpRect;
        private KeyboardRectangle _p2;
        private KeyboardRectangle _p3;
        private KeyboardRectangle _p4;
        private StickyValue<bool> _stickyAmmo;
        private StickyValue<bool> _stickyHp;
        private int _trans;

        public TheDivisionModel(MainManager mainManager, TheDivisionSettings settings) : base(mainManager, settings, new TheDivisionDataModel())
        {
            Name = "TheDivision";
            ProcessName = "TheDivision";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            DllManager.RestoreDll();

            _stickyAmmo.Dispose();
            _stickyHp.Dispose();
        }

        public override void Enable()
        {
            Initialized = false;

            _ammoWave = new Wave(new Point(30, 14), 0, Color.Transparent);
            _hpRect = new KeyboardRectangle(MainManager.DeviceManager.ActiveKeyboard, 3*Scale, 0*Scale,
                new List<Color>(),
                LinearGradientMode.Horizontal)
            {
                Height = 7*Scale,
                Width = 21*Scale,
                Rotate = true,
                ContainedBrush = false
            };

            _p2 = new KeyboardRectangle(MainManager.DeviceManager.ActiveKeyboard, 0*Scale, 1*Scale,
                new List<Color>(),
                LinearGradientMode.Horizontal)
            {
                Height = 2*Scale,
                Width = 3*Scale,
                Rotate = true,
                ContainedBrush = false
            };
            _p3 = new KeyboardRectangle(MainManager.DeviceManager.ActiveKeyboard, 0*Scale, 3*Scale,
                new List<Color>(),
                LinearGradientMode.Horizontal)
            {
                Height = 2*Scale,
                Width = 3*Scale,
                Rotate = true,
                ContainedBrush = false
            };
            _p4 = new KeyboardRectangle(MainManager.DeviceManager.ActiveKeyboard, 0*Scale, 5*Scale,
                new List<Color>(),
                LinearGradientMode.Horizontal)
            {
                Height = 2*Scale,
                Width = 3*Scale,
                Rotate = true,
                ContainedBrush = false
            };

            _stickyAmmo = new StickyValue<bool>(200);
            _stickyHp = new StickyValue<bool>(200);

            DllManager.PlaceDll();

            MainManager.PipeServer.PipeMessage += PipeServerOnPipeMessage;
            Initialized = true;
        }

        private void PipeServerOnPipeMessage(string reply)
        {
            if (!Initialized)
                return;

            // Convert the given string to a list of ints
            var stringParts = reply.Split(' ');
            var parts = new int[stringParts.Length];
            for (var i = 0; i < stringParts.Length; i++)
                parts[i] = int.Parse(stringParts[i]);

            if (parts[0] == 1)
                InterpertrateDivisionKey(parts);
        }

        // Parses Division key data to game data
        private void InterpertrateDivisionKey(IReadOnlyList<int> parts)
        {
            var gameDataModel = (TheDivisionDataModel)GameDataModel;
            var keyCode = parts[1];
            var rPer = parts[2];
            var gPer = parts[3];
            var bPer = parts[4];

            // F1 to F4 indicate the player and his party. Blinks red on damage taken
            if (keyCode >= 59 && keyCode <= 62)
            {
                var playerId = keyCode - 58;

                PlayerState newState;
                if (gPer > 10)
                    newState = PlayerState.Online;
                else if (rPer > 10)
                    newState = PlayerState.Hit;
                else
                    newState = PlayerState.Offline;

                if (playerId == 1)
                    gameDataModel.PartyMember1 = newState;
                else if (playerId == 2)
                    gameDataModel.PartyMember2 = newState;
                else if (playerId == 3)
                    gameDataModel.PartyMember3 = newState;
            }
            // R blinks white when low on ammo
            else if (keyCode == 19)
            {
                _stickyAmmo.Value = rPer == 100 && gPer > 1 && bPer > 1;
                gameDataModel.LowAmmo = _stickyAmmo.Value;
            }
            // G turns white when holding a grenade, turns off when out of grenades
            else if (keyCode == 34)
            {
                if (rPer == 100 && gPer < 10 && bPer < 10)
                    gameDataModel.GrenadeState = GrenadeState.HasGrenade;
                else if (rPer == 100 && gPer > 10 && bPer > 10)
                    gameDataModel.GrenadeState = GrenadeState.GrenadeEquipped;
                else
                    gameDataModel.GrenadeState = GrenadeState.HasNoGrenade;
            }
            // V blinks on low HP
            else if (keyCode == 47)
            {
                _stickyHp.Value = rPer == 100 && gPer > 1 && bPer > 1;
                gameDataModel.LowHp = _stickyHp.Value;
            }
        }

        public override void Update()
        {
            var gameDataModel = (TheDivisionDataModel)GameDataModel;
            if (gameDataModel.LowAmmo)
            {
                _ammoWave.Size++;
                if (_ammoWave.Size > 30)
                {
                    _ammoWave.Size = 0;
                    _trans = 255;
                }
            }
            else
                _ammoWave.Size = 0;

            _hpRect.Colors = gameDataModel.LowHp
                ? new List<Color> {Color.Red, Color.Orange}
                : new List<Color> {Color.FromArgb(10, 255, 0), Color.FromArgb(80, 255, 45)};

            if (gameDataModel.PartyMember1 == PlayerState.Offline)
                _p2.Colors = new List<Color> {Color.Gray, Color.White};
            else if (gameDataModel.PartyMember1 == PlayerState.Online)
                _p2.Colors = new List<Color> {Color.FromArgb(10, 255, 0), Color.FromArgb(80, 255, 45)};
            else
                _p2.Colors = new List<Color> {Color.Red, Color.Orange};

            if (gameDataModel.PartyMember2 == PlayerState.Offline)
                _p3.Colors = new List<Color> {Color.Gray, Color.White};
            else if (gameDataModel.PartyMember2 == PlayerState.Online)
                _p3.Colors = new List<Color> {Color.FromArgb(10, 255, 0), Color.FromArgb(80, 255, 45)};
            else
                _p3.Colors = new List<Color> {Color.Red, Color.Orange};

            if (gameDataModel.PartyMember3 == PlayerState.Offline)
                _p4.Colors = new List<Color> {Color.Gray, Color.White};
            else if (gameDataModel.PartyMember3 == PlayerState.Online)
                _p4.Colors = new List<Color> {Color.FromArgb(10, 255, 0), Color.FromArgb(80, 255, 45)};
            else
                _p4.Colors = new List<Color> {Color.Red, Color.Orange};
        }

        public override Bitmap GenerateBitmap()
        {
            var bitmap = MainManager.DeviceManager.ActiveKeyboard.KeyboardBitmap(Scale);
            using (var g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                _hpRect.Draw(g);
                _p2.Draw(g);
                _p3.Draw(g);
                _p4.Draw(g);
                // Very, very PH
                if (_ammoWave.Size != 0)
                {
                    var path = new GraphicsPath();
                    path.AddEllipse(_ammoWave.Point.X - _ammoWave.Size/2, _ammoWave.Point.Y - _ammoWave.Size/2,
                        _ammoWave.Size, _ammoWave.Size);

                    if (_ammoWave.Size > 15)
                        _trans = _trans - 16;
                    if (_trans < 1)
                        _trans = 255;
                    var pthGrBrush = new PathGradientBrush(path)
                    {
                        SurroundColors = new[] {_ammoWave.Color},
                        CenterColor = Color.FromArgb(_trans, 255, 0, 0)
                    };

                    g.FillPath(pthGrBrush, path);
                    pthGrBrush.FocusScales = new PointF(0.3f, 0.8f);

                    g.FillPath(pthGrBrush, path);
                    g.DrawEllipse(new Pen(pthGrBrush, 1), _ammoWave.Point.X - _ammoWave.Size/2,
                        _ammoWave.Point.Y - _ammoWave.Size/2, _ammoWave.Size, _ammoWave.Size);
                }
            }
            return bitmap;
        }

        public override Brush GenerateMouseBrush()
        {
            return Profile.GenerateBrush<TheDivisionDataModel>(GameDataModel, LayerType.Mouse, false, true);
        }

        public override Brush GenerateHeadsetBrush()
        {
            return Profile.GenerateBrush<TheDivisionDataModel>(GameDataModel, LayerType.Headset, false, true);
        }
    }
}