﻿using System;
using System.Drawing;
using System.Linq;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Settings;
using Artemis.Utilities;
using Artemis.Utilities.Memory;
using Newtonsoft.Json;
using Brush = System.Windows.Media.Brush;

namespace Artemis.Modules.Games.RocketLeague
{
    public class RocketLeagueModel : GameModel
    {
        private Memory _memory;
        private GamePointersCollection _pointer;

        public RocketLeagueModel(MainManager mainManager, RocketLeagueSettings settings)
            : base(mainManager, settings, new RocketLeagueDataModel())
        {
            Name = "RocketLeague";
            ProcessName = "RocketLeague";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            _memory = null;
        }

        public override void Enable()
        {
            Initialized = false;

            Updater.GetPointers();
            _pointer = JsonConvert.DeserializeObject<GamePointersCollection>(Offsets.Default.RocketLeague);

            var tempProcess = MemoryHelpers.GetProcessIfRunning(ProcessName);
            if (tempProcess == null)
                return;
            
            _memory = new Memory(tempProcess);

            Initialized = true;
        }

        public override void Update()
        {
            if (Profile == null || GameDataModel == null || _memory == null)
                return;

            var offsets = _pointer.GameAddresses.First(ga => ga.Description == "Boost").ToString();
            var boostAddress = _memory.GetAddress("\"RocketLeague.exe\"" + offsets);
            var boostFloat = _memory.ReadFloat(boostAddress)*100/3;

            ((RocketLeagueDataModel) GameDataModel).Boost = (int) Math.Ceiling(boostFloat);

            // Take care of any reading errors resulting in an OutOfMemory on draw
            if (((RocketLeagueDataModel) GameDataModel).Boost < 0)
                ((RocketLeagueDataModel) GameDataModel).Boost = 0;
            if (((RocketLeagueDataModel) GameDataModel).Boost > 100)
                ((RocketLeagueDataModel) GameDataModel).Boost = 100;
        }

        public override Bitmap GenerateBitmap()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            var keyboardRect = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(Scale);
            return Profile.GenerateBitmap<RocketLeagueDataModel>(keyboardRect, GameDataModel, false, true);
        }

        public override Brush GenerateMouseBrush()
        {
            return Profile.GenerateBrush<RocketLeagueDataModel>(GameDataModel, LayerType.Mouse, false, true);
        }

        public override Brush GenerateHeadsetBrush()
        {
            return Profile.GenerateBrush<RocketLeagueDataModel>(GameDataModel, LayerType.Headset, false, true);
        }
    }
}