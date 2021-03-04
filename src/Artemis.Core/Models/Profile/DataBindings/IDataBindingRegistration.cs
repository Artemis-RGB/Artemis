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
        ///     Returns the data binding applied using this registration
        /// </summary>
        public IDataBinding? GetDataBinding();

        /// <summary>
        ///     If found, creates a data binding from storage
        /// </summary>
        /// <returns></returns>
        IDataBinding? CreateDataBinding();
    }
}