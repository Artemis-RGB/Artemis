using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Artemis.UI.Linux.Providers.Input
{
    public class ArtemisLinuxInputProviderException : Exception
    {
        public ArtemisLinuxInputProviderException() : base()
        {
        }

        public ArtemisLinuxInputProviderException(string? message) : base(message)
        {
        }

        public ArtemisLinuxInputProviderException(string? message, Exception? innerException) : base(message, innerException)
        {
        }
    }
}
