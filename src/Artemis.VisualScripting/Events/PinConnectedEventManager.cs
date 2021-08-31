using System;
using System.Windows;
using Artemis.Core;
using Artemis.Core.Events;

namespace Artemis.VisualScripting.Events
{
    public class PinConnectedEventManager : WeakEventManager
    {
        #region Properties & Fields

        private static PinConnectedEventManager CurrentManager
        {
            get
            {
                Type type = typeof(PinConnectedEventManager);
                PinConnectedEventManager changedEventManager = (PinConnectedEventManager)GetCurrentManager(type);
                if (changedEventManager == null)
                {
                    changedEventManager = new PinConnectedEventManager();
                    SetCurrentManager(type, changedEventManager);
                }
                return changedEventManager;
            }
        }

        #endregion

        #region Constructors

        private PinConnectedEventManager()
        { }

        #endregion

        #region Methods

        public static void AddListener(IPin source, IWeakEventListener listener)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(IPin source, IWeakEventListener listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        public static void AddHandler(IPin source, EventHandler<SingleValueEventArgs<IPin>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        public static void RemoveHandler(IPin source, EventHandler<SingleValueEventArgs<IPin>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        protected override ListenerList NewListenerList() => new ListenerList<SingleValueEventArgs<IPin>>();

        protected override void StartListening(object source) => ((IPin)source).PinConnected += OnPinConnected;

        protected override void StopListening(object source) => ((IPin)source).PinConnected -= OnPinConnected;

        private void OnPinConnected(object sender, SingleValueEventArgs<IPin> args) => DeliverEvent(sender, args);

        #endregion
    }
}
