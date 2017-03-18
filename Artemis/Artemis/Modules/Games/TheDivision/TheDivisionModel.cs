//using System.Collections.Generic;
//using System.Linq;
//using System.Threading;
//using System.Threading.Tasks;
//using Artemis.DAL;
//using Artemis.DeviceProviders.Logitech.Utilities;
//using Artemis.Managers;
//using Artemis.Modules.Abstract;
//using Artemis.Utilities;
//using Artemis.Utilities.DataReaders;
//
//namespace Artemis.Modules.Games.TheDivision
//{
//    public class TheDivisionModel : ModuleModel
//    {
//        private readonly PipeServer _pipeServer;
//        private StickyValue<bool> _stickyAmmo;
//        private StickyValue<bool> _stickyHp;
//
//        public TheDivisionModel(DeviceManager deviceManager, LuaManager luaManager, PipeServer pipeServer)
//            : base(deviceManager, luaManager)
//        {
//            _pipeServer = pipeServer;
//
//            Settings = SettingsProvider.Load<TheDivisionSettings>();
//            DataModel = new TheDivisionDataModel();
//            ProcessNames.Add("TheDivision");
//        }
//
//        public override string Name => "TheDivision";
//        public override bool IsOverlay => false;
//        public override bool IsBoundToProcess => true;
//
//        public override void Dispose()
//        {
//            base.Dispose();
//
//            // Delay restoring the DLL to allow The Division to release it
//            Task.Factory.StartNew(() =>
//            {
//                Thread.Sleep(2000);
//                DllManager.RestoreLogitechDll();
//            });
//
//            _stickyAmmo?.Dispose();
//            _stickyHp?.Dispose();
//            _pipeServer.PipeMessage -= PipeServerOnPipeMessage;
//        }
//
//        public override void Enable()
//        {
//            _stickyAmmo = new StickyValue<bool>(200);
//            _stickyHp = new StickyValue<bool>(200);
//
//            var process = System.Diagnostics.Process.GetProcessesByName("TheDivision").First();
//            DllManager.PlaceLogitechDll();
//
//            _pipeServer.PipeMessage += PipeServerOnPipeMessage;
//
//            base.Enable();
//        }
//
//        private void PipeServerOnPipeMessage(string reply)
//        {
//            if (!IsInitialized)
//                return;
//
//            // Convert the given string to a list of ints
//            var stringParts = reply.Split(' ');
//            if (stringParts[0] != "1")
//                return;
//
//            // Parse into a list of ints and interpertrate
//            var parts = new int[stringParts.Length];
//            for (var i = 0; i < stringParts.Length; i++)
//                parts[i] = int.Parse(stringParts[i]);
//
//            InterpertrateDivisionKey(parts);
//        }
//
//        // Parses Division key data to game data
//        private void InterpertrateDivisionKey(IReadOnlyList<int> parts)
//        {
//            var gameDataModel = (TheDivisionDataModel) DataModel;
//            var keyCode = parts[1];
//            var rPer = parts[2];
//            var gPer = parts[3];
//            var bPer = parts[4];
//
//            var keyEnum = (KeyboardNames) keyCode;
//
//            // F1 to F4 indicate the player and his party. Blinks red on damage taken
//            if (keyCode >= 59 && keyCode <= 62)
//            {
//                var playerId = keyCode - 58;
//
//                PlayerState newState;
//                if (gPer > 10)
//                    newState = PlayerState.Online;
//                else if (rPer > 10)
//                    newState = PlayerState.Hit;
//                else
//                    newState = PlayerState.Offline;
//
//                if (playerId == 1)
//                    gameDataModel.LowHp = newState == PlayerState.Hit;
//                else if (playerId == 2)
//                    gameDataModel.PartyMember1 = newState;
//                else if (playerId == 3)
//                    gameDataModel.PartyMember2 = newState;
//                else if (playerId == 4)
//                    gameDataModel.PartyMember3 = newState;
//            }
//            // R blinks white when low on ammo
//            else if (keyCode == 19)
//            {
//                _stickyAmmo.Value = rPer == 100 && gPer > 1 && bPer > 1;
//                gameDataModel.LowAmmo = _stickyAmmo.Value;
//            }
//            // G turns white when holding a grenade, turns off when out of grenades
//            else if (keyCode == 34)
//            {
//                if (rPer == 100 && gPer < 10 && bPer < 10)
//                    gameDataModel.GrenadeState = GrenadeState.HasGrenade;
//                else if (rPer == 100 && gPer > 10 && bPer > 10)
//                    gameDataModel.GrenadeState = GrenadeState.GrenadeEquipped;
//                else
//                    gameDataModel.GrenadeState = GrenadeState.HasNoGrenade;
//            }
//            // V blinks on low HP
//            else if (keyCode == 47)
//            {
//                _stickyHp.Value = rPer == 100 && gPer > 1 && bPer > 1;
//                gameDataModel.LowHp = _stickyHp.Value;
//            }
//        }
//
//        public override void Update()
//        {
//            // DataModel updating is done whenever a pipe message is received
//        }
//    }
//}