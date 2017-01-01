using System;
using System.Text;
using Artemis.Modules.Games.EurotruckSimulator2.Data.Reader;

namespace Artemis.Modules.Games.EurotruckSimulator2.Data
{
    public class Ets2TelemetryData : IEts2TelemetryData
    {
        private Box<Ets2TelemetryStructure> _rawData;

        public IEts2Game Game => new Ets2Game(_rawData);
        public IEts2Truck Truck => new Ets2Truck(_rawData);
        public IEts2Trailer Trailer => new Ets2Trailer(_rawData);
        public IEts2Job Job => new Ets2Job(_rawData);
        public IEts2Navigation Navigation => new Ets2Navigation(_rawData);

        public void Update(Ets2TelemetryStructure rawData)
        {
            _rawData = new Box<Ets2TelemetryStructure>(rawData);
        }

        internal static DateTime SecondsToDate(int seconds)
        {
            if (seconds < 0) seconds = 0;
            return new DateTime((long) seconds*10000000, DateTimeKind.Utc);
        }

        internal static DateTime MinutesToDate(int minutes)
        {
            return SecondsToDate(minutes*60);
        }

        internal static string BytesToString(byte[] bytes)
        {
            if (bytes == null)
                return string.Empty;
            return Encoding.UTF8.GetString(bytes, 0, Array.FindIndex(bytes, b => b == 0));
        }
    }

    public class Ets2Game : IEts2Game
    {
        private readonly Box<Ets2TelemetryStructure> _rawData;

        public Ets2Game(Box<Ets2TelemetryStructure> rawData)
        {
            _rawData = rawData;
        }
        
        public bool Paused => _rawData.Struct.paused != 0;
        public DateTime Time => Ets2TelemetryData.MinutesToDate(_rawData.Struct.timeAbsolute);
        public float TimeScale => _rawData.Struct.localScale;
        public DateTime NextRestStopTime => Ets2TelemetryData.MinutesToDate(_rawData.Struct.nextRestStop);
        public string Version => $"{_rawData.Struct.ets2_version_major}.{_rawData.Struct.ets2_version_minor}";
        public string TelemetryPluginVersion => _rawData.Struct.ets2_telemetry_plugin_revision.ToString();
    }

    public class Ets2Vector : IEts2Vector
    {
        public Ets2Vector(float x, float y, float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
    }

    public class Ets2Placement : IEts2Placement
    {
        public Ets2Placement(float x, float y, float z,
            float heading, float pitch, float roll)
        {
            X = x;
            Y = y;
            Z = z;
            Heading = heading;
            Pitch = pitch;
            Roll = roll;
        }

        public float X { get; }
        public float Y { get; }
        public float Z { get; }
        public float Heading { get; }
        public float Pitch { get; }
        public float Roll { get; }
    }

    public class Ets2Truck : IEts2Truck
    {
        private readonly Box<Ets2TelemetryStructure> _rawData;

        public Ets2Truck(Box<Ets2TelemetryStructure> rawData)
        {
            _rawData = rawData;
        }

        public string Id => Ets2TelemetryData.BytesToString(_rawData.Struct.truckMakeId);
        public string Make => Ets2TelemetryData.BytesToString(_rawData.Struct.truckMake);
        public string Model => Ets2TelemetryData.BytesToString(_rawData.Struct.truckModel);

        /// <summary>
        ///     Truck speed in km/h.
        /// </summary>
        public float Speed => _rawData.Struct.speed*3.6f;

        /// <summary>
        ///     Cruise control speed in km/h.
        /// </summary>
        public float CruiseControlSpeed => _rawData.Struct.cruiseControlSpeed*3.6f;

        public bool CruiseControlOn => _rawData.Struct.cruiseControl != 0;
        public float Odometer => _rawData.Struct.truckOdometer;
        public int Gear => _rawData.Struct.gear;
        public int DisplayedGear => _rawData.Struct.displayedGear;
        public int ForwardGears => _rawData.Struct.gearsForward;
        public int ReverseGears => _rawData.Struct.gearsReverse;
        public string ShifterType => Ets2TelemetryData.BytesToString(_rawData.Struct.shifterType);
        public float EngineRpm => _rawData.Struct.engineRpm;
        public float EngineRpmMax => _rawData.Struct.engineRpmMax;
        public float Fuel => _rawData.Struct.fuel;
        public float FuelCapacity => _rawData.Struct.fuelCapacity;
        public float FuelAverageConsumption => _rawData.Struct.fuelAvgConsumption;
        public float FuelWarningFactor => _rawData.Struct.fuelWarningFactor;
        public bool FuelWarningOn => _rawData.Struct.fuelWarning != 0;
        public float WearEngine => _rawData.Struct.wearEngine;
        public float WearTransmission => _rawData.Struct.wearTransmission;
        public float WearCabin => _rawData.Struct.wearCabin;
        public float WearChassis => _rawData.Struct.wearChassis;
        public float WearWheels => _rawData.Struct.wearWheels;
        public float UserSteer => _rawData.Struct.userSteer;
        public float UserThrottle => _rawData.Struct.userThrottle;
        public float UserBrake => _rawData.Struct.userBrake;
        public float UserClutch => _rawData.Struct.userClutch;
        public float GameSteer => _rawData.Struct.gameSteer;
        public float GameThrottle => _rawData.Struct.gameThrottle;
        public float GameBrake => _rawData.Struct.gameBrake;
        public float GameClutch => _rawData.Struct.gameClutch;
        public int ShifterSlot => _rawData.Struct.shifterSlot;

        //public int ShifterToggle
        //{
        //    get { return _rawData.Struct.shifterToggle; }
        //}

        public bool EngineOn => _rawData.Struct.engineEnabled != 0;
        public bool ElectricOn => _rawData.Struct.electricEnabled != 0;
        public bool WipersOn => _rawData.Struct.wipers != 0;
        public int RetarderBrake => _rawData.Struct.retarderBrake;
        public int RetarderStepCount => (int) _rawData.Struct.retarderStepCount;
        public bool ParkBrakeOn => _rawData.Struct.parkBrake != 0;
        public bool MotorBrakeOn => _rawData.Struct.motorBrake != 0;
        public float BrakeTemperature => _rawData.Struct.brakeTemperature;
        public float Adblue => _rawData.Struct.adblue;
        public float AdblueCapacity => _rawData.Struct.adblueCapacity;
        public float AdblueAverageConsumption => _rawData.Struct.adblueConsumption;
        public bool AdblueWarningOn => _rawData.Struct.adblueWarning != 0;
        public float AirPressure => _rawData.Struct.airPressure;
        public bool AirPressureWarningOn => _rawData.Struct.airPressureWarning != 0;
        public float AirPressureWarningValue => _rawData.Struct.airPressureWarningValue;
        public bool AirPressureEmergencyOn => _rawData.Struct.airPressureEmergency != 0;
        public float AirPressureEmergencyValue => _rawData.Struct.airPressureEmergencyValue;
        public float OilTemperature => _rawData.Struct.oilTemperature;
        public float OilPressure => _rawData.Struct.oilPressure;
        public bool OilPressureWarningOn => _rawData.Struct.oilPressureWarning != 0;
        public float OilPressureWarningValue => _rawData.Struct.oilPressureWarningValue;
        public float WaterTemperature => _rawData.Struct.waterTemperature;
        public bool WaterTemperatureWarningOn => _rawData.Struct.waterTemperatureWarning != 0;
        public float WaterTemperatureWarningValue => _rawData.Struct.waterTemperatureWarningValue;
        public float BatteryVoltage => _rawData.Struct.batteryVoltage;
        public bool BatteryVoltageWarningOn => _rawData.Struct.batteryVoltageWarning != 0;
        public float BatteryVoltageWarningValue => _rawData.Struct.batteryVoltageWarningValue;
        public float LightsDashboardValue => _rawData.Struct.lightsDashboard;
        public bool LightsDashboardOn => _rawData.Struct.lightsDashboard > 0;
        public bool BlinkerLeftActive => _rawData.Struct.blinkerLeftActive != 0;
        public bool BlinkerRightActive => _rawData.Struct.blinkerRightActive != 0;
        public bool BlinkerLeftOn => _rawData.Struct.blinkerLeftOn != 0;
        public bool BlinkerRightOn => _rawData.Struct.blinkerRightOn != 0;
        public bool LightsParkingOn => _rawData.Struct.lightsParking != 0;
        public bool LightsBeamLowOn => _rawData.Struct.lightsBeamLow != 0;
        public bool LightsBeamHighOn => _rawData.Struct.lightsBeamHigh != 0;
        public bool LightsAuxFrontOn => _rawData.Struct.lightsAuxFront != 0;
        public bool LightsAuxRoofOn => _rawData.Struct.lightsAuxRoof != 0;
        public bool LightsBeaconOn => _rawData.Struct.lightsBeacon != 0;
        public bool LightsBrakeOn => _rawData.Struct.lightsBrake != 0;
        public bool LightsReverseOn => _rawData.Struct.lightsReverse != 0;

        public IEts2Placement Placement => new Ets2Placement(
            _rawData.Struct.coordinateX,
            _rawData.Struct.coordinateY,
            _rawData.Struct.coordinateZ,
            _rawData.Struct.rotationX,
            _rawData.Struct.rotationY,
            _rawData.Struct.rotationZ);

        public IEts2Vector Acceleration => new Ets2Vector(
            _rawData.Struct.accelerationX,
            _rawData.Struct.accelerationY,
            _rawData.Struct.accelerationZ);

        public IEts2Vector Head => new Ets2Vector(
            _rawData.Struct.headPositionX,
            _rawData.Struct.headPositionY,
            _rawData.Struct.headPositionZ);

        public IEts2Vector Cabin => new Ets2Vector(
            _rawData.Struct.cabinPositionX,
            _rawData.Struct.cabinPositionY,
            _rawData.Struct.cabinPositionZ);

        public IEts2Vector Hook => new Ets2Vector(
            _rawData.Struct.hookPositionX,
            _rawData.Struct.hookPositionY,
            _rawData.Struct.hookPositionZ);

        /*
        public IEts2GearSlot[] GearSlots
        {
            get
            {
                var array = new IEts2GearSlot[_rawData.Struct.selectorCount];
                for (int i = 0; i < array.Length; i++)
                    array[i] = new Ets2GearSlot(_rawData, i);
                return array;
            }
        }
                
        public IEts2Wheel[] Wheels
        {
            get
            {
                var array = new IEts2Wheel[_rawData.Struct.wheelCount];
                for (int i = 0; i < array.Length; i++)
                    array[i] = new Ets2Wheel(_rawData, i);
                return array;
            }
        }
        */
    }

    public class Ets2Trailer : IEts2Trailer
    {
        private readonly Box<Ets2TelemetryStructure> _rawData;

        public Ets2Trailer(Box<Ets2TelemetryStructure> rawData)
        {
            _rawData = rawData;
        }

        public bool Attached => _rawData.Struct.trailer_attached != 0;
        public string Id => Ets2TelemetryData.BytesToString(_rawData.Struct.trailerId);
        public string Name => Ets2TelemetryData.BytesToString(_rawData.Struct.trailerName);

        /// <summary>
        ///     Trailer mass in kilograms.
        /// </summary>
        public float Mass => _rawData.Struct.trailerMass;

        public float Wear => _rawData.Struct.wearTrailer;

        public IEts2Placement Placement => new Ets2Placement(
            _rawData.Struct.trailerCoordinateX,
            _rawData.Struct.trailerCoordinateY,
            _rawData.Struct.trailerCoordinateZ,
            _rawData.Struct.trailerRotationX,
            _rawData.Struct.trailerRotationY,
            _rawData.Struct.trailerRotationZ);
    }

    public class Ets2Navigation : IEts2Navigation
    {
        private readonly Box<Ets2TelemetryStructure> _rawData;

        public Ets2Navigation(Box<Ets2TelemetryStructure> rawData)
        {
            _rawData = rawData;
        }

        public DateTime EstimatedTime => Ets2TelemetryData.SecondsToDate((int) _rawData.Struct.navigationTime);
        public int EstimatedDistance => (int) _rawData.Struct.navigationDistance;

        public int SpeedLimit
            =>
            _rawData.Struct.navigationSpeedLimit > 0 ? (int) Math.Round(_rawData.Struct.navigationSpeedLimit*3.6f) : 0;
    }

    public class Ets2Job : IEts2Job
    {
        private readonly Box<Ets2TelemetryStructure> _rawData;

        public Ets2Job(Box<Ets2TelemetryStructure> rawData)
        {
            _rawData = rawData;
        }

        public int Income => _rawData.Struct.jobIncome;
        public DateTime DeadlineTime => Ets2TelemetryData.MinutesToDate(_rawData.Struct.jobDeadline);

        public DateTime RemainingTime
        {
            get
            {
                if (_rawData.Struct.jobDeadline > 0)
                    return Ets2TelemetryData.MinutesToDate(_rawData.Struct.jobDeadline - _rawData.Struct.timeAbsolute);
                return Ets2TelemetryData.MinutesToDate(0);
            }
        }

        public string SourceCity => Ets2TelemetryData.BytesToString(_rawData.Struct.jobCitySource);
        public string SourceCompany => Ets2TelemetryData.BytesToString(_rawData.Struct.jobCompanySource);
        public string DestinationCity => Ets2TelemetryData.BytesToString(_rawData.Struct.jobCityDestination);
        public string DestinationCompany => Ets2TelemetryData.BytesToString(_rawData.Struct.jobCompanyDestination);
    }

    /*
    class Ets2Wheel : IEts2Wheel
    {
        public Ets2Wheel(Box<Ets2TelemetryStructure> rawData, int wheelIndex)
        {
            Simulated = rawData.Struct.wheelSimulated[wheelIndex] != 0;
            Steerable = rawData.Struct.wheelSteerable[wheelIndex] != 0;
            Radius = rawData.Struct.wheelRadius[wheelIndex];
            Position = new Ets2Vector(
                rawData.Struct.wheelPositionX[wheelIndex],
                rawData.Struct.wheelPositionY[wheelIndex],
                rawData.Struct.wheelPositionZ[wheelIndex]);
            Powered = rawData.Struct.wheelPowered[wheelIndex] != 0;
            Liftable = rawData.Struct.wheelLiftable[wheelIndex] != 0;
        }

        public bool Simulated { get; private set; }
        public bool Steerable { get; private set; }
        public bool Powered { get; private set; }
        public bool Liftable { get; private set; }
        public float Radius { get; private set; }
        public IEts2Vector Position { get; private set; }
    }
    
    class Ets2GearSlot : IEts2GearSlot
    {
        public Ets2GearSlot(Box<Ets2TelemetryStructure> rawData, int slotIndex)
        {
            Gear = rawData.Struct.slotGear[slotIndex];
            HandlePosition = (int)rawData.Struct.slotHandlePosition[slotIndex];
            SlotSelectors = (int)rawData.Struct.slotSelectors[slotIndex];
        }

        public int Gear { get; private set; }
        public int HandlePosition { get; private set; }
        public int SlotSelectors { get; private set; }
    }
    */

    public class Box<T> where T : struct
    {
        public Box(T @struct)
        {
            Struct = @struct;
        }

        public T Struct { get; set; }
    }
}