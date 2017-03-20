using System;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Modules.Abstract;
using AssettoCorsaSharedMemory;

namespace Artemis.Modules.Games.AssettoCorsa
{
    public class AssettoCorsaModel : ModuleModel
    {
        private AssettoCorsaSharedMemory.AssettoCorsa _ac;

        public AssettoCorsaModel(DeviceManager deviceManager, LuaManager luaManager) : base(deviceManager, luaManager)
        {
            Settings = SettingsProvider.Load<AssettoCorsaSettings>();
            DataModel = new AssettoCorsaDataModel();
            ProcessNames.Add("AssettoCorsa");
        }

        public override string Name => "AssettoCorsa";
        public override bool IsOverlay => false;
        public override bool IsBoundToProcess => true;

        public override void Enable()
        {
            _ac = new AssettoCorsaSharedMemory.AssettoCorsa {PhysicsInterval = 40};
            // The event handlers map their respecive enums to the datamodel, not pretty but fastest way possible
            _ac.StaticInfoUpdated += AcOnStaticInfoUpdated;
            _ac.GraphicsUpdated += AcOnGraphicsUpdated;
            _ac.PhysicsUpdated += AcOnPhysicsUpdated;
            _ac.Start();
            
            base.Enable();
        }

        public override void Dispose()
        {
            base.Dispose();
            _ac.Stop();
            _ac = null;
        }

        private void AcOnStaticInfoUpdated(object sender, StaticInfoEventArgs e)
        {
            var dataModel = (AssettoCorsaDataModel)DataModel;
            dataModel.StaticInfo.SMVersion = e.StaticInfo.SMVersion;
            dataModel.StaticInfo.ACVersion = e.StaticInfo.ACVersion;
            dataModel.StaticInfo.Session.NumberOfSessions = e.StaticInfo.NumberOfSessions;
            dataModel.StaticInfo.Session.NumCars = e.StaticInfo.NumCars;
            dataModel.StaticInfo.Session.Track = e.StaticInfo.Track;
            dataModel.StaticInfo.Session.TrackConfiguration = e.StaticInfo.TrackConfiguration;
            dataModel.StaticInfo.Session.TrackSPlineLength = e.StaticInfo.TrackSPlineLength;
            dataModel.StaticInfo.Session.PlayerName = e.StaticInfo.PlayerName;
            dataModel.StaticInfo.Session.PlayerSurname = e.StaticInfo.PlayerSurname;
            dataModel.StaticInfo.Session.PlayerNick = e.StaticInfo.PlayerNick;
            dataModel.StaticInfo.Session.SectorCount = e.StaticInfo.SectorCount;
            dataModel.StaticInfo.Session.PenaltiesEnabled = e.StaticInfo.PenaltiesEnabled;
            dataModel.StaticInfo.Car.CarModel = e.StaticInfo.CarModel;
            dataModel.StaticInfo.Car.MaxTorque = e.StaticInfo.MaxTorque;
            dataModel.StaticInfo.Car.MaxPower = e.StaticInfo.MaxPower;
            dataModel.StaticInfo.Car.MaxRpm = e.StaticInfo.MaxRpm;
            dataModel.StaticInfo.Car.MaxFuel = e.StaticInfo.MaxFuel;
            dataModel.StaticInfo.Car.SuspensionMaxTravel = e.StaticInfo.SuspensionMaxTravel;
            dataModel.StaticInfo.Car.TyreRadius = e.StaticInfo.TyreRadius;
            dataModel.StaticInfo.Car.MaxTurboBoost = e.StaticInfo.MaxTurboBoost;
            dataModel.StaticInfo.Car.AidFuelRate = e.StaticInfo.AidFuelRate;
            dataModel.StaticInfo.Car.AidTireRate = e.StaticInfo.AidTireRate;
            dataModel.StaticInfo.Car.AidMechanicalDamage = e.StaticInfo.AidMechanicalDamage;
            dataModel.StaticInfo.Car.AidAllowTyreBlankets = e.StaticInfo.AidAllowTyreBlankets;
            dataModel.StaticInfo.Car.AidStability = e.StaticInfo.AidStability;
            dataModel.StaticInfo.Car.AidAutoClutch = e.StaticInfo.AidAutoClutch;
            dataModel.StaticInfo.Car.AidAutoBlip = e.StaticInfo.AidAutoBlip;
            dataModel.StaticInfo.Car.HasDRS = e.StaticInfo.HasDRS;
            dataModel.StaticInfo.Car.HasERS = e.StaticInfo.HasERS;
            dataModel.StaticInfo.Car.HasKERS = e.StaticInfo.HasKERS;
            dataModel.StaticInfo.Car.KersMaxJoules = e.StaticInfo.KersMaxJoules;
            dataModel.StaticInfo.Car.EngineBrakeSettingsCount = e.StaticInfo.EngineBrakeSettingsCount;
            dataModel.StaticInfo.Car.ErsPowerControllerCount = e.StaticInfo.ErsPowerControllerCount;
        }

        private void AcOnGraphicsUpdated(object sender, GraphicsEventArgs e)
        {
            var dataModel = (AssettoCorsaDataModel)DataModel;
            dataModel.Interface.Status = e.Graphics.Status;
            dataModel.Interface.Session = e.Graphics.Session;
            dataModel.Interface.CurrentTime = e.Graphics.CurrentTime;
            dataModel.Interface.LastTime = e.Graphics.LastTime;
            dataModel.Interface.BestTime = e.Graphics.BestTime;
            dataModel.Interface.Split = e.Graphics.Split;
            dataModel.Interface.Position = e.Graphics.Position;
            dataModel.Interface.iCurrentTime = e.Graphics.iCurrentTime;
            dataModel.Interface.iLastTime = e.Graphics.iLastTime;
            dataModel.Interface.iBestTime = e.Graphics.iBestTime;
            dataModel.Interface.SessionTimeLeft = e.Graphics.SessionTimeLeft;
            dataModel.Interface.DistanceTraveled = e.Graphics.DistanceTraveled;
            dataModel.Interface.IsInPit = e.Graphics.IsInPit;
            dataModel.Interface.CurrentSectorIndex = e.Graphics.CurrentSectorIndex;
            dataModel.Interface.LastSectorTime = e.Graphics.LastSectorTime;
            dataModel.Interface.NumberOfLaps = e.Graphics.NumberOfLaps;
            dataModel.Interface.TyreCompound = e.Graphics.TyreCompound;
            dataModel.Interface.ReplayTimeMultiplier = e.Graphics.ReplayTimeMultiplier;
            dataModel.Interface.NormalizedCarPosition = e.Graphics.NormalizedCarPosition;
        }

        private void AcOnPhysicsUpdated(object sender, PhysicsEventArgs e)
        {
            var dataModel = (AssettoCorsaDataModel)DataModel;
            dataModel.Physics.Abs = e.Physics.Abs;
            dataModel.Physics.Gas = e.Physics.Gas;
            dataModel.Physics.Brake = e.Physics.Brake;
            dataModel.Physics.Fuel = e.Physics.Fuel;
            dataModel.Physics.Gear = e.Physics.Gear;
            dataModel.Physics.Rpms = e.Physics.Rpms;
            dataModel.Physics.SteerAngle = e.Physics.SteerAngle;
            dataModel.Physics.SpeedKmh = e.Physics.SpeedKmh;
            dataModel.Physics.Velocity = e.Physics.Velocity;
            dataModel.Physics.AccG = e.Physics.AccG;
            dataModel.Physics.WheelSlip = e.Physics.WheelSlip;
            dataModel.Physics.WheelLoad = e.Physics.WheelLoad;
            dataModel.Physics.WheelsPressure = e.Physics.WheelsPressure;
            dataModel.Physics.WheelAngularSpeed = e.Physics.WheelAngularSpeed;
            dataModel.Physics.TyreWear = e.Physics.TyreWear;
            dataModel.Physics.TyreDirtyLevel = e.Physics.TyreDirtyLevel;
            dataModel.Physics.TyreCoreTemperature = e.Physics.TyreCoreTemperature;
            dataModel.Physics.CamberRad = e.Physics.CamberRad;
            dataModel.Physics.SuspensionTravel = e.Physics.SuspensionTravel;
            dataModel.Physics.Drs = e.Physics.Drs;
            dataModel.Physics.TC = e.Physics.TC;
            dataModel.Physics.Heading = e.Physics.Heading;
            dataModel.Physics.Pitch = e.Physics.Pitch;
            dataModel.Physics.Roll = e.Physics.Roll;
            dataModel.Physics.CgHeight = e.Physics.CgHeight;
            dataModel.Physics.CarDamage = e.Physics.CarDamage;
            dataModel.Physics.NumberOfTyresOut = e.Physics.NumberOfTyresOut;
            dataModel.Physics.PitLimiterOn = e.Physics.PitLimiterOn;
            dataModel.Physics.KersCharge = e.Physics.KersCharge;
            dataModel.Physics.KersInput = e.Physics.KersInput;
            dataModel.Physics.AutoShifterOn = e.Physics.AutoShifterOn;
            dataModel.Physics.RideHeight = e.Physics.RideHeight;
            dataModel.Physics.TurboBoost = e.Physics.TurboBoost;
            dataModel.Physics.Ballast = e.Physics.Ballast;
            dataModel.Physics.AirDensity = e.Physics.AirDensity;
            dataModel.Physics.AirTemp = e.Physics.AirTemp;
            dataModel.Physics.RoadTemp = e.Physics.RoadTemp;
            dataModel.Physics.LocalAngularVelocity = e.Physics.LocalAngularVelocity;
            dataModel.Physics.FinalFF = e.Physics.FinalFF;
            dataModel.Physics.PerformanceMeter = e.Physics.PerformanceMeter;
            dataModel.Physics.EngineBrake = e.Physics.EngineBrake;
            dataModel.Physics.ErsRecoveryLevel = e.Physics.ErsRecoveryLevel;
            dataModel.Physics.ErsPowerLevel = e.Physics.ErsPowerLevel;
            dataModel.Physics.ErsHeatCharging = e.Physics.ErsHeatCharging;
            dataModel.Physics.ErsisCharging = e.Physics.ErsisCharging;
            dataModel.Physics.KersCurrentKJ = e.Physics.KersCurrentKJ;
            dataModel.Physics.DrsAvailable = e.Physics.DrsAvailable;
            dataModel.Physics.DrsEnabled = e.Physics.DrsEnabled;
            dataModel.Physics.BrakeTemp = e.Physics.BrakeTemp;
            dataModel.Physics.Clutch = e.Physics.Clutch;
            dataModel.Physics.TyreTempI = e.Physics.TyreTempI;
            dataModel.Physics.TyreTempM = e.Physics.TyreTempM;
            dataModel.Physics.TyreTempO = e.Physics.TyreTempO;
            dataModel.Physics.IsAIControlled = e.Physics.IsAIControlled;
            dataModel.Physics.TyreContactPoint = e.Physics.TyreContactPoint;
            dataModel.Physics.TyreContactNormal = e.Physics.TyreContactNormal;
            dataModel.Physics.TyreContactHeading = e.Physics.TyreContactHeading;
            dataModel.Physics.BrakeBias = e.Physics.BrakeBias;
        }

        public override void Update()
        {
            // Updating is done by the events
        }
    }
}