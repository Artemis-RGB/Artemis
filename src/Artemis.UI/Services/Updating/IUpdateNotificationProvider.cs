using System.Threading.Tasks;

namespace Artemis.UI.Services.Updating;

public interface IUpdateNotificationProvider
{
    void ShowNotification(string releaseId, string releaseVersion);
}