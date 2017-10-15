using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;


namespace Artemis.Modules.Games.FormulaOne2017
{
    public class FormulaOne2017Model : ModuleModel
    {
        private bool _mustListen;
        private UdpClient _udpClient;
        private UdpClient _udpListener;
        private DateTime _lastUpdate;
        private int _revAtZeroFrames;

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
            // If we're not receiving updates, assume the game is paused/in the main menu
            if (DateTime.Now - _lastUpdate > TimeSpan.FromSeconds(1))
                ((FormulaOne2017DataModel) DataModel).Session.SessionType = SessionType.Unknown;
        }

        public override void Enable()
        {
            _mustListen = true;
            Task.Run(async () =>
            {
                _udpClient = new UdpClient(20777);
                while (_mustListen)
                {
                    //IPEndPoint object will allow us to read datagrams sent from any source.
                    try
                    {
                        var receivedResults = await _udpClient.ReceiveAsync();
                        HandleGameData(receivedResults);
                    }
                    catch (ObjectDisposedException)
                    {
                        // ignored, happens when shutting the module down
                    }
                }
            });
            base.Enable();
        }

        private void HandleGameData(UdpReceiveResult receivedResults)
        {
            _lastUpdate = DateTime.Now;
            var dataModel = (FormulaOne2017DataModel) DataModel;
            var pinnedPacket = GCHandle.Alloc(receivedResults.Buffer, GCHandleType.Pinned);
            var msg = (FormulaOne2017DataModel.UdpPacketData) Marshal.PtrToStructure(pinnedPacket.AddrOfPinnedObject(), typeof(FormulaOne2017DataModel.UdpPacketData));
            pinnedPacket.Free();

            dataModel.Car.SpeedKph = msg.m_speed * 1.609344;
            dataModel.Car.SpeedMph = msg.m_speed;

            dataModel.Car.Steering = msg.m_steer;
            dataModel.Car.Throttle = msg.m_throttle;
            dataModel.Car.Brake = msg.m_brake;
            dataModel.Car.Clutch = msg.m_clutch;

            dataModel.Car.Drs = msg.m_drs > 0;

            dataModel.Car.Overview.Team = (F1Team) msg.m_team_info;
            dataModel.Car.Overview.TractionControl = CarOverview.FloatToAssistLevel(msg.m_traction_control);
            dataModel.Car.Overview.AntiLockBrakes = msg.m_anti_lock_brakes > 0;

            dataModel.Car.Details.Rpm = msg.m_engineRate;
            dataModel.Car.Details.MaxRpm = msg.m_max_rpm;
            dataModel.Car.Details.IdleRpm = msg.m_idle_rpm;

            // The one the game provides is all over the place at max rev causing blinking etc
            // easily fixed by simply ignoring rapid changes from 100 to 0
            if (msg.m_rev_lights_percent == 0)
                _revAtZeroFrames++;
            else
                _revAtZeroFrames = 0;
            if (_revAtZeroFrames > 2 || msg.m_rev_lights_percent != 0 || dataModel.Car.Details.RevLightsPercent != 100)
                dataModel.Car.Details.RevLightsPercent = msg.m_rev_lights_percent;

            dataModel.Car.Details.Gear = (int) msg.m_gear;
            dataModel.Car.Details.MaxGear = (int) msg.m_max_gears;

            dataModel.Car.Details.Kers = msg.m_kers_level;
            dataModel.Car.Details.MaxKers = msg.m_kers_max_level;

            dataModel.Car.Details.Fuel = msg.m_fuel_in_tank;
            dataModel.Car.Details.MaxFuel = msg.m_fuel_capacity;

            dataModel.Car.Details.LateralG = msg.m_gforce_lat;
            dataModel.Car.Details.LongitudinalG = msg.m_gforce_lon;

//            dataModel.Car.Details.WheelSpeedFrontLeft = msg.m_wheel_speed_fl;
//            dataModel.Car.Details.WheelSpeedFrontRight = msg.m_wheel_speed_fr;
//            dataModel.Car.Details.WheelSpeedRearLeft = msg.m_wheel_speed_bl;
//            dataModel.Car.Details.WheelSpeedRearRight = msg.m_wheel_speed_br;

            dataModel.Session.SessionType = (SessionType) msg.m_sessionType;
            // It's unknown in time trial but lets overwrite that to race
            if (dataModel.Session.SessionType == SessionType.Unknown && dataModel.Car.SpeedMph > 0)
                dataModel.Session.SessionType = SessionType.Race;

            dataModel.Session.DrsEnabled = msg.m_drsAllowed > 0;
            dataModel.Session.Flags = (SessionFlag) msg.m_vehicleFIAFlags;

            dataModel.Session.TotalSeconds = msg.m_time;
            dataModel.Session.LapSeconds = msg.m_lapTime;

            dataModel.Session.Track = (F1Track) msg.m_track_number;
            dataModel.Session.TrackLength = msg.m_track_size;
            dataModel.Session.TotalDistance = msg.m_totalDistance;
            dataModel.Session.LapDistance = msg.m_lapDistance;
            
            dataModel.Session.LapNumber = (int) msg.m_lap;
            dataModel.Session.TotalLaps = (int) msg.m_total_laps;
            dataModel.Session.Position = (int) msg.m_car_position;
        }

        public override void Dispose()
        {
            _mustListen = false;
            _udpClient.Dispose();
            _udpClient = null;
            base.Dispose();
        }
    }
}
