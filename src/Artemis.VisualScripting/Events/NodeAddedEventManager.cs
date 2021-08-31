using System;
using System.Windows;
using Artemis.Core;
using Artemis.Core.Events;

namespace Artemis.VisualScripting.Events
{
    public class NodeAddedEventManager : WeakEventManager
    {
        #region Properties & Fields

        private static NodeAddedEventManager CurrentManager
        {
            get
            {
                Type type = typeof(NodeAddedEventManager);
                NodeAddedEventManager changedEventManager = (NodeAddedEventManager)GetCurrentManager(type);
                if (changedEventManager == null)
                {
                    changedEventManager = new NodeAddedEventManager();
                    SetCurrentManager(type, changedEventManager);
                }
                return changedEventManager;
            }
        }

        #endregion

        #region Constructors

        private NodeAddedEventManager()
        { }

        #endregion

        #region Methods

        public static void AddListener(INodeScript source, IWeakEventListener listener)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedAddListener(source, listener);
        }

        public static void RemoveListener(INodeScript source, IWeakEventListener listener)
        {
            if (listener == null) throw new ArgumentNullException(nameof(listener));

            CurrentManager.ProtectedRemoveListener(source, listener);
        }

        public static void AddHandler(INodeScript source, EventHandler<SingleValueEventArgs<INode>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedAddHandler(source, handler);
        }

        public static void RemoveHandler(INodeScript source, EventHandler<SingleValueEventArgs<INode>> handler)
        {
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            CurrentManager.ProtectedRemoveHandler(source, handler);
        }

        protected override ListenerList NewListenerList() => new ListenerList<SingleValueEventArgs<INode>>();

        protected override void StartListening(object source) => ((INodeScript)source).NodeAdded += OnNodeAdded;

        protected override void StopListening(object source) => ((INodeScript)source).NodeAdded -= OnNodeAdded;

        private void OnNodeAdded(object sender, SingleValueEventArgs<INode> args) => DeliverEvent(sender, args);

        #endregion
    }
}
