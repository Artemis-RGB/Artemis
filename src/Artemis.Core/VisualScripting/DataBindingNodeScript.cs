using System;
using Artemis.Core.Internal;
using Artemis.Storage.Entities.Profile.Nodes;

namespace Artemis.Core
{
    public class DataBindingNodeScript<TLayerProperty> : NodeScript
    {
        #region Properties & Fields

        internal DataBindingExitNode<TLayerProperty> DataBindingExitNode { get; }

        /// <inheritdoc />
        public override Type ResultType => typeof(object);

        #endregion

        /// <inheritdoc />
        public DataBindingNodeScript(string name, string description, DataBinding<TLayerProperty> dataBinding, object? context = null)
            : base(name, description, context)
        {
            DataBindingExitNode = new DataBindingExitNode<TLayerProperty>(dataBinding);
            ExitNode = DataBindingExitNode;
            AddNode(ExitNode);
        }

        /// <inheritdoc />
        public DataBindingNodeScript(string name, string description, DataBinding<TLayerProperty> dataBinding, NodeScriptEntity entity, object? context = null)
            : base(name, description, entity, context)
        {
            DataBindingExitNode = new DataBindingExitNode<TLayerProperty>(dataBinding);
            ExitNode = DataBindingExitNode;
            AddNode(ExitNode);
        }
    }
}