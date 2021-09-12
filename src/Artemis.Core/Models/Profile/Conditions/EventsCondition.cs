using System;
using System.Collections.Generic;
using System.Linq;
using Artemis.Storage.Entities.Profile.Abstract;
using Artemis.Storage.Entities.Profile.Conditions;

namespace Artemis.Core
{
    public class EventsCondition : CorePropertyChanged, INodeScriptCondition
    {
        private readonly EventsConditionEntity _entity;
        private readonly List<EventCondition> _eventsList;
        private TimeLineEventOverlapMode _eventOverlapMode;

        public EventsCondition(ProfileElement profileElement)
        {
            _entity = new EventsConditionEntity();
            _eventsList = new List<EventCondition>();

            ProfileElement = profileElement;
            Events = new List<EventCondition>(_eventsList);
        }

        internal EventsCondition(EventsConditionEntity entity, ProfileElement profileElement)
        {
            _entity = entity;
            _eventsList = new List<EventCondition>();

            ProfileElement = profileElement;
            Events = new List<EventCondition>(_eventsList);

            Load();
        }

        /// <summary>
        ///     Gets a list of events this condition reacts to
        /// </summary>
        public IReadOnlyCollection<EventCondition> Events { get; }

        /// <summary>
        ///     Gets or sets how the condition behaves when events trigger before the timeline finishes
        /// </summary>
        public TimeLineEventOverlapMode EventOverlapMode
        {
            get => _eventOverlapMode;
            set => SetAndNotify(ref _eventOverlapMode, value);
        }

        /// <inheritdoc />
        public IConditionEntity Entity => _entity;

        /// <inheritdoc />
        public ProfileElement ProfileElement { get; }

        /// <inheritdoc />
        public bool IsMet { get; private set; }

        /// <summary>
        ///     Adds a new event condition
        /// </summary>
        /// <returns>The newly created event condition</returns>
        public EventCondition AddEventCondition()
        {
            EventCondition eventCondition = new(ProfileElement.GetType().Name.ToLower(), ProfileElement.Profile);
            lock (_eventsList)
            {
                _eventsList.Add(eventCondition);
            }

            return eventCondition;
        }

        /// <summary>
        ///     Removes the provided event condition
        /// </summary>
        /// <param name="eventCondition">The event condition to remove</param>
        public void RemoveEventCondition(EventCondition eventCondition)
        {
            lock (_eventsList)
            {
                _eventsList.Remove(eventCondition);
            }
        }

        /// <inheritdoc />
        public void Update()
        {
            lock (_eventsList)
            {
                if (EventOverlapMode == TimeLineEventOverlapMode.Toggle)
                {
                    if (_eventsList.Any(c => c.Evaluate()))
                        IsMet = !IsMet;
                }
                else
                {
                    IsMet = _eventsList.Any(c => c.Evaluate());
                }
            }
        }

        /// <inheritdoc />
        public void ApplyToTimeline(bool isMet, bool wasMet, Timeline timeline)
        {
            if (!isMet)
                return;

            // Event overlap mode doesn't apply in this case
            if (timeline.IsFinished)
            {
                timeline.JumpToStart();
                return;
            }

            // If the timeline was already running, look at the event overlap mode
            if (EventOverlapMode == TimeLineEventOverlapMode.Restart)
                timeline.JumpToStart();
            else if (EventOverlapMode == TimeLineEventOverlapMode.Copy)
                timeline.AddExtraTimeline();
            else if (EventOverlapMode == TimeLineEventOverlapMode.Toggle && !wasMet)
                timeline.JumpToStart();

            // The remaining overlap mode is 'ignore' which requires no further action
        }

        /// <inheritdoc />
        public void Dispose()
        {
            foreach (EventCondition eventCondition in Events)
                eventCondition.Dispose();
        }

        #region Storage

        /// <inheritdoc />
        public void Load()
        {
            EventOverlapMode = (TimeLineEventOverlapMode) _entity.EventOverlapMode;
            lock (_eventsList)
            {
                _eventsList.Clear();
                foreach (EventConditionEntity eventCondition in _entity.Events)
                    _eventsList.Add(new EventCondition(eventCondition, ProfileElement.GetType().Name.ToLower(), ProfileElement.Profile));
            }
        }

        /// <inheritdoc />
        public void Save()
        {
            _entity.EventOverlapMode = (int) EventOverlapMode;
            _entity.Events.Clear();
            lock (_eventsList)
            {
                foreach (EventCondition eventCondition in _eventsList)
                {
                    eventCondition.Save();
                    _entity.Events.Add(eventCondition.Entity);
                }
            }
        }

        /// <inheritdoc />
        public void LoadNodeScript()
        {
            lock (_eventsList)
            {
                foreach (EventCondition eventCondition in _eventsList)
                    eventCondition.LoadNodeScript();
            }
        }

        #endregion
    }
}