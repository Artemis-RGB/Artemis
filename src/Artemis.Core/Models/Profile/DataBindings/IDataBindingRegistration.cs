using System;

namespace Artemis.Core
{
    /// <summary>
    ///     Represents a data binding registration
    /// </summary>
    public interface IDataBindingRegistration
    {
        /// <summary>
        ///     Gets or sets the display name of the data binding registration
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        ///     Gets the type of the value this data binding registration points to
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        ///     Returns the data binding applied using this registration
        /// </summary>
        public IDataBinding? GetDataBinding();

        /// <summary>
        ///     If found, creates a data binding from storage
        /// </summary>
        /// <returns></returns>
        IDataBinding? CreateDataBinding();

        /// <summary>
        ///     If present, removes the current data binding
        /// </summary>
        void ClearDataBinding();

        /// <summary>
        ///     Gets the value of the data binding
        /// </summary>
        /// <returns>A value matching the type of <see cref="ValueType" /></returns>
        object? GetValue();
    }
}