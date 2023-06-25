using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Artemis.UI.Shared.Routing;

public interface IRouter
{
    IRoutable? Root { get; set; }
    IObservable<string?> CurrentPath { get; }
    List<IRouterRegistration> Routes { get; }

    Task Navigate(string path, RouterNavigationOptions? options = null);
    Task<bool> GoBack();
    Task<bool> GoForward();
}