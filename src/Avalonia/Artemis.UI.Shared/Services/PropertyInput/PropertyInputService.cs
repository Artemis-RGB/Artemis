using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Artemis.Core;
using Artemis.UI.Shared.Services.Interfaces;

namespace Artemis.UI.Shared.Services.PropertyInput
{
    internal class PropertyInputService : IPropertyInputService
    {
        private readonly List<PropertyInputRegistration> _registeredPropertyEditors;

        public PropertyInputService()
        {
            _registeredPropertyEditors = new List<PropertyInputRegistration>();
            RegisteredPropertyEditors = new ReadOnlyCollection<PropertyInputRegistration>(_registeredPropertyEditors);
        }

        /// <inheritdoc />
        public ReadOnlyCollection<PropertyInputRegistration> RegisteredPropertyEditors { get; }

        /// <inheritdoc />
        public PropertyInputRegistration RegisterPropertyInput<T>(Plugin plugin) where T : PropertyInputViewModel
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public PropertyInputRegistration RegisterPropertyInput(Type viewModelType, Plugin plugin)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void RemovePropertyInput(PropertyInputRegistration registration)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool CanCreatePropertyInputViewModel(ILayerProperty layerProperty)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public PropertyInputViewModel<T>? CreatePropertyInputViewModel<T>(LayerProperty<T> layerProperty)
        {
            throw new NotImplementedException();
        }
    }

    public interface IPropertyInputService : IArtemisSharedUIService
    {
        /// <summary>
        ///     Gets a read-only collection of all registered property editors
        /// </summary>
        ReadOnlyCollection<PropertyInputRegistration> RegisteredPropertyEditors { get; }

        /// <summary>
        ///     Registers a new property input view model used in the profile editor for the generic type defined in
        ///     <see cref="PropertyInputViewModel{T}" />
        ///     <para>Note: DataBindingProperty will remove itself on plugin disable so you don't have to</para>
        /// </summary>
        /// <param name="plugin"></param>
        /// <returns></returns>
        PropertyInputRegistration RegisterPropertyInput<T>(Plugin plugin) where T : PropertyInputViewModel;

        /// <summary>
        ///     Registers a new property input view model used in the profile editor for the generic type defined in
        ///     <see cref="PropertyInputViewModel{T}" />
        ///     <para>Note: DataBindingProperty will remove itself on plugin disable so you don't have to</para>
        /// </summary>
        /// <param name="viewModelType"></param>
        /// <param name="plugin"></param>
        /// <returns></returns>
        PropertyInputRegistration RegisterPropertyInput(Type viewModelType, Plugin plugin);

        /// <summary>
        ///     Removes the property input view model
        /// </summary>
        /// <param name="registration"></param>
        void RemovePropertyInput(PropertyInputRegistration registration);

        /// <summary>
        ///     Determines if there is a matching registration for the provided layer property
        /// </summary>
        /// <param name="layerProperty">The layer property to try to find a view model for</param>
        bool CanCreatePropertyInputViewModel(ILayerProperty layerProperty);

        /// <summary>
        ///     If a matching registration is found, creates a new <see cref="PropertyInputViewModel{T}" /> supporting
        ///     <typeparamref name="T" />
        /// </summary>
        PropertyInputViewModel<T>? CreatePropertyInputViewModel<T>(LayerProperty<T> layerProperty);
    }
}