using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;

namespace Artemis.Modules.Games.FormulaOne2017
{
    public class FormulaOne2017Model : ModuleModel
    {
        private UdpClient _udpListener;
        private bool _mustListen;

        public FormulaOne2017Model(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<FormulaOne2017Settings>();
            DataModel = new FormulaOne2017DataModel();
            ProcessNames.Add("F1_2017");
        }

        public override string Name => "FormulaOne2017";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Update()
        {
        }

        public override void Enable()
        {
            _mustListen = true;
            Task.Run(async () =>
            {
                using (var udpClient = new UdpClient(20777))
                {
                    string loggingEvent = "";
                    while (_mustListen)
                    {
                        //IPEndPoint object will allow us to read datagrams sent from any source.
                        var receivedResults = await udpClient.ReceiveAsync();
                        HandleGameData(receivedResults);
                    }
                }
            });
            base.Enable();
        }

        private void HandleGameData(UdpReceiveResult receivedResults)
        {
            var dataModel = (FormulaOne2017DataModel) DataModel;
            var pinnedPacket = GCHandle.Alloc(receivedResults.Buffer, GCHandleType.Pinned);
            var msg = (FormulaOne2017DataModel.UdpPacketData) Marshal.PtrToStructure(pinnedPacket.AddrOfPinnedObject(), typeof(FormulaOne2017DataModel.UdpPacketData));
            pinnedPacket.Free();

//            dataModel.Rpm = msg.m_engineRate;
//            dataModel.MaxRpm = msg.m_max_rpm;
//            dataModel.IdleRpm = msg.m_idle_rpm;
        }

        public override void Dispose()
        {
            _mustListen = false;
            base.Dispose();
        }
    }
}
