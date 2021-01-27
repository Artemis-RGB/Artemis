using System.Threading.Tasks;
using EmbedIO;

namespace Artemis.Core.Services
{
    internal class PluginsModule : WebModuleBase
    {
        /// <inheritdoc />
        public PluginsModule(string baseRoute) : base(baseRoute)
        {
        }

        #region Overrides of WebModuleBase

        /// <inheritdoc />
        protected override async Task OnRequestAsync(IHttpContext context)
        {
        }

        /// <inheritdoc />
        public override bool IsFinalHandler => true;

        #endregion
    }
}