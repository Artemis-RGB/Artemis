using Artemis.Events;
using Artemis.Models;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.TypeWave
{
    public class TypeWaveViewModel : Screen, IHandle<ChangeActiveEffect>
    {
        private TypeWaveSettings _typeWaveSettings;

        public TypeWaveViewModel(MainModel mainModel)
        {
            // Subscribe to main model
            MainModel = mainModel;
            MainModel.Events.Subscribe(this);

            // Settings are loaded from file by class
            TypeWaveSettings = new TypeWaveSettings();

            // Create effect model and add it to MainModel
            TypeWaveModel = new TypeWaveModel(mainModel, TypeWaveSettings);
            MainModel.EffectModels.Add(TypeWaveModel);
        }

        public MainModel MainModel { get; set; }
        public TypeWaveModel TypeWaveModel { get; set; }

        public static string Name => "Type Waves";
        public bool EffectEnabled => MainModel.IsEnabled(TypeWaveModel);

        public TypeWaveSettings TypeWaveSettings
        {
            get { return _typeWaveSettings; }
            set
            {
                if (Equals(value, _typeWaveSettings)) return;
                _typeWaveSettings = value;
                NotifyOfPropertyChange(() => TypeWaveSettings);
            }
        }

        public void Handle(ChangeActiveEffect message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }

        public void ToggleEffect()
        {
            if (EffectEnabled && !MainModel.Suspended)
                MainModel.ToggleSuspension();
            else if (!EffectEnabled && !MainModel.Suspended)
                MainModel.EnableEffect(TypeWaveModel);
            else
            {
                MainModel.ToggleSuspension();
                MainModel.EnableEffect(TypeWaveModel);
            }
        }

        public void SaveSettings()
        {
            if (TypeWaveModel == null)
                return;

            TypeWaveSettings.Save();
        }

        public void ResetSettings()
        {
            // TODO: Confirmation dialog (Generic MVVM approach)
            TypeWaveSettings.ToDefault();
            NotifyOfPropertyChange(() => TypeWaveSettings);

            SaveSettings();
        }
    }
}