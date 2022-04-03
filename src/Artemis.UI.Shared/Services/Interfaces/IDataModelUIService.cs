using System;
using System.Collections.Generic;
using Artemis.Core;
using Artemis.Core.Modules;
using Artemis.UI.Shared.DataModelVisualization;
using Artemis.UI.Shared.DataModelVisualization.Shared;

namespace Artemis.UI.Shared.Services
{
    /// <summary>
    ///     A service for UI related data model tasks
    /// </summary>
    public interface IDataModelUIService : IArtemisSharedUIService
    {
        /// <summary>
        ///     Gets a read-only list of all registered data model editors
        /// </summary>
        IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelEditors { get; }

        /// <summary>
        ///     Gets a read-only list of all registered data model displays
        /// </summary>
        IReadOnlyCollection<DataModelVisualizationRegistration> RegisteredDataModelDisplays { get; }

        /// <summary>
        ///     Creates a data model visualization view model for the main data model
        /// </summary>
        /// <returns>
        ///     A data model visualization view model containing all data model expansions and modules that expand the main
        ///     data model
        /// </returns>
        DataModelPropertiesViewModel GetMainDataModelVisualization();

        /// <summary>
        ///     Creates a data model visualization view model for the data model of the provided plugin feature
        /// </summary>
        /// <param name="modules">The modules to create the data model visualization view model for</param>
        /// <param name="includeMainDataModel">
        ///     Whether or not also to include the main data model (and therefore any modules marked
        ///     as <see cref="Module.IsAlwaysAvailable" />)
        /// </param>
        /// <returns>A data model visualization view model containing the data model of the provided feature</returns>
        DataModelPropertiesViewModel? GetPluginDataModelVisualization(List<Module> modules, bool includeMainDataModel);

        /// <summary>
        ///     Updates the children of the provided main data model visualization, removing disabled children and adding newly
        ///     enabled children
        /// </summary>
        void UpdateModules(DataModelPropertiesViewModel mainDataModelVisualization);

        /// <summary>
        ///     Registers a new data model editor
        /// </summary>
        /// <typeparam name="T">The type of the editor</typeparam>
        /// <param name="plugin">The plugin this editor belongs to</param>
        /// <param name="compatibleConversionTypes">A collection of extra types this editor supports</param>
        /// <returns>A registration that can be used to remove the editor</returns>
        DataModelVisualizationRegistration RegisterDataModelInput<T>(Plugin plugin, IReadOnlyCollection<Type> compatibleConversionTypes) where T : DataModelInputViewModel;

        /// <summary>
        ///     Registers a new data model display
        /// </summary>
        /// <typeparam name="T">The type of the display</typeparam>
        /// <param name="plugin">The plugin this display belongs to</param>
        /// <returns>A registration that can be used to remove the display</returns>
        DataModelVisualizationRegistration RegisterDataModelDisplay<T>(Plugin plugin) where T : DataModelDisplayViewModel;

        /// <summary>
        ///     Removes a data model editor
        /// </summary>
        /// <param name="registration">
        ///     The registration of the editor as returned by <see cref="RegisterDataModelInput{T}" />
        /// </param>
        void RemoveDataModelInput(DataModelVisualizationRegistration registration);

        /// <summary>
        ///     Removes a data model display
        /// </summary>
        /// <param name="registration">
        ///     The registration of the display as returned by <see cref="RegisterDataModelDisplay{T}" />
        /// </param>
        void RemoveDataModelDisplay(DataModelVisualizationRegistration registration);

        /// <summary>
        ///     Creates the most appropriate display view model for the provided <paramref name="propertyType" /> that can display
        ///     a value
        /// </summary>
        /// <param name="propertyType">The type of data model property to find a display view model for</param>
        /// <param name="description">The description of the data model property</param>
        /// <param name="fallBackToDefault">
        ///     If <see langword="true"></see>, a simple .ToString() display view model will be
        ///     returned if nothing else is found
        /// </param>
        /// <returns>The most appropriate display view model for the provided <paramref name="propertyType"></paramref></returns>
        DataModelDisplayViewModel? GetDataModelDisplayViewModel(Type propertyType, DataModelPropertyAttribute? description, bool fallBackToDefault = false);

        /// <summary>
        ///     Creates the most appropriate input view model for the provided <paramref name="propertyType" /> that allows
        ///     inputting a value
        /// </summary>
        /// <param name="propertyType">The type of data model property to find a display view model for</param>
        /// <param name="description">The description of the data model property</param>
        /// <param name="initialValue">The initial value to show in the input</param>
        /// <param name="updateCallback">A function to call whenever the input was updated (submitted or not)</param>
        /// <returns>The most appropriate input view model for the provided <paramref name="propertyType" /></returns>
        DataModelInputViewModel? GetDataModelInputViewModel(Type propertyType, DataModelPropertyAttribute? description, object? initialValue, Action<object?, bool> updateCallback);
    }
}