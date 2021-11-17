using System;
using System.Windows;
using Artemis.Core;
using Artemis.Core.Events;

namespace Artemis.VisualScripting.Events
{
    public class NodeRemovedEventManager : WeakEventManager
    {
        #region Properties & Fields

        private static NodeRemovedEventManager CurrentManager
        {
            get
            {
                Type type = typeof(NodeRemovedEventManager);
                NodeRemovedEventManager changedEventManager = (NodeRemovedEventManager)GetCurrentManager(type);
                if (changedEventManager == null)
                {
                    changedEventManager = new NodeRemovedEventManager();
                    SetCurrentManager(type, changedEventManager);
                }
                return changedEventManager;
            }
        }

        #endregion

        #region Constructors

        private NodeRemovedEventManager()
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

        protected override void StartListening(object source) => ((INodeScript)source).NodeRemoved += OnNodeRemoved;

        protected override void StopListening(object source) => ((INodeScript)source).NodeRemoved -= OnNodeRemoved;

        private void OnNodeRemoved(object sender, SingleValueEventArgs<INode> args) => DeliverEvent(sender, args);

        #endregion
    }
}
