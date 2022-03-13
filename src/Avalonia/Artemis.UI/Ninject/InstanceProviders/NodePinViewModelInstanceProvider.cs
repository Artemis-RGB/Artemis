using System;
using System.Reflection;
using Artemis.Core;
using Artemis.UI.Screens.VisualScripting.Pins;
using Ninject.Extensions.Factory;

namespace Artemis.UI.Ninject.InstanceProviders;

public class NodePinViewModelInstanceProvider : StandardInstanceProvider
{
    protected override Type GetType(MethodInfo methodInfo, object[] arguments)
    {
        if (methodInfo.ReturnType != typeof(PinCollectionViewModel) && methodInfo.ReturnType != typeof(PinViewModel))
            return base.GetType(methodInfo, arguments);

        if (arguments[0] is IPin pin)
            return CreatePinViewModelType(pin);
        if (arguments[0] is IPinCollection pinCollection)
            return CreatePinCollectionViewModelType(pinCollection);

        return base.GetType(methodInfo, arguments);
    }

    private Type CreatePinViewModelType(IPin pin)
    {
        if (pin.Direction == PinDirection.Input)
            return typeof(InputPinViewModel<>).MakeGenericType(pin.Type);
        return typeof(OutputPinViewModel<>).MakeGenericType(pin.Type);
    }

    private Type CreatePinCollectionViewModelType(IPinCollection pinCollection)
    {
        if (pinCollection.Direction == PinDirection.Input)
            return typeof(InputPinCollectionViewModel<>).MakeGenericType(pinCollection.Type);
        return typeof(OutputPinCollectionViewModel<>).MakeGenericType(pinCollection.Type);
    }
}