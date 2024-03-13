using System;

namespace Artemis.Storage.Exceptions;

public class ArtemisStorageException : Exception
{
    public ArtemisStorageException(string message) : base(message)
    {
    }
    
    public ArtemisStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}