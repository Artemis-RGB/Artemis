using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        public pCarsDataClass MapStructToClass(pCarsAPIStruct pcarsDataStruct, pCarsDataClass pCarsData)
        {
            //pCarsDataClass pCarsData = new pCarsDataClass();
            pCarsData.mVersion = pcarsDataStruct.mVersion;
            pCarsData.mBuildVersion = pcarsDataStruct.mBuildVersion;

            // Session type
            pCarsData.mGameState = (eGameState) pcarsDataStruct.mGameState;
            pCarsData.mSessionState = (eSessionState) pcarsDataStruct.mSessionState;
            pCarsData.mRaceState = (eRaceState) pcarsDataStruct.mRaceState;

            pCarsData.mViewedParticipantIndex = pcarsDataStruct.mViewedParticipantIndex;
            pCarsData.mNumParticipants = pcarsDataStruct.mNumParticipants;

            if (pCarsData.mPlayerParticipantIndex < 0)
                pCarsData.mPlayerParticipantIndex = pCarsData.mViewedParticipantIndex;

            for (var loop = 0; loop < (uint) eAPIStructLengths.NUM_PARTICIPANTS; loop++)
            {
                if (pCarsData.listParticipantInfo.Count != (uint) eAPIStructLengths.NUM_PARTICIPANTS)
                    for (var i = 0; i < (uint) eAPIStructLengths.NUM_PARTICIPANTS; i++)
                        pCarsData.listParticipantInfo.Add(new pCarsParticipantsClass());

                if (pcarsDataStruct.mParticipantData[loop].mCurrentLap != 0)
                {
                    var newPartData = new pCarsParticipantsClass
                    {
                        parIsActive = pcarsDataStruct.mParticipantData[loop].mIsActive,
                        parName = pcarsDataStruct.mParticipantData[loop].mName,
                        parWorldPosition = new List<float>(pcarsDataStruct.mParticipantData[loop].mWorldPosition),
                        parCurrentLapDistance = pcarsDataStruct.mParticipantData[loop].mCurrentLapDistance,
                        parRacePosition = pcarsDataStruct.mParticipantData[loop].mRacePosition,
                        parLapsCompleted = pcarsDataStruct.mParticipantData[loop].mLapsCompleted,
                        parCurrentLap = pcarsDataStruct.mParticipantData[loop].mCurrentLap,
                        parCurrentSector = (eCurrentSector) pcarsDataStruct.mParticipantData[loop].mCurrentSector
                    };

                    pCarsData.listParticipantInfo[loop] = newPartData;
                }
            }

            // Unfiltered Input
            pCarsData.mUnfilteredThrottle = pcarsDataStruct.mUnfilteredThrottle;
            pCarsData.mUnfilteredBrake = pcarsDataStruct.mUnfilteredBrake;
            pCarsData.mUnfilteredSteering = pcarsDataStruct.mUnfilteredSteering;
            pCarsData.mUnfilteredClutch = pcarsDataStruct.mUnfilteredClutch;

            // Vehicle & Track information
            pCarsData.mCarName = pcarsDataStruct.mCarName;
            pCarsData.mCarClassName = pcarsDataStruct.mCarClassName;
            pCarsData.mLapsInEvent = pcarsDataStruct.mLapsInEvent;
            pCarsData.mTrackLocation = pcarsDataStruct.mTrackLocation;
            pCarsData.mTrackVariant = pcarsDataStruct.mTrackVariation;
            pCarsData.mTrackLength = pcarsDataStruct.mTrackLength;

            // Timing & Scoring
            pCarsData.mLapInvalidated = pcarsDataStruct.mLapInvalidated;
            pCarsData.mLastLapTime = pcarsDataStruct.mLastLapTime;
            pCarsData.mCurrentTime = pcarsDataStruct.mCurrentTime;

            pCarsData.mSplitTimeAhead = pcarsDataStruct.mSplitTimeAhead;
            pCarsData.mSplitTimeBehind = pcarsDataStruct.mSplitTimeBehind;
            pCarsData.mSplitTime = pcarsDataStruct.mSplitTime;
            pCarsData.mEventTimeRemaining = pcarsDataStruct.mEventTimeRemaining;


            //make sure that the collections are not empty
            if (pCarsData.mCurrentLapTime.Count == 0)
                pCarsData.mCurrentLapTime = new ObservableCollection<LapTimesClass> {new LapTimesClass()};

            if (pCarsData.mSessionFastestLapTime.Count == 0)
                pCarsData.mSessionFastestLapTime = new ObservableCollection<LapTimesClass> {new LapTimesClass()};

            if (pCarsData.mPersonalFastestLapTime.Count == 0)
                pCarsData.mPersonalFastestLapTime = new ObservableCollection<LapTimesClass> {new LapTimesClass()};

            if (pCarsData.mWorldFastestLapTime.Count == 0)
                pCarsData.mWorldFastestLapTime = new ObservableCollection<LapTimesClass> {new LapTimesClass()};

            //create the new entry at index 0
            //index 0 is the first in the collection
            //a collection is required for the datagrid binding

            pCarsData.mCurrentLapTime[0] = new LapTimesClass
            {
                ltLapTime = pcarsDataStruct.mCurrentTime,
                ltSect1 = pcarsDataStruct.mCurrentSector1Time,
                ltSect2 = pcarsDataStruct.mCurrentSector2Time,
                ltSect3 = pcarsDataStruct.mCurrentSector3Time
            };

            pCarsData.mSessionFastestLapTime[0] = new LapTimesClass
            {
                ltLapTime = pcarsDataStruct.mSessionFastestLapTime,
                ltSect1 = pcarsDataStruct.mSessionFastestSector1Time,
                ltSect2 = pcarsDataStruct.mSessionFastestSector2Time,
                ltSect3 = pcarsDataStruct.mSessionFastestSector3Time
            };

            pCarsData.mPersonalFastestLapTime[0] = new LapTimesClass
            {
                ltLapTime = pcarsDataStruct.mPersonalFastestLapTime,
                ltSect1 = pcarsDataStruct.mPersonalFastestSector1Time,
                ltSect2 = pcarsDataStruct.mPersonalFastestSector2Time,
                ltSect3 = pcarsDataStruct.mPersonalFastestSector3Time
            };

            pCarsData.mWorldFastestLapTime[0] = new LapTimesClass
            {
                ltLapTime = pcarsDataStruct.mWorldFastestLapTime,
                ltSect1 = pcarsDataStruct.mWorldFastestSector1Time,
                ltSect2 = pcarsDataStruct.mWorldFastestSector2Time,
                ltSect3 = pcarsDataStruct.mWorldFastestSector3Time
            };

            // Flags
            pCarsData.mFlagColour = (eFlagColors) pcarsDataStruct.mHighestFlagColour;
            pCarsData.mFlagReason = (eFlagReason) pcarsDataStruct.mHighestFlagReason;

            // Pit Info
            pCarsData.mPitMode = (ePitMode) pcarsDataStruct.mPitMode;
            pCarsData.mPitSchedule = (ePitSchedule) pcarsDataStruct.mPitSchedule;

            // Car State
            pCarsData.mCarFlags = (eCarFlags) pcarsDataStruct.mCarFlags;
            pCarsData.mOilTempCelsius = pcarsDataStruct.mOilTempCelsius;
            pCarsData.mOilPressureKPa = pcarsDataStruct.mOilPressureKPa;
            pCarsData.mWaterTempCelsius = pcarsDataStruct.mWaterTempCelsius;
            pCarsData.mWaterPressureKPa = pcarsDataStruct.mWaterPressureKPa;
            pCarsData.mFuelPressureKPa = pcarsDataStruct.mFuelPressureKPa;

            pCarsData.mFuelLevel = (float) Math.Round(pcarsDataStruct.mFuelLevel*pcarsDataStruct.mFuelCapacity, 2);
            pCarsData.mFuelCapacity = pcarsDataStruct.mFuelCapacity;
            pCarsData.mSpeed = pcarsDataStruct.mSpeed;
            pCarsData.mRPM = pcarsDataStruct.mRPM;
            pCarsData.mMaxRPM = pcarsDataStruct.mMaxRPM;

            //logger.Trace("mRPM = " + pCarsData.mRPM);
            //logger.Trace("mMaxRPM = " + pCarsData.mMaxRPM);
            //
            pCarsData.mBrake = pcarsDataStruct.mBrake;
            pCarsData.mThrottle = pcarsDataStruct.mThrottle;
            pCarsData.mClutch = pcarsDataStruct.mClutch;
            pCarsData.mSteering = pcarsDataStruct.mSteering;
            pCarsData.mGear = pcarsDataStruct.mGear;
            pCarsData.mNumGears = pcarsDataStruct.mNumGears;
            pCarsData.mOdometerKM = pcarsDataStruct.mOdometerKM;
            pCarsData.mAntiLockActive = pcarsDataStruct.mAntiLockActive;

            pCarsData.mLastOpponentCollisionIndex = pcarsDataStruct.mLastOpponentCollisionIndex;
            pCarsData.mLastOpponentCollisionMagnitude = pcarsDataStruct.mLastOpponentCollisionMagnitude;

            pCarsData.mBoostActive = pcarsDataStruct.mBoostActive;
            pCarsData.mBoostAmount = pcarsDataStruct.mBoostAmount;

            // Motion & Device Related
            //////pCarsData.mWorldPosition = new List<float>(pcarsDataStruct.mWorldPosition);
            pCarsData.mOrientation = new List<float>(pcarsDataStruct.mOrientation);
            pCarsData.mLocalVelocity = new List<float>(pcarsDataStruct.mLocalVelocity);
            pCarsData.mWorldVelocity = new List<float>(pcarsDataStruct.mWorldVelocity);
            pCarsData.mAngularVelocity = new List<float>(pcarsDataStruct.mAngularVelocity);
            pCarsData.mLocalAcceleration = new List<float>(pcarsDataStruct.mLocalAcceleration);
            pCarsData.mWorldAcceleration = new List<float>(pcarsDataStruct.mWorldAcceleration);
            pCarsData.mExtentsCentre = new List<float>(pcarsDataStruct.mExtentsCentre);

            // Wheels / Tyres
            //pCarsData.mTyreFlags = pcarsDataStruct.mTyreFlags.Select(i => (eTyreFlags)i).ToList();
            //pCarsData.mTerrain = pcarsDataStruct.mTerrain.Select(i => (eTerrain)i).ToList();

            pCarsData.mTyreFlags = new List<uint>(pcarsDataStruct.mTyreFlags);
            pCarsData.mTerrain = new List<uint>(pcarsDataStruct.mTerrain);
            pCarsData.mTyreY = new List<float>(pcarsDataStruct.mTyreY);
            pCarsData.mTyreRPS = new List<float>(pcarsDataStruct.mTyreRPS);
            pCarsData.mTyreSlipSpeed = new List<float>(pcarsDataStruct.mTyreSlipSpeed);
            pCarsData.mTyreTemp = new List<float>(pcarsDataStruct.mTyreTemp);
            pCarsData.mTyreGrip = new List<float>(pcarsDataStruct.mTyreGrip);
            pCarsData.mTyreHeightAboveGround = new List<float>(pcarsDataStruct.mTyreHeightAboveGround);
            pCarsData.mTyreLateralStiffness = new List<float>(pcarsDataStruct.mTyreLateralStiffness);
            pCarsData.mTyreWear = new List<float>(pcarsDataStruct.mTyreWear);
            pCarsData.mBrakeDamage = new List<float>(pcarsDataStruct.mBrakeDamage);
            pCarsData.mSuspensionDamage = new List<float>(pcarsDataStruct.mSuspensionDamage);

            pCarsData.mBrakeTempCelsius = new List<float>(pcarsDataStruct.mBrakeTempCelsius);
            pCarsData.mTyreTreadTemp = new List<float>(pcarsDataStruct.mTyreTreadTemp);
            pCarsData.mTyreLayerTemp = new List<float>(pcarsDataStruct.mTyreLayerTemp);
            pCarsData.mTyreCarcassTemp = new List<float>(pcarsDataStruct.mTyreCarcassTemp);
            pCarsData.mTyreRimTemp = new List<float>(pcarsDataStruct.mTyreRimTemp);
            pCarsData.mTyreInternalAirTemp = new List<float>(pcarsDataStruct.mTyreInternalAirTemp);

            // Car Damage
            pCarsData.mCrashState = (eCrashDamageState) pcarsDataStruct.mCrashState;
            pCarsData.mAeroDamage = pcarsDataStruct.mAeroDamage;
            pCarsData.mEngineDamage = pcarsDataStruct.mEngineDamage;

            // Weather
            pCarsData.mAmbientTemperature = pcarsDataStruct.mAmbientTemperature;
            pCarsData.mTrackTemperature = pcarsDataStruct.mTrackTemperature;
            pCarsData.mRainDensity = pcarsDataStruct.mRainDensity;
            pCarsData.mWindSpeed = pcarsDataStruct.mWindSpeed;
            pCarsData.mWindDirectionX = pcarsDataStruct.mWindDirectionX;
            pCarsData.mWindDirectionY = pcarsDataStruct.mWindDirectionY;
            pCarsData.mCloudBrightness = pcarsDataStruct.mCloudBrightness;

            return pCarsData;
        }
    }
}