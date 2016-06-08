using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.TypeWave
{
    public sealed class TypeWaveViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public TypeWaveViewModel(MainManager main, IEventAggregator events)
            : base(main, new TypeWaveModel(main, new TypeWaveSettings()))
        {
            DisplayName = "Type Waves";
            events.Subscribe(this);

            MainManager.EffectManager.EffectModels.Add(EffectModel);
            EffectSettings = ((TypeWaveModel) EffectModel).Settings;
        }

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}