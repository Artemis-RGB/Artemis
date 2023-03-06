using System;
using System.Threading.Tasks;

namespace Artemis.UI.Services.Updating;

public interface IUpdateNotificationProvider
{
    void ShowNotification(Guid releaseId, string releaseVersion);
}