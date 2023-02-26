using System.Threading.Tasks;

namespace Artemis.UI.Services.Updating;

public interface IUpdateNotificationProvider
{
    Task ShowNotification(string releaseId, string releaseVersion);
}