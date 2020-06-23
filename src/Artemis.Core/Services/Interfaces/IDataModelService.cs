using Artemis.Core.Models;
using Artemis.Core.Plugins.Abstract.DataModels;

namespace Artemis.Core.Services.Interfaces
{
    public interface IDataModelService : IArtemisService
    {
        /// <summary>
        ///     Add an expansion to the datamodel to be available for use after the next update
        /// </summary>
        /// <param name="baseDataModelExpansion"></param>
        void AddExpansion(DataModel baseDataModelExpansion);

        /// <summary>
        ///     Remove a previously added expansion so that it is no longer available and updated
        /// </summary>
        /// <param name="baseDataModelExpansion"></param>
        void RemoveExpansion(DataModel baseDataModelExpansion);

        /// <summary>
        ///     Generates a data model description for the main datamodel including all it's expansions
        /// </summary>
        /// <returns>The generated data model description</returns>
        DataModelDescription GetMainDataModelDescription();
    }
}