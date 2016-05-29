using System;
using System.Drawing;
using System.Linq.Dynamic;
using Artemis.Events;
using Artemis.Managers;
using Artemis.Models;
using Artemis.Models.Profiles;
using Artemis.Utilities;
using Artemis.Utilities.DataReaders;
using Caliburn.Micro;
using Brush = System.Windows.Media.Brush;
using Color = System.Windows.Media.Color;

namespace Artemis.Modules.Games.Overwatch
{
    public class OverwatchModel : GameModel
    {
        private readonly IEventAggregator _events;

        public OverwatchModel(IEventAggregator events, MainManager mainManager, OverwatchSettings settings)
            : base(mainManager, settings, new OverwatchDataModel())
        {
            _events = events;
            Name = "Overwatch";
            ProcessName = "Overwatch";
            Scale = 4;
            Enabled = Settings.Enabled;
            Initialized = false;

            MmfReader = new MmfReader("overwatchMmf");
        }

        public MmfReader MmfReader { get; set; }

        public int Scale { get; set; }

        public override void Dispose()
        {
            Initialized = false;
        }

        public override void Enable()
        {
            Initialized = true;
        }

        public override void Update()
        {
            var gameDataModel = (OverwatchDataModel) GameDataModel;
            var colors = MmfReader.GetColorArray();
            if (colors == null)
                return;

            var bitmap = new Bitmap(22, 6);

            using (var g = Graphics.FromImage(bitmap))
            {
                for (var y = 0; y < 6; y++)
                {
                    for (var x = 0; x < 22; x++)
                    {
                        g.DrawRectangle(new Pen(ColorHelpers.ToDrawingColor(colors[y, x])), y, x, 1, 1 );
                    }
                }
            }
            _events.PublishOnUIThread(new ChangeBitmap(bitmap));
            if (colors[0, 0].Equals(Color.FromRgb(55, 30, 0)))
                gameDataModel.Status = OverwatchStatus.InMainMenu;
            else if (colors[0, 0].Equals(Color.FromRgb(3, 5, 11)))
                gameDataModel.Status = OverwatchStatus.InGame;
        }

        public override Bitmap GenerateBitmap()
        {
            if (Profile == null || GameDataModel == null)
                return null;

            var keyboardRect = MainManager.DeviceManager.ActiveKeyboard.KeyboardRectangle(Scale);
            return Profile.GenerateBitmap<OverwatchDataModel>(keyboardRect, GameDataModel, false, true);
        }

        public override Brush GenerateMouseBrush()
        {
            return Profile?.GenerateBrush<OverwatchDataModel>(GameDataModel, LayerType.Mouse, false, true);
        }

        public override Brush GenerateHeadsetBrush()
        {
            return Profile?.GenerateBrush<OverwatchDataModel>(GameDataModel, LayerType.Headset, false, true);
        }
    }
}