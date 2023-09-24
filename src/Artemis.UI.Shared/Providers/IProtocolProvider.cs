using System.Threading.Tasks;

namespace Artemis.UI.Shared.Providers;

/// <summary>
///     Represents a provider associating with a custom protocol, e.g. artemis://
/// </summary>
public interface IProtocolProvider
{
    /// <summary>
    ///     Associate Artemis with the provided custom protocol.
    /// </summary>
    /// <param name="protocol">The protocol to associate Artemis with.</param>
    Task AssociateWithProtocol(string protocol);

    /// <summary>
    ///     Disassociate Artemis with the provided custom protocol.
    /// </summary>
    /// <param name="protocol">The protocol to disassociate Artemis with.</param>
    Task DisassociateWithProtocol(string protocol);
}