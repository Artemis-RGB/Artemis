using System.ComponentModel;

namespace Artemis.Modules.Games.ProjectCars.Data
{
    public partial class pCarsDataClass : INotifyPropertyChanged
    {
        // Session Type

        private eGameState mgamestate = eGameState.GAME_EXITED; // [ enum (Type#1) Game state ]
        private eRaceState mracestate; // [ RANGE = 0->... ]
        private eSessionState msessionstate = eSessionState.SESSION_INVALID; // [ enum (Type#2) Session state ]

        public eGameState mGameState
        {
            get { return mgamestate; }
            set
            {
                if (mgamestate == value)
                    return;
                SetProperty(ref mgamestate, value);
            }
        }

        public eSessionState mSessionState
        {
            get { return msessionstate; }
            set
            {
                if (msessionstate == value)
                    return;
                SetProperty(ref msessionstate, value);
            }
        }

        public eRaceState mRaceState
        {
            get { return mracestate; }
            set
            {
                if (mracestate == value)
                    return;
                SetProperty(ref mracestate, value);
            }
        }
    }
}