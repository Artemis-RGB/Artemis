using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Profiles.Layers.Models;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;

namespace Artemis.Modules.Games.GtaV
{
    public class GtaVModel : GameModel
    {
        private readonly PipeServer _pipeServer;

        public GtaVModel(DeviceManager deviceManager, PipeServer pipeServer)
            : base(deviceManager, SettingsProvider.Load<GtaVSettings>(), new GtaVDataModel())
        {
            _pipeServer = pipeServer;
            Name = "GTAV";
            ProcessName = "GTA5";
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public override void Enable()
        {
            DllManager.PlaceLogitechDll();
            _pipeServer.PipeMessage += PipeServerOnPipeMessage;
            Initialized = true;
        }

        public override void Dispose()
        {
            Initialized = false;

            // Delay restoring the DLL to allow GTA to release it
            Task.Factory.StartNew(() =>
            {
                Thread.Sleep(2000);
                DllManager.RestoreLogitechDll();
            });

            _pipeServer.PipeMessage -= PipeServerOnPipeMessage;
            base.Dispose();
        }

        public override void Update()
        {
            // DataModel updating is done whenever a pipe message is received
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }

        private void PipeServerOnPipeMessage(string reply)
        {
            if (!Initialized)
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