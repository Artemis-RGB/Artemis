namespace Artemis.Core
{
    /// <summary>
    /// Represents a data binding registration
    /// </summary>
    public interface IDataBindingRegistration
    {
        /// <summary>
        /// If found, creates a data binding from storage 
        /// </summary>
        /// <returns></returns>
        IDataBinding CreateDataBinding();
    }
}