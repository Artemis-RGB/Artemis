using System.Runtime.InteropServices;
using Artemis.Modules.Abstract;
using MoonSharp.Interpreter;

namespace Artemis.Modules.Games.FormulaOne2017
{
    [MoonSharpUserData]
    public class FormulaOne2017DataModel : ModuleDataModel
    {
        public FormulaOne2017DataModel()
        {
            Car = new Car();
            Session = new Session();
        }

        public Car Car { get; set; }
        public Session Session { get; set; }

        #region Native structs

        [StructLayout(LayoutKind.Sequential)]
        public struct UdpPacketData
        {
            public float m_time; // Total seconds driven from start line
            public float m_lapTime; // Total seconds of current lap
            public float m_lapDistance; // Total distance through lap in meters
            public float m_totalDistance; // Total distance driven from start line
            public float m_x; // World space position
            public float m_y; // World space position
            public float m_z; // World space position
            public float m_speed; // Meters/sec
            public float m_xv; // Velocity in world space
            public float m_yv; // Velocity in world space
            public float m_zv; // Velocity in world space
            public float m_xr; // World space right direction
            public float m_yr; // World space right direction
            public float m_zr; // World space right direction
            public float m_xd; // World space forward direction
            public float m_yd; // World space forward direction
            public float m_zd; // World space forward direction
            public float m_susp_pos_bl; //
            public float m_susp_pos_br; //
            public float m_susp_pos_fl; //
            public float m_susp_pos_fr; //
            public float m_susp_vel_bl; //
            public float m_susp_vel_br; //
            public float m_susp_vel_fl; //
            public float m_susp_vel_fr; //
            public float m_wheel_speed_bl; //
            public float m_wheel_speed_br; //
            public float m_wheel_speed_fl; //
            public float m_wheel_speed_fr; // 
            public float m_throttle; // Throttle input
            public float m_steer; // Steering input (-1 left to +1 right)
            public float m_brake; // Brake input
            public float m_clutch; // Clutch input
            public float m_gear; // 0 - R | 1 - N | 2-9 - 1-8
            public float m_gforce_lat; // Lateral G's
            public float m_gforce_lon; // Longiitude G's
            public float m_lap; // Current lap number
            public float m_engineRate; // Engine RPM
            public float m_sli_pro_native_support; // SLI Pro support
            public float m_car_position; // car race position
            public float m_kers_level; // kers energy left
            public float m_kers_max_level; // kers maximum energy
            public float m_drs; // 0 = off, 1 = on
            public float m_traction_control; // 0 (off) - 2 (high)
            public float m_anti_lock_brakes; // 0 (off) - 1 (on)
            public float m_fuel_in_tank; // current fuel mass
            public float m_fuel_capacity; // fuel capacity
            public float m_in_pits; // 0 = none, 1 = pitting, 2 = in pit area
            public float m_sector; // 0 = sector1, 1 = sector2; 2 = sector3
            public float m_sector1_time; // time of sector1 (or 0)
            public float m_sector2_time; // time of sector2 (or 0)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] m_brakes_temp; // brakes temperature (centigrade)
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public float[] m_wheels_pressure; // wheels pressure PSI
            public float m_team_info; // team ID 
            public float m_total_laps; // total number of laps in this race
            public float m_track_size; // track size meters
            public float m_last_lap_time; // last lap time
            public float m_max_rpm; // cars max RPM, at which point the rev limiter will kick in
            public float m_idle_rpm; // cars idle RPM
            public float m_max_gears; // maximum number of gears
            public float m_sessionType; // 0 = unknown, 1 = practice, 2 = qualifying, 3 = race
            public float m_drsAllowed; // 0 = not allowed, 1 = allowed, -1 = invalid / unknown
            public float m_track_number; // -1 for unknown, 0-21 for tracks
            public float m_vehicleFIAFlags; // -1 = invalid/unknown, 0 = none, 1 = green, 2 = blue, 3 = yellow, 4 = red
        }

        #endregion
    }

    public class Car
    {
        public Car()
        {
            Overview = new CarOverview();
            Details = new CarDetails();
        }

        public CarOverview Overview { get; set; }
        public CarDetails Details { get; set; }

        public float SpeedMps { get; set; }

        public float Steering { get; set; }
        public float Throttle { get; set; }
        public float Brake { get; set; }
        public float Clutch { get; set; }
        
        public bool Drs { get; set; }
    }

    public class CarDetails
    {
        public float Rpm { get; set; }
        public float MaxRpm { get; set; }
        public float IdleRpm { get; set; }

        public int Gear { get; set; }
        public int MaxGear { get; set; }

        public float Kers { get; set; }
        public float MaxKers { get; set; }

        public float Fuel { get; set; }
        public float MaxFuel { get; set; }

        public float LateralG { get; set; }
        public float LongitudeG { get; set; }

        public float WheelSpeedFrontLeft { get; set; }
        public float WheelSpeedFrontRight { get; set; }
        public float WheelSpeedRearLeft { get; set; }
        public float WheelSpeedRearRight { get; set; }
    }

    public class CarOverview
    {
        public TractionControl TractionControl { get; set; }
        public bool AntiLockBrakes { get; set; }
    }

    public enum TractionControl
    {
        Off = 0,
        Medium = 1,
        High = 2
    }

    public class Session
    {
        public SessionType SessionType { get; set; }
        public bool DrsEnabled { get; set; }
        public SessionFlag Flags { get; set; }

        public float TotalSeconds { get; set; }
        public float LapSeconds { get; set; }

        public float TrackLength { get; set; }
        public float TotalDistance { get; set; }
        public float LapDistance { get; set; }

        public int LabNumber { get; set; }
        public int Position { get; set; }
    }

    public enum SessionFlag
    {
        Unknown = -1,
        None = 0,
        Green = 1,
        Blue = 2,
        Yellow = 3,
        Red = 4
    }

    public enum SessionType
    {
        Unknown,
        Practise,
        Qualifying,
        Race
    }
}
