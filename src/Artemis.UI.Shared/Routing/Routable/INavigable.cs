using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

public interface INavigable
{
    Task Navigated();
}

public interface INavigable<TParams>
{
    Task Navigated(TParams parameters);
}