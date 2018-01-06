using System;

namespace Artemis.Core.Services.Interfaces
{
    public interface ICoreService: IArtemisService, IDisposable
    {
        bool IsInitialized { get; set; }
    }
}