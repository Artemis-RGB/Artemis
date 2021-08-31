using System;
using System.Windows;
using Artemis.Core;
using Artemis.Core.Events;

namespace Artemis.VisualScripting.Events
{
    public class PinRemovedEventManager : WeakEventManager
    {
        #region Properties & Fields

        private static PinRemovedEventManager CurrentManager
        {
            get
            {
                Type type = typeof(PinRemovedEventManager);
                PinRemovedEventManager changedEventManager = (PinRemovedEventManager)GetCurrentManager(type);
                if (changedEventManager == null)
                {
                    changedEventManager = new PinRemovedEventManager();
                    SetCurrentManager(type, changedEventManager);
                }
                return changedEventManager;
            }
        }

        #endregion

        #region Constructors

        private PinRemovedEventManager()
        { }

        #endregion

        #region Methods

        public static void AddListener(IPinCollection source, IWeakEventListener listener)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(IPinCollection source, IWeakEventListener listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        public static void AddHandler(IPinCollection source, EventHandler<SingleValueEventArgs<IPin>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        public static void RemoveHandler(IPinCollection source, EventHandler<SingleValueEventArgs<IPin>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        protected override ListenerList NewListenerList() => new ListenerList<SingleValueEventArgs<IPin>>();

        protected override void StartListening(object source) => ((IPinCollection)source).PinRemoved += OnPinRemoved;

        protected override void StopListening(object source) => ((IPinCollection)source).PinRemoved -= OnPinRemoved;

        private void OnPinRemoved(object sender, SingleValueEventArgs<IPin> args) => DeliverEvent(sender, args);

        #endregion
    }
}
