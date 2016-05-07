using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.AmbientLightning
{
    public sealed class AmbientLightningEffectViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public AmbientLightningEffectViewModel(MainManager mainManager, KeyboardManager keyboardManager,
            EffectManager effectManager, IEventAggregator events)
            : base(
                mainManager, effectManager,
                new AmbientLightningEffectModel(mainManager, keyboardManager, new AmbientLightningEffectSettings()))
        {
            DisplayName = "Ambient Lightning";

            events.Subscribe(this);
            EffectManager.EffectModels.Add(EffectModel);
        }

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}