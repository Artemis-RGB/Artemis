using System;

namespace Artemis.Modules.Games.EurotruckSimulator2.Data
{
    public interface IEts2TelemetryData
    {
        /// <summary>
        ///     Game information.
        /// </summary>
        IEts2Game Game { get; }

        /// <summary>
        ///     Truck information.
        /// </summary>
        IEts2Truck Truck { get; }

        /// <summary>
        ///     Trailer information.
        /// </summary>
        IEts2Trailer Trailer { get; }

        /// <summary>
        ///     Job information.
        /// </summary>
        IEts2Job Job { get; }

        /// <summary>
        ///     Navigation information.
        /// </summary>
        IEts2Navigation Navigation { get; }
    }

    public interface IEts2Game
    {
        /// <summary>
        ///     Current game time.
        ///     Serializes to ISO 8601 string in JSON.
        ///     Example: "0001-01-05T05:11:00Z"
        /// </summary>
        DateTime Time { get; }

        /// <summary>
        ///     True if game is currently paused, false otherwise.
        /// </summary>
        bool Paused { get; }

        /// <summary>
        ///     Current version of the telemetry plugin DLL file.
        ///     Example: "4"
        /// </summary>
        string TelemetryPluginVersion { get; }

        /// <summary>
        ///     Current version of the game.
        ///     Example: "1.10"
        /// </summary>
        string Version { get; }

        /// <summary>
        ///     When the fatigue simulation is disabled, the behavior of this channel
        ///     is implementation dependent. The game might provide the value which would
        ///     apply if it was enabled or provide no value at all.
        ///     Example: "0001-01-01T10:52:00Z"
        /// </summary>
        DateTime NextRestStopTime { get; }

        /// <summary>
        ///     Scale applied to distance and time to compensate
        ///     for the scale of the map (e.g. 1s of real time corresponds
        ///     to local_scale seconds of simulated game time).
        ///     Example: 3
        /// </summary>
        float TimeScale { get; }
    }

    public interface IEts2Vector
    {
        float X { get; }
        float Y { get; }
        float Z { get; }
    }

    public interface IEts2Placement
    {
        /// <summary>
        ///     X coordinate of the placement.
        ///     Example: 13723.7881
        /// </summary>
        float X { get; }

        /// <summary>
        ///     Y coordinate of the placement.
        ///     Example: 65.22377
        /// </summary>
        float Y { get; }

        /// <summary>
        ///     Z coordinate of the placement.
        ///     Example: 13738.8018
        /// </summary>
        float Z { get; }

        /// <summary>
        ///     The angle is measured counterclockwise in horizontal plane when looking
        ///     from top where 0 corresponds to forward (north), 0.25 to left (west),
        ///     0.5 to backward (south) and 0.75 to right (east).
        ///     Stored in unit range where (0,1) corresponds to (0,360).
        ///     Example: 0.13688866
        /// </summary>
        float Heading { get; }

        /// <summary>
        ///     The pitch angle is zero when in horizontal direction,
        ///     with positive values pointing up (0.25 directly to zenith),
        ///     and negative values pointing down (-0.25 directly to nadir).
        ///     Stored in unit range where (-0.25,0.25) corresponds to (-90,90).
        ///     Example: 0.00005
        /// </summary>
        float Pitch { get; }

        /// <summary>
        ///     The angle is measured in counterclockwise when looking in direction of the roll axis.
        ///     Stored in unit range where (-0.5,0.5) corresponds to (-180,180).
        ///     Example: -0.00009
        /// </summary>
        float Roll { get; }
    }

    public interface IEts2Truck
    {
        /// <summary>
        ///     Current truck speed in km/h.
        ///     Example: 50.411231
        /// </summary>
        float Speed { get; }

        /// <summary>
        ///     Represents vehicle space linear acceleration of
        ///     the truck measured in meters per second^2.
        ///     Example: { "x": 0.046569, "y": -0.00116, "z": -1.03676 }
        /// </summary>
        IEts2Vector Acceleration { get; }

        /// <summary>
        ///     Current truck placement in the game world.
        /// </summary>
        IEts2Placement Placement { get; }

        /// <summary>
        ///     The value of the odometer in km.
        ///     Example: 105809.25
        /// </summary>
        float Odometer { get; }

        /// <summary>
        ///     Speed selected for the cruise control in km/h.
        ///     Example: 75
        /// </summary>
        float CruiseControlSpeed { get; }

        /// <summary>
        ///     Brand Id of the current truck.
        ///     Example: "man".
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Localized brand name of the current truck for display purposes.
        ///     Example: "MAN".
        /// </summary>
        string Make { get; }

        /// <summary>
        ///     Localized model name of the current truck.
        ///     Example: "TGX".
        /// </summary>
        string Model { get; }

        /// <summary>
        ///     Gear that is currently selected in the engine.
        ///     Positive values reflect forward gears, negative - reverse.
        ///     Example: 9
        /// </summary>
        int Gear { get; }

        /// <summary>
        ///     Gear that is currently displayed on the main dashboard.
        ///     Positive values reflect forward gears, negative - reverse.
        ///     Example: 4
        /// </summary>
        int DisplayedGear { get; }

        /// <summary>
        ///     Number of forward gears on undamaged truck.
        ///     Example: 12
        /// </summary>
        int ForwardGears { get; }

        /// <summary>
        ///     Number of reverse gears on undamaged truck.
        ///     Example: 2
        /// </summary>
        int ReverseGears { get; }

        /// <summary>
        ///     Current RPM value of the truck's engine (rotates per minute).
        ///     Example: 1372.3175
        /// </summary>
        float EngineRpm { get; }

        /// <summary>
        ///     Maximal RPM value of the truck's engine.
        ///     Example: 2500
        /// </summary>
        float EngineRpmMax { get; }

        /// <summary>
        ///     Current amount of fuel in liters.
        ///     Example: 696.7544
        /// </summary>
        float Fuel { get; }

        /// <summary>
        ///     Fuel tank capacity in litres.
        ///     Example: 700
        /// </summary>
        float FuelCapacity { get; }

        /// <summary>
        ///     Average consumption of the fuel in liters/km.
        ///     Example: 0.4923077
        /// </summary>
        float FuelAverageConsumption { get; }

        /// <summary>
        ///     Steering received from input (-1;1).
        ///     Note that it is interpreted counterclockwise.
        ///     If the user presses the steer right button on digital input
        ///     (e.g. keyboard) this value goes immediatelly to -1.0
        ///     Example: -1.0
        /// </summary>
        float UserSteer { get; }

        /// <summary>
        ///     Throttle received from input (-1;1).
        ///     If the user presses the forward button on digital input
        ///     (e.g. keyboard) this value goes immediatelly to 1.0
        ///     Example: 0
        /// </summary>
        float UserThrottle { get; }

        /// <summary>
        ///     Brake received from input (-1;1)
        ///     If the user presses the brake button on digital input
        ///     (e.g. keyboard) this value goes immediatelly to 1.0
        ///     Example: 0
        /// </summary>
        float UserBrake { get; }

        /// <summary>
        ///     Clutch received from input (-1;1)
        ///     If the user presses the clutch button on digital input
        ///     (e.g. keyboard) this value goes immediatelly to 1.0
        ///     Example: 0
        /// </summary>
        float UserClutch { get; }

        /// <summary>
        ///     Steering as used by the simulation (-1;1)
        ///     Note that it is interpreted counterclockwise.
        ///     Accounts for interpolation speeds and simulated
        ///     counterfoces for digital inputs.
        ///     Example: -0.423521
        /// </summary>
        float GameSteer { get; }

        /// <summary>
        ///     Throttle pedal input as used by the simulation (0;1)
        ///     Accounts for the press attack curve for digital inputs
        ///     or cruise-control input.
        ///     Example: 0.17161
        /// </summary>
        float GameThrottle { get; }

        /// <summary>
        ///     Brake pedal input as used by the simulation (0;1)
        ///     Accounts for the press attack curve for digital inputs.
        ///     Does not contain retarder, parking or motor brake.
        ///     Example: 0
        /// </summary>
        float GameBrake { get; }

        /// <summary>
        ///     Clutch pedal input as used by the simulation (0;1)
        ///     Accounts for the automatic shifting or interpolation of
        ///     player input.
        ///     Example: 0
        /// </summary>
        float GameClutch { get; }

        /// <summary>
        ///     Current level of the retarder brake.
        ///     Ranges from 0 to RetarderStepCount.
        ///     Example: 0
        /// </summary>
        int RetarderBrake { get; }

        /// <summary>
        ///     Number of steps in the retarder.
        ///     Set to zero if retarder is not mounted to the truck.
        ///     Example: 3
        /// </summary>
        int RetarderStepCount { get; }

        /// <summary>
        ///     Gearbox slot the h-shifter handle is currently in.
        ///     0 means that no slot is selected.
        ///     Example: 0
        /// </summary>
        int ShifterSlot { get; }

        /// <summary>
        ///     TODO: need to fix.
        /// </summary>
        /// <summary>
        ///     Pressure in the brake air tank in psi.
        ///     Example: 133.043961
        /// </summary>
        float AirPressure { get; }

        /// <summary>
        ///     Temperature of the brakes in degrees celsius.
        ///     Example: 15.9377184
        /// </summary>
        float BrakeTemperature { get; }

        /// <summary>
        ///     Amount of AdBlue in liters.
        ///     Example: 0
        /// </summary>
        float Adblue { get; }

        /// <summary>
        ///     Average consumption of the adblue in liters/km.
        ///     Example: 0
        /// </summary>
        float AdblueAverageConsumption { get; }

        /// <summary>
        ///     Pressure of the oil in psi.
        ///     Example: 36.4550362
        /// </summary>
        float OilPressure { get; }

        /// <summary>
        ///     Temperature of the oil in degrees celsius.
        ///     Example: 16.2580566
        /// </summary>
        float OilTemperature { get; }

        /// <summary>
        ///     Temperature of the water in degrees celsius.
        ///     Example: 16.1623955
        /// </summary>
        float WaterTemperature { get; }

        /// <summary>
        ///     Voltage of the battery in volts.
        ///     Example: 23.4985161
        /// </summary>
        float BatteryVoltage { get; }

        /// <summary>
        ///     AdBlue tank capacity in litres.
        ///     Example: 0
        /// </summary>
        float AdblueCapacity { get; }

        /// <summary>
        ///     Current level of truck's engine wear/damage between 0 (min) and 1 (max).
        ///     Example: 0.00675457
        /// </summary>
        float WearEngine { get; }

        /// <summary>
        ///     Current level of truck's transmission wear/damage between 0 (min) and 1 (max).
        /// </summary>
        float WearTransmission { get; }

        /// <summary>
        ///     Current level of truck's cabin wear/damage between 0 (min) and 1 (max).
        /// </summary>
        float WearCabin { get; }

        /// <summary>
        ///     Current level of truck's chassis wear/damage between 0 (min) and 1 (max).
        /// </summary>
        float WearChassis { get; }

        /// <summary>
        ///     Current level of truck's wheel wear/damage between 0 (min) and 1 (max).
        /// </summary>
        float WearWheels { get; }

        /// <summary>
        ///     Default position of the head in the cabin space.
        ///     Example: { "x": -0.795116067, "y": 1.43522251, "z": -0.08483863 }
        /// </summary>
        IEts2Vector Head { get; }

        /// <summary>
        ///     Position of the cabin in the vehicle space.
        ///     This is position of the joint around which the cabin rotates.
        ///     This attribute might be not present if the vehicle does not have a separate cabin.
        ///     Example: { "x": 0, "y": 1.36506855, "z": -1.70362806 }
        /// </summary>
        IEts2Vector Cabin { get; }

        /// <summary>
        ///     Position of the trailer connection hook in vehicle space.
        ///     Example: { "x": 0, "y": 0.939669, "z": -6.17736959 }
        /// </summary>
        IEts2Vector Hook { get; }

        /// <summary>
        ///     All available selectors (e.g. range/splitter toggles). TODO: need to fix.
        /// </summary>
        /// <summary>
        ///     Type of the shifter.
        ///     One of the following values: "arcade", "automatic", "manual", "hshifter".
        /// </summary>
        string ShifterType { get; }

        /// <summary>
        ///     Indicates whether cruise control is turned on or off.
        /// </summary>
        bool CruiseControlOn { get; }

        /// <summary>
        ///     Indicates whether wipers are currently turned on or off.
        /// </summary>
        bool WipersOn { get; }

        /// <summary>
        ///     Is the parking brake enabled or not.
        /// </summary>
        bool ParkBrakeOn { get; }

        /// <summary>
        ///     Is the motor brake enabled or not.
        /// </summary>
        bool MotorBrakeOn { get; }

        /// <summary>
        ///     Is the engine enabled or not.
        /// </summary>
        bool EngineOn { get; }

        /// <summary>
        ///     Is the electric enabled or not.
        /// </summary>
        bool ElectricOn { get; }

        /// <summary>
        ///     Is left blinker currently emits light or not.
        /// </summary>
        bool BlinkerLeftActive { get; }

        /// <summary>
        ///     Is right blinker currently emits light or not.
        /// </summary>
        bool BlinkerRightActive { get; }

        /// <summary>
        ///     Is left blinker currently turned on or off.
        /// </summary>
        bool BlinkerLeftOn { get; }

        /// <summary>
        ///     Is right blinker currently turned on or off.
        /// </summary>
        bool BlinkerRightOn { get; }

        /// <summary>
        ///     Are the parking lights enabled or not.
        /// </summary>
        bool LightsParkingOn { get; }

        /// <summary>
        ///     Are the low beam lights enabled or not.
        /// </summary>
        bool LightsBeamLowOn { get; }

        /// <summary>
        ///     Are the high beam lights enabled or not.
        /// </summary>
        bool LightsBeamHighOn { get; }

        /// <summary>
        ///     Are the auxiliary front lights active or not.
        /// </summary>
        bool LightsAuxFrontOn { get; }

        /// <summary>
        ///     Are the auxiliary roof lights active or not.
        /// </summary>
        bool LightsAuxRoofOn { get; }

        /// <summary>
        ///     Are the beacon lights enabled or not.
        /// </summary>
        bool LightsBeaconOn { get; }

        /// <summary>
        ///     Is the brake light active or not.
        /// </summary>
        bool LightsBrakeOn { get; }

        /// <summary>
        ///     Is the reverse light active or not.
        /// </summary>
        bool LightsReverseOn { get; }

        /// <summary>
        ///     Is the battery voltage/not charging warning active or not.
        /// </summary>
        bool BatteryVoltageWarningOn { get; }

        /// <summary>
        ///     Is the air pressure warning active or not.
        /// </summary>
        bool AirPressureWarningOn { get; }

        /// <summary>
        ///     Are the emergency brakes active as result of low air pressure or not.
        /// </summary>
        bool AirPressureEmergencyOn { get; }

        /// <summary>
        ///     Is the low adblue warning active or not.
        /// </summary>
        bool AdblueWarningOn { get; }

        /// <summary>
        ///     Is the oil pressure warning active or not.
        /// </summary>
        bool OilPressureWarningOn { get; }

        /// <summary>
        ///     Is the water temperature warning active or not.
        /// </summary>
        bool WaterTemperatureWarningOn { get; }

        /// <summary>
        ///     Intensity of the dashboard backlight between 0 (off) and 1 (max).
        /// </summary>
        float LightsDashboardValue { get; }

        /// <summary>
        ///     Is the dashboard backlight currently turned on or off.
        /// </summary>
        bool LightsDashboardOn { get; }

        /// <summary>
        ///     Is the low fuel warning active or not.
        /// </summary>
        bool FuelWarningOn { get; }

        /// <summary>
        ///     Fraction of the fuel capacity bellow which is activated the fuel warning.
        ///     Example: 0.15
        /// </summary>
        float FuelWarningFactor { get; }

        /// <summary>
        ///     Pressure of the air in the tank bellow which the warning activates.
        ///     Example: 65
        /// </summary>
        float AirPressureWarningValue { get; }

        /// <summary>
        ///     Pressure of the air in the tank bellow which the emergency brakes activate.
        ///     Example: 30
        /// </summary>
        float AirPressureEmergencyValue { get; }

        /// <summary>
        ///     Pressure of the oil bellow which the warning activates.
        ///     Example: 10
        /// </summary>
        float OilPressureWarningValue { get; }

        /// <summary>
        ///     Temperature of the water above which the warning activates.
        ///     Example: 105
        /// </summary>
        float WaterTemperatureWarningValue { get; }

        /// <summary>
        ///     Voltage of the battery bellow which the warning activates.
        ///     Example: 22
        /// </summary>
        float BatteryVoltageWarningValue { get; }
    }

    public interface IEts2Navigation
    {
        /// <summary>
        ///     Relative estimated time of arrival.
        ///     Example: "0001-01-01T02:05:00Z"
        /// </summary>
        DateTime EstimatedTime { get; }

        /// <summary>
        ///     Estimated distance to the destination in meters.
        ///     Example: 1224
        /// </summary>
        int EstimatedDistance { get; }

        /// <summary>
        ///     Current value of the "Route Advisor speed limit" in km/h.
        ///     Example: 50
        /// </summary>
        int SpeedLimit { get; }
    }

    public interface IEts2Job
    {
        /// <summary>
        ///     Reward in internal game-specific currency.
        ///     Example: 2316
        /// </summary>
        int Income { get; }

        /// <summary>
        ///     Absolute in-game time of end of job delivery window.
        ///     Delivering the job after this time will cause it be late.
        ///     Example: "0001-01-09T03:34:00Z"
        /// </summary>
        DateTime DeadlineTime { get; }

        /// <summary>
        ///     Relative remaining in-game time left before deadline.
        ///     Example: "0001-01-01T07:06:00Z"
        /// </summary>
        DateTime RemainingTime { get; }

        /// <summary>
        ///     Localized name of the source city for display purposes.
        ///     Example: "Linz"
        /// </summary>
        string SourceCity { get; }

        /// <summary>
        ///     Localized name of the destination city for display purposes.
        ///     Example: "Salzburg"
        /// </summary>
        string DestinationCity { get; }

        /// <summary>
        ///     Localized name of the source company for display purposes.
        ///     Example: "DHL"
        /// </summary>
        string SourceCompany { get; }

        /// <summary>
        ///     Localized name of the destination company for display purposes.
        ///     Example: "JCB"
        /// </summary>
        string DestinationCompany { get; }
    }

    public interface IEts2Trailer
    {
        /// <summary>
        ///     Id of the cargo for internal use by code.
        ///     Example: "derrick"
        /// </summary>
        string Id { get; }

        /// <summary>
        ///     Localized name of the current trailer for display purposes.
        ///     Example: "Derrick"
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Is the trailer attached to the truck or not.
        /// </summary>
        bool Attached { get; }

        /// <summary>
        ///     Mass of the cargo in kilograms.
        ///     Example: 22000
        /// </summary>
        float Mass { get; }

        /// <summary>
        ///     Current trailer placement in the game world.
        /// </summary>
        IEts2Placement Placement { get; }

        /// <summary>
        ///     Current level of trailer wear/damage between 0 (min) and 1 (max).
        ///     Example: 0.0314717
        /// </summary>
        float Wear { get; }
    }

    /*
    public interface IEts2Wheel
    {
        /// <summary>
        /// Is the wheel physically simulated or not.
        /// </summary>
        bool Simulated { get; }
        /// <summary>
        /// Is the wheel steerable or not.
        /// </summary>
        bool Steerable { get; }
        /// <summary>
        /// Radius of the wheel.
        /// Example: 0.5120504
        /// </summary>
        float Radius { get; }
        /// <summary>
        /// Position of respective wheels in the vehicle space.
        /// Example: { "x": -0.9, "y": 0.506898463, "z": 6.25029 }
        /// </summary>
        IEts2Vector Position { get; }
        /// <summary>
        /// Is the wheel powered or not.
        /// </summary>
        bool Powered { get; }
        /// <summary>
        /// Is the wheel liftable or not.
        /// </summary>
        bool Liftable { get; }
    }

    public interface IEts2GearSlot
    {
        /// <summary>
        /// Gear selected when requirements for this h-shifter slot are meet.
        /// Example: 0
        /// </summary>
        int Gear { get; }
        /// <summary>
        /// Position of h-shifter handle.
        /// Zero corresponds to neutral position. 
        /// Mapping to physical position of the handle depends on input setup.
        /// Example: 0
        /// </summary>
        int HandlePosition { get; }
        /// <summary>
        /// Bitmask of required on/off state of selectors.
        /// Only first N number of bits are relevant (where N is the number of IEts2GearSlot objects).
        /// Example: 0
        /// </summary>
        int SlotSelectors { get; }
    }
    */
}