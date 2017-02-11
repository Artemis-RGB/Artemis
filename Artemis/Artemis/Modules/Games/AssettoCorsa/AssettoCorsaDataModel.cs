using Artemis.Modules.Abstract;
using AssettoCorsaSharedMemory;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.AssettoCorsa
{
    [MoonSharpUserData]
    public class AssettoCorsaDataModel : ModuleDataModel
    {
        public AssettoCorsaDataModel()
        {
            Physics = new Physics();
            Interface = new Graphics();
            StaticInfo = new StaticInfo();
        }

        public Physics Physics { get; set; }
        public Graphics Interface { get; set; }
        public StaticInfo StaticInfo { get; set; }
    }

    [MoonSharpUserData]
    public class Physics
    {
        public float Abs { get; set; }
        public float Gas { get; set; }
        public float Brake { get; set; }
        public float Fuel { get; set; }
        public int Gear { get; set; }
        public int Rpms { get; set; }
        public float SteerAngle { get; set; }
        public float SpeedKmh { get; set; }
        public float[] Velocity { get; set; }
        public float[] AccG { get; set; }
        public float[] WheelSlip { get; set; }
        public float[] WheelLoad { get; set; }
        public float[] WheelsPressure { get; set; }
        public float[] WheelAngularSpeed { get; set; }
        public float[] TyreWear { get; set; }
        public float[] TyreDirtyLevel { get; set; }
        public float[] TyreCoreTemperature { get; set; }
        public float[] CamberRad { get; set; }
        public float[] SuspensionTravel { get; set; }
        public float Drs { get; set; }
        public float TC { get; set; }
        public float Heading { get; set; }
        public float Pitch { get; set; }
        public float Roll { get; set; }
        public float CgHeight { get; set; }
        public float[] CarDamage { get; set; }
        public int NumberOfTyresOut { get; set; }
        public int PitLimiterOn { get; set; }
        public float KersCharge { get; set; }
        public float KersInput { get; set; }
        public int AutoShifterOn { get; set; }
        public float[] RideHeight { get; set; }
        public float TurboBoost { get; set; }
        public float Ballast { get; set; }
        public float AirDensity { get; set; }
        public float AirTemp { get; set; }
        public float RoadTemp { get; set; }
        public float[] LocalAngularVelocity { get; set; }
        public float FinalFF { get; set; }
        public float PerformanceMeter { get; set; }
        public int EngineBrake { get; set; }
        public int ErsRecoveryLevel { get; set; }
        public int ErsPowerLevel { get; set; }
        public int ErsHeatCharging { get; set; }
        public int ErsisCharging { get; set; }
        public float KersCurrentKJ { get; set; }
        public int DrsAvailable { get; set; }
        public int DrsEnabled { get; set; }
        public float[] BrakeTemp { get; set; }
        public float Clutch { get; set; }
        public float[] TyreTempI { get; set; }
        public float[] TyreTempM { get; set; }
        public float[] TyreTempO { get; set; }
        public int IsAIControlled { get; set; }
        public Coordinates[] TyreContactPoint { get; set; }
        public Coordinates[] TyreContactNormal { get; set; }
        public Coordinates[] TyreContactHeading { get; set; }
        public float BrakeBias { get; set; }
    }

    [MoonSharpUserData]
    public class Graphics
    {
        public AC_STATUS Status { get; set; }
        public AC_SESSION_TYPE Session { get; set; }
        public string CurrentTime { get; set; }
        public string LastTime { get; set; }
        public string BestTime { get; set; }
        public string Split { get; set; }
        public int Position { get; set; }
        public int iCurrentTime { get; set; }
        public int iBestTime { get; set; }
        public int iLastTime { get; set; }
        public float SessionTimeLeft { get; set; }
        public float DistanceTraveled { get; set; }
        public int IsInPit { get; set; }
        public int CurrentSectorIndex { get; set; }
        public int LastSectorTime { get; set; }
        public int NumberOfLaps { get; set; }
        public string TyreCompound { get; set; }
        public float ReplayTimeMultiplier { get; set; }
        public float NormalizedCarPosition { get; set; }
    }

    [MoonSharpUserData]
    public class StaticInfo
    {
        public StaticInfo()
        {
            Session = new Session();
            Car = new Car();
        }

        public string SMVersion { get; set; }
        public string ACVersion { get; set; }
        public Session Session { get; set; }
        public Car Car { get; set; }
    }

    [MoonSharpUserData]
    public class Session
    {
        public int NumberOfSessions { get; set; }
        public int NumCars { get; set; }
        public string Track { get; set; }
        public string TrackConfiguration { get; set; }
        public float TrackSPlineLength { get; set; }
        public string PlayerName { get; set; }
        public string PlayerSurname { get; set; }
        public string PlayerNick { get; set; }
        public int SectorCount { get; set; }
        public int PenaltiesEnabled { get; set; }
    }

    [MoonSharpUserData]
    public class Car
    {
        public string CarModel { get; set; }
        public float MaxTorque { get; set; }
        public float MaxPower { get; set; }
        public int MaxRpm { get; set; }
        public float MaxFuel { get; set; }
        public float[] SuspensionMaxTravel { get; set; }
        public float[] TyreRadius { get; set; }
        public float MaxTurboBoost { get; set; }
        public float AidFuelRate { get; set; }
        public float AidTireRate { get; set; }
        public float AidMechanicalDamage { get; set; }
        public int AidAllowTyreBlankets { get; set; }
        public float AidStability { get; set; }
        public int AidAutoClutch { get; set; }
        public int AidAutoBlip { get; set; }
        public int HasDRS { get; set; }
        public int HasERS { get; set; }
        public int HasKERS { get; set; }
        public float KersMaxJoules { get; set; }
        public int EngineBrakeSettingsCount { get; set; }
        public int ErsPowerControllerCount { get; set; }
    }
}