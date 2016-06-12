using Artemis.Events;
using Artemis.Managers;
using Artemis.ViewModels.Abstract;
using Caliburn.Micro;

namespace Artemis.Modules.Effects.Bubbles
{
    public class BubblesViewModel : EffectViewModel, IHandle<ActiveEffectChanged>
    {
        public BubblesViewModel(MainManager main, IEventAggregator events)
            : base(main, new BubblesModel(main, new BubblesSettings()))
        {
            DisplayName = "Bubbles";
            events.Subscribe(this);

            MainManager.EffectManager.EffectModels.Add(EffectModel);
            EffectSettings = ((BubblesModel)EffectModel).Settings;
        }

        public void Handle(ActiveEffectChanged message)
        {
            NotifyOfPropertyChange(() => EffectEnabled);
        }
    }
}
