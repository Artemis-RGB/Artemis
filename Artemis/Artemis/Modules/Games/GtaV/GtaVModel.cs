using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;

namespace Artemis.Modules.Games.GtaV
{
    public class GtaVModel : ModuleModel
    {
        private readonly PipeServer _pipeServer;

        public GtaVModel(DeviceManager deviceManager, LuaManager luaManager, PipeServer pipeServer)
            : base(deviceManager, luaManager)
        {
            _pipeServer = pipeServer;

            Settings = SettingsProvider.Load<GtaVSettings>();
            DataModel = new GtaVDataModel();
            ProcessNames.Add("GTA5");
        }

        public override string Name => "GTAV";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Enable()
        {
            DllManager.PlaceLogitechDll();
            _pipeServer.PipeMessage += PipeServerOnPipeMessage;
            base.Enable();
        }

        public override void Dispose()
        {
            base.Dispose();

            // Delay restoring the DLL to allow GTA to release it
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(5000);
                DllManager.RestoreLogitechDll();
            });

            _pipeServer.PipeMessage -= PipeServerOnPipeMessage;
        }

        public override void Update()
        {
            // DataModel updating is done whenever a pipe message is received
        }

        private void PipeServerOnPipeMessage(string reply)
        {
            if (!IsInitialized)
                return;

            // Convert the given string to a list of ints
            var stringParts = reply.Split(' ');
            var parts = new string[stringParts.Length];
            for (var i = 0; i < stringParts.Length; i++)
                parts[i] = stringParts[i];

            if (parts[0] == "0")
                InterpertrateLighting(parts);
        }

        private void InterpertrateLighting(string[] parts)
        {
            var gameDataModel = (GtaVDataModel) DataModel;

            var custom = parts[1];
            gameDataModel.IsWanted = custom.StartsWith("ff");

            var rPer = byte.Parse(parts[2]);
            var gPer = byte.Parse(parts[3]);
            var bPer = byte.Parse(parts[4]);
            gameDataModel.Color = Color.FromArgb(255, rPer, gPer, bPer).ToHex();
        }
    }
}