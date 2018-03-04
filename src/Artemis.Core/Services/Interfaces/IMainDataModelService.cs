using Artemis.Core.Models;
using Artemis.Plugins.Interfaces;

namespace Artemis.Core.Services.Interfaces
{
    public interface IMainDataModelService : IArtemisService
    {
        /// <summary>
        ///     Called each frame when the main data model must update
        /// </summary>
        /// <param name="deltaTime">Time since the last update</param>
        void Update(double deltaTime);

        /// <summary>
        ///     Add an expansion to the datamodel to be available for use after the next update
        /// </summary>
        /// <param name="dataModelExpansion"></param>
        void AddExpansion(IDataModelExpansion dataModelExpansion);

        /// <summary>
        ///     Remove a previously added expansion so that it is no longer available and updated
        /// </summary>
        /// <param name="dataModelExpansion"></param>
        void RemoveExpansion(IDataModelExpansion dataModelExpansion);

        /// <summary>
        ///     Generates a data model description for the main datamodel including all it's expansions
        /// </summary>
        /// <returns>The generated data model description</returns>
        DataModelDescription GetMainDataModelDescription();
    }
}