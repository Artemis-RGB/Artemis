using System;
using System.Collections.Generic;
using System.Text;
using Artemis.Core.Services.Interfaces;

namespace Artemis.UI.Shared.Services.Interfaces
{
    public interface IDataBindingUIService : IArtemisSharedUIService
    {
        object GetDataBindingViewModel(Type propertyType);
    }
}