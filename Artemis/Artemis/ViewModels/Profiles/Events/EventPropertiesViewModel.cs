using System;
using Artemis.Profiles.Events;
using Artemis.Utilities;
using Caliburn.Micro;

namespace Artemis.ViewModels.Profiles.Events
{
    public class EventPropertiesViewModel : PropertyChangedBase
    {
        private EventPropertiesModel _proposedProperties;

        public EventPropertiesViewModel(EventPropertiesModel eventPropertiesModel)
        {
            if (eventPropertiesModel == null)
                ProposedProperties = new KeyboardEventPropertiesModel
                {
                    ExpirationType = ExpirationType.Time,
                    Length = new TimeSpan(0, 0, 1),
                    TriggerDelay = new TimeSpan(0)
                };
            else
                ProposedProperties = GeneralHelpers.Clone(eventPropertiesModel);
        }

        public EventPropertiesModel ProposedProperties
        {
            get { return _proposedProperties; }
            set
            {
                if (Equals(value, _proposedProperties)) return;
                _proposedProperties = value;
                NotifyOfPropertyChange(() => ProposedProperties);
            }
        }

        public EventPropertiesModel GetAppliedProperties()
        {
            return GeneralHelpers.Clone(ProposedProperties);
        }
    }
}