using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a collection of output pins containing values of type <typeparamref name="T" />
    /// </summary>
    /// <typeparam name="T">The type of value the pins in this collection hold</typeparam>
    public sealed class OutputPinCollection<T> : PinCollection
    {
        #region Properties & Fields

        /// <inheritdoc />
        public override PinDirection Direction => PinDirection.Output;
        /// <inheritdoc />
        public override Type Type => typeof(T);

        #endregion

        #region Constructors

        internal OutputPinCollection(INode node, string name, int initialCount)
            : base(node, name, initialCount)
        { }

        #endregion

        #region Methods

        /// <inheritdoc />
        protected override IPin CreatePin() => new OutputPin<T>(Node, string.Empty);

        #endregion
    }
}
