using System;
using System.Collections.Generic;
using System.IO;
using Artemis.DAL;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Modules.Games.ProjectCars.Data;
using Artemis.Profiles.Layers.Models;
using Artemis.Properties;
using Artemis.Services;
using Artemis.Settings;
using Artemis.Utilities;
using Ninject.Extensions.Logging;

namespace Artemis.Modules.Games.ProjectCars
{
    public class ProjectCarsModel : GameModel
    {
        private readonly MetroDialogService _dialogService;

        public ProjectCarsModel(DeviceManager deviceManager, MetroDialogService dialogService)
            : base(deviceManager, SettingsProvider.Load<ProjectCarsSettings>(), new ProjectCarsDataModel())
        {
            _dialogService = dialogService;
            Name = "ProjectCars";
            ProcessName = "pCARS64";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;
        }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
            base.Dispose();
        }

        public override void Enable()
        {
            Initialized = true;
        }

        public override void Update()
        {
            var dataModel = (ProjectCarsDataModel) DataModel;
            var returnTuple = pCarsAPI_GetData.ReadSharedMemoryData();

            // item1 is the true/false indicating a good read or not
            if (returnTuple.Item1)
            {
                // map the data that's read from the struct and map it to the class
                dataModel.GameData = dataModel.GameData.MapStructToClass(returnTuple.Item2, dataModel.GameData);
            }
        }

        public override List<LayerModel> GetRenderLayers(bool keyboardOnly)
        {
            return Profile.GetRenderLayers(DataModel, keyboardOnly);
        }
    }
}