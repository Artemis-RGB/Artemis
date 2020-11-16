using System;
using Artemis.Core.DataModelExpansions;

namespace Artemis.Core
{
    internal class EventPredicateWrapperDataModel<T> : EventPredicateWrapperDataModel
    {
        [DataModelProperty(Name = "Event arguments", Description = "The arguments provided when the event triggers")]
        public T Arguments => (UntypedArguments is T typedArguments ? typedArguments : default)!;
    }

    /// <summary>
    ///     Represents a datamodel that wraps the event arguments of an event
    /// </summary>
    public abstract class EventPredicateWrapperDataModel : DataModel
    {
        internal EventPredicateWrapperDataModel()
        {
            Feature = Constants.CorePluginFeature;
        }

        /// <summary>
        ///     Gets the last arguments of this event as an object
        /// </summary>
        [DataModelIgnore]
        public object? UntypedArguments { get; internal set; }

        /// <summary>
        ///     Creates a new instance of the <see cref="EventPredicateWrapperDataModel" /> class
        /// </summary>
        public static EventPredicateWrapperDataModel Create(Type type)
        {
            object? instance = Activator.CreateInstance(typeof(EventPredicateWrapperDataModel<>).MakeGenericType(type));
            if (instance == null)
                throw new ArtemisCoreException($"Failed to create an instance of EventPredicateWrapperDataModel<T> for type {type.Name}");

            return (EventPredicateWrapperDataModel) instance;
        }
    }
}