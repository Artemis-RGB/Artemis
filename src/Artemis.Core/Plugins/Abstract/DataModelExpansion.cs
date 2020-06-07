namespace Artemis.Core.Plugins.Abstract
{
    /// <inheritdoc />
    /// <summary>
    ///     Allows you to expand the application-wide datamodel
    /// </summary>
    public abstract class DataModelExpansion : Plugin
    {
        public abstract void Update(double deltaTime);
    }
}