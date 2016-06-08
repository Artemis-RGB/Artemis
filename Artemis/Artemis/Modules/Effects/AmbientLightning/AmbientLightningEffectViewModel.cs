using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.AmbientLightning
{
    public sealed class AmbientLightningEffectViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public AmbientLightningEffectViewModel(MainManager main, IEventAggregator events)
            : base(main, new AmbientLightningEffectModel(main, new AmbientLightningEffectSettings()))
        {
            DisplayName = "Ambient Lightning";

            events.Subscribe(this);
            MainManager.EffectManager.EffectModels.Add(EffectModel);
        }

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}