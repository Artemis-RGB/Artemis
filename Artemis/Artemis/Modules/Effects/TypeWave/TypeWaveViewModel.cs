using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.TypeWave
{
    public sealed class TypeWaveViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public TypeWaveViewModel(MainManager mainManager, KeyboardManager keyboardManager, EffectManager effectManager,
            IEventAggregator events)
            : base(mainManager, effectManager, new TypeWaveModel(mainManager, keyboardManager, new TypeWaveSettings()))
        {
            DisplayName = "Type Waves";

            events.Subscribe(this);
            EffectManager.EffectModels.Add(EffectModel);
        }

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}