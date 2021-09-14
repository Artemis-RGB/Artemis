using System.Linq;
using Artemis.Core;
using Artemis.UI.Ninject.Factories;
using Artemis.UI.Shared.Services;
using Stylet;

namespace Artemis.UI.Screens.ProfileEditor.DisplayConditions.Event
{
    public class EventsConditionViewModel : Conductor<EventConditionViewModel>.Collection.OneActive
    {
        private readonly IConditionVmFactory _conditionVmFactory;
        private readonly IProfileEditorService _profileEditorService;

        public EventsConditionViewModel(EventsCondition eventsCondition, IConditionVmFactory conditionVmFactory, IProfileEditorService profileEditorService)
        {
            _conditionVmFactory = conditionVmFactory;
            _profileEditorService = profileEditorService;
            EventsCondition = eventsCondition;
        }

        public EventsCondition EventsCondition { get; }

        public TimeLineEventOverlapMode EventOverlapMode
        {
            get => EventsCondition.EventOverlapMode;
            set
            {
                if (EventsCondition.EventOverlapMode == value) return;
                EventsCondition.EventOverlapMode = value;
                _profileEditorService.SaveSelectedProfileElement();
            }
        }

        public void AddEvent()
        {
            EventCondition eventCondition = EventsCondition.AddEventCondition();
            Items.Add(_conditionVmFactory.EventConditionViewModel(eventCondition));

            _profileEditorService.SaveSelectedProfileElement();
        }

        public void DeleteEvent(EventConditionViewModel eventConditionViewModel)
        {
            EventsCondition.RemoveEventCondition(eventConditionViewModel.EventCondition);
            Items.Remove(eventConditionViewModel);

            _profileEditorService.SaveSelectedProfileElement();
        }

        #region Overrides of Screen

        /// <inheritdoc />
        protected override void OnInitialActivate()
        {
            if (!EventsCondition.Events.Any())
                EventsCondition.AddEventCondition();

            foreach (EventCondition eventCondition in EventsCondition.Events)
                Items.Add(_conditionVmFactory.EventConditionViewModel(eventCondition));
            base.OnInitialActivate();
        }

        #endregion
    }
}