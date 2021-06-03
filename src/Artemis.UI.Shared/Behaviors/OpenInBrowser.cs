using System.Windows.Documents;
using System.Windows.Navigation;
using Artemis.Core;
using Microsoft.Xaml.Behaviors;

namespace Artemis.UI.Shared
{
    /// <summary>
    ///     Represents a behavior that opens the URI of the hyperlink in the browser when requested
    /// </summary>
    public class OpenInBrowser : Behavior<Hyperlink>
    {
        private Hyperlink? _hyperLink;

        /// <inheritdoc />
        protected override void OnAttached()
        {
            base.OnAttached();

            _hyperLink = AssociatedObject;
            if (_hyperLink == null)
                return;

            _hyperLink.RequestNavigate += HyperLinkOnRequestNavigate;
        }

        /// <inheritdoc />
        protected override void OnDetaching()
        {
            if (_hyperLink == null) return;
            _hyperLink.RequestNavigate -= HyperLinkOnRequestNavigate;

            base.OnDetaching();
        }

        private void HyperLinkOnRequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            Utilities.OpenUrl(e.Uri.AbsoluteUri);
        }
    }
}