using System;

namespace Artemis.Core.Plugins.Abstract.DataModels.Attributes
{
    [AttributeUsage(System.AttributeTargets.Property)]
    public class DataModelPropertyAttribute : Attribute
    {
        /// <summary>
        ///     Gets or sets the user-friendly name for this property, shown in the UI.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets the user-friendly description for this property, shown in the UI.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        ///     Gets or sets the an optional input prefix to show before input elements in the UI.
        /// </summary>
        public string InputPrefix { get; set; }

        /// <summary>
        ///     Gets or sets an optional input affix to show behind input elements in the UI.
        /// </summary>
        public string InputAffix { get; set; }

        /// <summary>
        ///     Gets or sets an optional maximum input value, only enforced in the UI.
        /// </summary>
        public object MaxInputValue { get; set; }

        /// <summary>
        ///     Gets or sets the input drag step size, used in the UI.
        /// </summary>
        public float InputStepSize { get; set; }

        /// <summary>
        ///     Gets or sets an optional minimum input value, only enforced in the UI.
        /// </summary>
        public object MinInputValue { get; set; }
    }
}