using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a condition and a value inside a <see cref="ConditionalDataBinding{TLayerProperty,TProperty}" />
    /// </summary>
    public interface IDataBindingCondition : IStorageModel, IDisposable
    {
        /// <summary>
        ///     Evaluates the condition
        /// </summary>
        bool Evaluate();
    }
}