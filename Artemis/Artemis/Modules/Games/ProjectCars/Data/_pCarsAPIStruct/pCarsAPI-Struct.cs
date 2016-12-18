using System.Runtime.InteropServices;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public struct pCarsAPIParticipantStruct
    {
        [MarshalAs(UnmanagedType.I1)] public bool mIsActive;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int) eAPIStructLengths.STRING_LENGTH_MAX)] public string mName;
            // [ string ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eVector.VEC_MAX)] public float[] mWorldPosition;
            // [ UNITS = World Space  X  Y  Z ]

        public float mCurrentLapDistance; // [ UNITS = Metres ]   [ RANGE = 0.0f->... ]    [ UNSET = 0.0f ]
        public uint mRacePosition; // [ RANGE = 1->... ]   [ UNSET = 0 ]
        public uint mLapsCompleted; // [ RANGE = 0->... ]   [ UNSET = 0 ]
        public uint mCurrentLap; // [ RANGE = 0->... ]   [ UNSET = 0 ]
        public uint mCurrentSector; // [ enum (Type#4) Current Sector ]
    }

    public struct pCarsAPIStruct
    {
        //SMS supplied data structure
        // Version Number
        public uint mVersion; // [ RANGE = 0->... ]
        public uint mBuildVersion; // [ RANGE = 0->... ]   [ UNSET = 0 ]

        // Session type
        public uint mGameState; // [ enum (Type#1) Game state ]
        public uint mSessionState; // [ enum (Type#2) Session state ]
        public uint mRaceState; // [ enum (Type#3) Race State ]

        // Participant Info
        public int mViewedParticipantIndex; // [ RANGE = 0->STORED_PARTICIPANTS_MAX ]   [ UNSET = -1 ]
        public int mNumParticipants; // [ RANGE = 0->STORED_PARTICIPANTS_MAX ]   [ UNSET = -1 ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eAPIStructLengths.NUM_PARTICIPANTS)] public
            pCarsAPIParticipantStruct[] mParticipantData;

        // Unfiltered Input
        public float mUnfilteredThrottle; // [ RANGE = 0.0f->1.0f ]
        public float mUnfilteredBrake; // [ RANGE = 0.0f->1.0f ]
        public float mUnfilteredSteering; // [ RANGE = -1.0f->1.0f ]
        public float mUnfilteredClutch; // [ RANGE = 0.0f->1.0f ]

        // Vehicle & Track information
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int) eAPIStructLengths.STRING_LENGTH_MAX)] public string
            mCarName; // [ string ]

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int) eAPIStructLengths.STRING_LENGTH_MAX)] public string
            mCarClassName; // [ string ]

        public uint mLapsInEvent; // [ RANGE = 0->... ]   [ UNSET = 0 ]

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int) eAPIStructLengths.STRING_LENGTH_MAX)] public string
            mTrackLocation; // [ string ]

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = (int) eAPIStructLengths.STRING_LENGTH_MAX)] public string
            mTrackVariation; // [ string ]

        public float mTrackLength; // [ UNITS = Metres ]   [ RANGE = 0.0f->... ]    [ UNSET = 0.0f ]

        // Timing & Scoring
        public bool mLapInvalidated; // [ UNITS = boolean ]   [ RANGE = false->true ]   [ UNSET = false ]
        public float mSessionFastestLapTime; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mLastLapTime; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
        public float mCurrentTime; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
        public float mSplitTimeAhead; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mSplitTimeBehind; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mSplitTime; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
        public float mEventTimeRemaining; // [ UNITS = milli-seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mPersonalFastestLapTime; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mWorldFastestLapTime; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mCurrentSector1Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mCurrentSector2Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mCurrentSector3Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mSessionFastestSector1Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mSessionFastestSector2Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mSessionFastestSector3Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mPersonalFastestSector1Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mPersonalFastestSector2Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mPersonalFastestSector3Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mWorldFastestSector1Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mWorldFastestSector2Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public float mWorldFastestSector3Time; // [ UNITS = seconds ]   [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]

        // Flags
        public uint mHighestFlagColour; // [ enum (Type#5) Flag Colour ]
        public uint mHighestFlagReason; // [ enum (Type#6) Flag Reason ]

        // Pit Info
        public uint mPitMode; // [ enum (Type#7) Pit Mode ]
        public uint mPitSchedule; // [ enum (Type#8) Pit Stop Schedule ]

        // Car State
        public uint mCarFlags; // [ enum (Type#6) Car Flags ]
        public float mOilTempCelsius; // [ UNITS = Celsius ]   [ UNSET = 0.0f ]
        public float mOilPressureKPa; // [ UNITS = Kilopascal ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
        public float mWaterTempCelsius; // [ UNITS = Celsius ]   [ UNSET = 0.0f ]
        public float mWaterPressureKPa; // [ UNITS = Kilopascal ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
        public float mFuelPressureKPa; // [ UNITS = Kilopascal ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
        public float mFuelLevel; // [ RANGE = 0.0f->1.0f ]
        public float mFuelCapacity; // [ UNITS = Liters ]   [ RANGE = 0.0f->1.0f ]   [ UNSET = 0.0f ]
        public float mSpeed; // [ UNITS = Metres per-second ]   [ RANGE = 0.0f->... ]
        public float mRPM; // [ UNITS = Revolutions per minute ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
        public float mMaxRPM; // [ UNITS = Revolutions per minute ]   [ RANGE = 0.0f->... ]   [ UNSET = 0.0f ]
        public float mBrake; // [ RANGE = 0.0f->1.0f ]
        public float mThrottle; // [ RANGE = 0.0f->1.0f ]
        public float mClutch; // [ RANGE = 0.0f->1.0f ]
        public float mSteering; // [ RANGE = -1.0f->1.0f ]

        public int mGear;
            // [ RANGE = -1 (Reverse)  0 (Neutral)  1 (Gear 1)  2 (Gear 2)  etc... ]   [ UNSET = 0 (Neutral) ]

        public int mNumGears; // [ RANGE = 0->... ]   [ UNSET = -1 ]
        public float mOdometerKM; // [ RANGE = 0.0f->... ]   [ UNSET = -1.0f ]
        public bool mAntiLockActive; // [ UNITS = boolean ]   [ RANGE = false->true ]   [ UNSET = false ]
        public int mLastOpponentCollisionIndex; // [ RANGE = 0->STORED_PARTICIPANTS_MAX ]   [ UNSET = -1 ]
        public float mLastOpponentCollisionMagnitude; // [ RANGE = 0.0f->... ]
        public bool mBoostActive; // [ UNITS = boolean ]   [ RANGE = false->true ]   [ UNSET = false ]
        public float mBoostAmount; // [ RANGE = 0.0f->100.0f ] 

        // Motion & Device Related
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eVector.VEC_MAX)] public float[] mOrientation;
            // [ UNITS = Euler Angles ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eVector.VEC_MAX)] public float[] mLocalVelocity;
            // [ UNITS = Metres per-second ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eVector.VEC_MAX)] public float[] mWorldVelocity;
            // [ UNITS = Metres per-second ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eVector.VEC_MAX)] public float[] mAngularVelocity;
            // [ UNITS = Radians per-second ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eVector.VEC_MAX)] public float[] mLocalAcceleration;
            // [ UNITS = Metres per-second ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eVector.VEC_MAX)] public float[] mWorldAcceleration;
            // [ UNITS = Metres per-second ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eVector.VEC_MAX)] public float[] mExtentsCentre;
            // [ UNITS = Local Space  X  Y  Z ]

        // Wheels / Tyres
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public uint[] mTyreFlags;
            // [ enum (Type#7) Tyre Flags ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public uint[] mTerrain;
            // [ enum (Type#3) Terrain Materials ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreY;
            // [ UNITS = Local Space  Y ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreRPS;
            // [ UNITS = Revolutions per second ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreSlipSpeed;
            // [ UNITS = Metres per-second ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreTemp;
            // [ UNITS = Celsius ]   [ UNSET = 0.0f ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreGrip;
            // [ RANGE = 0.0f->1.0f ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreHeightAboveGround;
            // [ UNITS = Local Space  Y ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreLateralStiffness;
            // [ UNITS = Lateral stiffness coefficient used in tyre deformation ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreWear;
            // [ RANGE = 0.0f->1.0f ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mBrakeDamage;
            // [ RANGE = 0.0f->1.0f ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mSuspensionDamage;
            // [ RANGE = 0.0f->1.0f ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mBrakeTempCelsius;
            // [ RANGE = 0.0f->1.0f ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreTreadTemp;
            // [ UNITS = Kelvin ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreLayerTemp;
            // [ UNITS = Kelvin ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreCarcassTemp;
            // [ UNITS = Kelvin ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreRimTemp;
            // [ UNITS = Kelvin ]

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = (int) eTyres.TYRE_MAX)] public float[] mTyreInternalAirTemp;
            // [ UNITS = Kelvin ]

        // Car Damage
        public uint mCrashState; // [ enum (Type#4) Crash Damage State ]
        public float mAeroDamage; // [ RANGE = 0.0f->1.0f ]
        public float mEngineDamage; // [ RANGE = 0.0f->1.0f ]

        // Weather
        public float mAmbientTemperature; // [ UNITS = Celsius ]   [ UNSET = 25.0f ]
        public float mTrackTemperature; // [ UNITS = Celsius ]   [ UNSET = 30.0f ]
        public float mRainDensity; // [ UNITS = How much rain will fall ]   [ RANGE = 0.0f->1.0f ]
        public float mWindSpeed; // [ RANGE = 0.0f->100.0f ]   [ UNSET = 2.0f ]
        public float mWindDirectionX; // [ UNITS = Normalised Vector X ]
        public float mWindDirectionY; // [ UNITS = Normalised Vector Y ]
        public float mCloudBrightness; // [ RANGE = 0.0f->... ]
    }
}