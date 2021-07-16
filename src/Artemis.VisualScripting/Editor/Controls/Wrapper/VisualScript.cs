using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using Artemis.Core.VisualScripting;
using Artemis.VisualScripting.Events;
using Artemis.VisualScripting.ViewModel;

namespace Artemis.VisualScripting.Editor.Controls.Wrapper
{
    public class VisualScript : AbstractBindable
    {
        #region Properties & Fields

        private readonly HashSet<VisualScriptNode> _selectedNodes = new();
        private readonly Dictionary<VisualScriptNode, (double X, double Y)> _nodeStartPositions = new();
        private double _nodeDragAccumulationX;
        private double _nodeDragAccumulationY;

        public IScript Script { get; }

        public int SurfaceSize { get; }
        public int GridSize { get; }

        private double _nodeDragScale = 1;
        public double NodeDragScale
        {
            get => _nodeDragScale;
            set => SetProperty(ref _nodeDragScale, value);
        }

        private VisualScriptPin _isConnectingPin;
        private VisualScriptPin IsConnectingPin
        {
            get => _isConnectingPin;
            set
            {
                if (SetProperty(ref _isConnectingPin, value))
                    OnPropertyChanged(nameof(IsConnecting));
            }
        }

        public ObservableCollection<VisualScriptNode> Nodes { get; } = new();

        public IEnumerable<VisualScriptCable> Cables => Nodes.SelectMany(n => n.InputPins.SelectMany(p => p.InternalConnections))
                                                             .Concat(Nodes.SelectMany(n => n.OutputPins.SelectMany(p => p.InternalConnections)))
                                                             .Concat(Nodes.SelectMany(n => n.InputPinCollections.SelectMany(p => p.Pins).SelectMany(p => p.InternalConnections)))
                                                             .Concat(Nodes.SelectMany(n => n.OutputPinCollections.SelectMany(p => p.Pins).SelectMany(p => p.InternalConnections)))
                                                             .Distinct();

        public bool IsConnecting => IsConnectingPin != null;

        #endregion

        #region Constructors

        public VisualScript(IScript script, int surfaceSize, int gridSize)
        {
            this.Script = script;
            this.SurfaceSize = surfaceSize;
            this.GridSize = gridSize;

            Nodes.CollectionChanged += OnNodeCollectionChanged;
        }

        #endregion

        #region Methods

        public void DeselectAllNodes(VisualScriptNode except = null)
        {
            List<VisualScriptNode> selectedNodes = _selectedNodes.ToList();
            foreach (VisualScriptNode node in selectedNodes.Where(n => n != except))
                node.Deselect();
        }

        private void OnNodeCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            if (args.OldItems != null)
                UnregisterNodes(args.OldItems.Cast<VisualScriptNode>());

            if (args.NewItems != null)
                RegisterNodes(args.NewItems.Cast<VisualScriptNode>());
        }

        private void RegisterNodes(IEnumerable<VisualScriptNode> nodes)
        {
            foreach (VisualScriptNode node in nodes)
            {
                node.IsSelectedChanged += OnNodeIsSelectedChanged;
                node.DragStarting += OnNodeDragStarting;
                node.DragEnding += OnNodeDragEnding;
                node.DragMoving += OnNodeDragMoving;

                if (node.IsSelected)
                    _selectedNodes.Add(node);
            }
        }

        private void UnregisterNodes(IEnumerable<VisualScriptNode> nodes)
        {
            foreach (VisualScriptNode node in nodes)
            {
                node.IsSelectedChanged -= OnNodeIsSelectedChanged;
                node.DragStarting -= OnNodeDragStarting;
                node.DragEnding -= OnNodeDragEnding;
                node.DragMoving -= OnNodeDragMoving;

                _selectedNodes.Remove(node);
            }
        }

        private void OnNodeIsSelectedChanged(object sender, VisualScriptNodeIsSelectedChangedEventArgs args)
        {
            if (sender is not VisualScriptNode node) return;

            if (args.IsSelected)
            {
                if (!args.AlterSelection)
                    DeselectAllNodes(node);

                _selectedNodes.Add(node);
            }
            else
                _selectedNodes.Remove(node);
        }

        private void OnNodeDragStarting(object sender, EventArgs args)
        {
            _nodeDragAccumulationX = 0;
            _nodeDragAccumulationY = 0;
            _nodeStartPositions.Clear();

            foreach (VisualScriptNode node in _selectedNodes)
                _nodeStartPositions.Add(node, (node.X, node.Y));
        }

        private void OnNodeDragEnding(object sender, EventArgs args)
        {
            foreach (VisualScriptNode node in _selectedNodes)
                node.SnapNodeToGrid();
        }

        private void OnNodeDragMoving(object sender, VisualScriptNodeDragMovingEventArgs args)
        {
            _nodeDragAccumulationX += args.DX * NodeDragScale;
            _nodeDragAccumulationY += args.DY * NodeDragScale;

            foreach (VisualScriptNode node in _selectedNodes)
            {
                node.X = _nodeStartPositions[node].X + _nodeDragAccumulationX;
                node.Y = _nodeStartPositions[node].Y + _nodeDragAccumulationY;
                node.SnapNodeToGrid();
            }
        }

        internal void OnDragOver(Point position)
        {
            if (IsConnectingPin == null) return;

            IsConnectingPin.AbsolutePosition = position;
        }

        internal void OnIsConnectingPinChanged(VisualScriptPin isConnectingPin)
        {
            IsConnectingPin = isConnectingPin;
        }

        internal void OnPinConnected(PinConnectedEventArgs args)
        {
            OnPropertyChanged(nameof(Cables));
        }

        internal void OnPinDisconnected(PinDisconnectedEventArgs args)
        {
            OnPropertyChanged(nameof(Cables));
        }

        public void RemoveNode(VisualScriptNode node)
        {
            Nodes.Remove(node);
            Script.RemoveNode(node.Node);
        }

        internal void RecreateCables()
        {
            Dictionary<IPin, VisualScriptPin> pinMapping = Nodes.SelectMany(n => n.InputPins)
                                                                .Concat(Nodes.SelectMany(n => n.OutputPins))
                                                                .Concat(Nodes.SelectMany(n => n.InputPinCollections.SelectMany(p => p.Pins)))
                                                                .Concat(Nodes.SelectMany(n => n.OutputPinCollections.SelectMany(p => p.Pins)))
                                                                .ToDictionary(p => p.Pin, p => p);

            foreach (VisualScriptPin pin in pinMapping.Values)
                pin.InternalConnections.Clear();

            HashSet<IPin> connectedPins = new();
            foreach (IPin pin in pinMapping.Keys)
            {
                foreach (IPin connectedPin in pin.ConnectedTo)
                    if (!connectedPins.Contains(connectedPin))
                    {
                        VisualScriptPin pin1 = pinMapping[pin];
                        VisualScriptPin pin2 = pinMapping[connectedPin];
                        VisualScriptCable cable = new(pin1, pin2, false);
                        pin1.InternalConnections.Add(cable);
                        pin2.InternalConnections.Add(cable);
                    }

                connectedPins.Add(pin);
            }

            OnPropertyChanged(nameof(Cables));
        }

        #endregion
    }
}
