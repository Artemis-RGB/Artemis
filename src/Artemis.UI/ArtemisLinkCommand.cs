using System;
using System.Windows.Input;
using Artemis.Core;
using Artemis.UI.Shared.Routing;

namespace Artemis.UI;

public class ArtemisLinkCommand: ICommand
{
    public static IRouter? Router;
    
    /// <inheritdoc />
    public bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public void Execute(object? parameter)
    {
        if (parameter is not string url || !Uri.TryCreate(url, UriKind.Absolute, out Uri? uri))
            return;
        
        if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps)
            Utilities.OpenUrl(url);
        else if (uri.Scheme == "artemis")
            Router?.Navigate(uri.Host + uri.LocalPath);
    }

    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

}