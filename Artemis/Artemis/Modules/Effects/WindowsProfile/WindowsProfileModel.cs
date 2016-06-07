using System;
using System.Collections.Generic;
using System.Diagnostics;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;

namespace Artemis.Modules.Effects.WindowsProfile
{
    public class WindowsProfileModel : EffectModel
    {
        private List<PerformanceCounter> _cores;
        private int _cpuFrames;

        public WindowsProfileModel(MainManager mainManager, WindowsProfileSettings settings)
            : base(mainManager, new WindowsProfileDataModel())
        {
            Name = "WindowsProfile";
            Settings = settings;
        }

        public WindowsProfileSettings Settings { get; set; }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
            // Setup CPU cores
            _cores = GetPerformanceCounters();
            var coreCount = _cores.Count;
            while (coreCount < 8)
            {
                _cores.Add(null);
                coreCount++;
            }

            Initialized = true;
        }

        public override void Update()
        {
            UpdateCpu();
        }

        private void UpdateCpu()
        {
            // CPU is only updated every 15 frames, the performance counter gives 0 if updated too often
            _cpuFrames++;
            if (_cpuFrames < 16)
                return;

            _cpuFrames = 0;

            var dataModel = (WindowsProfileDataModel)DataModel;

            // Update cores, not ideal but data models don't support lists. 
            if (_cores[0] != null)
                dataModel.Cpu.Core1Usage = (int)_cores[0].NextValue();
            if (_cores[1] != null)
                dataModel.Cpu.Core2Usage = (int)_cores[1].NextValue();
            if (_cores[2] != null)
                dataModel.Cpu.Core3Usage = (int)_cores[2].NextValue();
            if (_cores[3] != null)
                dataModel.Cpu.Core4Usage = (int)_cores[3].NextValue();
            if (_cores[4] != null)
                dataModel.Cpu.Core5Usage = (int)_cores[4].NextValue();
            if (_cores[5] != null)
                dataModel.Cpu.Core6Usage = (int)_cores[5].NextValue();
            if (_cores[6] != null)
                dataModel.Cpu.Core7Usage = (int)_cores[6].NextValue();
            if (_cores[7] != null)
                dataModel.Cpu.Core8Usage = (int)_cores[7].NextValue();
        }

        public override List<LayerModel> GetRenderLayers(bool renderMice, bool renderHeadsets)
        {
            return Profile.GetRenderLayers<WindowsProfileDataModel>(DataModel, renderMice, renderHeadsets, true);
        }

        public static List<PerformanceCounter> GetPerformanceCounters()
        {
            var performanceCounters = new List<PerformanceCounter>();
            var procCount = Environment.ProcessorCount;
            for (var i = 0; i < procCount; i++)
            {
                var pc = new PerformanceCounter("Processor", "% Processor Time", i.ToString());
                performanceCounters.Add(pc);
            }
            return performanceCounters;
        }
    }
}