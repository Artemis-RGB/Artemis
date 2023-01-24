using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Artemis.Core;
using Ninject;
using Ninject.Parameters;

namespace Artemis.UI.Shared.Services.PropertyInput;

internal class PropertyInputService : IPropertyInputService
{
    private readonly IKernel _kernel;
    private readonly List<PropertyInputRegistration> _registeredPropertyEditors;

    public PropertyInputService(IKernel kernel)
    {
        _kernel = kernel;
        _registeredPropertyEditors = new List<PropertyInputRegistration>();
        RegisteredPropertyEditors = new ReadOnlyCollection<PropertyInputRegistration>(_registeredPropertyEditors);
    }

    /// <inheritdoc />
    public ReadOnlyCollection<PropertyInputRegistration> RegisteredPropertyEditors { get; }

    public PropertyInputRegistration RegisterPropertyInput<T>(Plugin plugin) where T : PropertyInputViewModel
    {
        return RegisterPropertyInput(typeof(T), plugin);
    }

    public PropertyInputRegistration RegisterPropertyInput(Type viewModelType, Plugin plugin)
    {
        if (!typeof(PropertyInputViewModel).IsAssignableFrom(viewModelType))
            throw new ArtemisSharedUIException($"Property input VM type must implement {nameof(PropertyInputViewModel)}");

        lock (_registeredPropertyEditors)
        {
            // Indirectly checked if there's a BaseType above
            Type supportedType = viewModelType.BaseType!.GetGenericArguments()[0];
            // If the supported type is a generic, assume there is a base type
            if (supportedType.IsGenericParameter)
            {
                if (supportedType.BaseType == null)
                    throw new ArtemisSharedUIException("Generic property input VM type must have a type constraint");
                supportedType = supportedType.BaseType;
            }

            PropertyInputRegistration? existing = _registeredPropertyEditors.FirstOrDefault(r => r.SupportedType == supportedType);
            if (existing != null)
            {
                if (existing.Plugin != plugin)
                    throw new ArtemisSharedUIException($"Cannot register property editor for type {supportedType.Name} because an editor was already " +
                                                       $"registered by {existing.Plugin}");

                return existing;
            }

            _kernel.Bind(viewModelType).ToSelf();
            PropertyInputRegistration registration = new(this, plugin, supportedType, viewModelType);
            _registeredPropertyEditors.Add(registration);
            return registration;
        }
    }

    public void RemovePropertyInput(PropertyInputRegistration registration)
    {
        lock (_registeredPropertyEditors)
        {
            if (_registeredPropertyEditors.Contains(registration))
            {
                registration.Unsubscribe();
                _registeredPropertyEditors.Remove(registration);

                _kernel.Unbind(registration.ViewModelType);
            }
        }
    }

    public bool CanCreatePropertyInputViewModel(ILayerProperty layerProperty)
    {
        PropertyInputRegistration? registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == layerProperty.PropertyType);
        if (registration == null && layerProperty.PropertyType.IsEnum)
            registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == typeof(Enum));

        return registration != null;
    }

    public PropertyInputViewModel<T>? CreatePropertyInputViewModel<T>(LayerProperty<T> layerProperty)
    {
        Type? viewModelType = null;
        PropertyInputRegistration? registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == typeof(T));

        // Check for enums if no supported type was found
        if (registration == null && typeof(T).IsEnum)
        {
            // The enum VM will likely be a generic, that requires creating a generic type matching the layer property
            registration = RegisteredPropertyEditors.FirstOrDefault(r => r.SupportedType == typeof(Enum));
            if (registration != null && registration.ViewModelType.IsGenericType)
                viewModelType = registration.ViewModelType.MakeGenericType(layerProperty.GetType().GenericTypeArguments);
        }
        else if (registration != null)
        {
            viewModelType = registration.ViewModelType;
        }
        else
        {
            return null;
        }

        if (viewModelType == null)
            return null;

        ConstructorArgument parameter = new("layerProperty", layerProperty);
        // ReSharper disable once InconsistentlySynchronizedField
        // When you've just spent the last 2 hours trying to figure out a deadlock and reach this line, I'm so, so sorry. I thought this would be fine.
        IKernel kernel = registration?.Plugin.Container ?? _kernel;
        return (PropertyInputViewModel<T>) kernel.Get(viewModelType, parameter);
    }
}