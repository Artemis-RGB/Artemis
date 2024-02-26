using System.Text.Json;
using System.Threading;
using Artemis.UI.Extensions;
using Artemis.UI.Shared;
using Artemis.WebClient.Workshop.Services;
using PropertyChanged.SourceGenerator;

namespace Artemis.UI.Screens.Debugger.Workshop;

public partial class WorkshopDebugViewModel : ActivatableViewModelBase
{
    [Notify] private string? _token;
    [Notify] private bool _emailVerified;
    [Notify] private string? _claims;
    [Notify] private IWorkshopService.WorkshopStatus? _workshopStatus;

    public WorkshopDebugViewModel(IWorkshopService workshopService, IAuthenticationService authenticationService)
    {
        DisplayName = "Workshop";

        this.WhenActivatedAsync(async _ =>
        {
            Token = await authenticationService.GetBearer();
            EmailVerified = authenticationService.GetIsEmailVerified();
            Claims = JsonSerializer.Serialize(authenticationService.Claims, new JsonSerializerOptions {WriteIndented = true});
            WorkshopStatus = await workshopService.GetWorkshopStatus(CancellationToken.None);
        });
    }
}