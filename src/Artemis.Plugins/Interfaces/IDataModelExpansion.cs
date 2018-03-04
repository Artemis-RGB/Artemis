namespace Artemis.Plugins.Interfaces
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to expand the application-wide datamodel
    /// </summary>
    public interface IDataModelExpansion : IPlugin
    {
        void Update(double deltaTime);
    }
}