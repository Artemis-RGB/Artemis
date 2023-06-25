using System.Collections.Generic;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

public interface IRouter
{
    IRoutable? Root { get; set; }
    string? CurrentPath { get; }
    List<IRouterRegistration> Routes { get; }
    
    Task<bool> Navigate(string path, RouterNavigationOptions? options = null);
    Task<bool> GoBack();
    Task<bool> GoForward();
}