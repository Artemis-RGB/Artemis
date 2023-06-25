using System.Threading.Tasks;
using DryIoc;

namespace Artemis.UI.Shared.Routing;

public interface IRoutable
{
    Task<bool> Activate(RouteResolution routeResolution, IContainer container);
}