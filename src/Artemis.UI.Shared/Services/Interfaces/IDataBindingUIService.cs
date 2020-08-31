using System;

namespace Artemis.UI.Shared.Services
{
    public interface IDataBindingUIService : IArtemisSharedUIService
    {
        object GetDataBindingViewModel(Type propertyType);
    }
}