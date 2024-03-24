using System.Collections.Generic;
using Artemis.UI.Screens.Home;
using Artemis.UI.Screens.ProfileEditor;
using Artemis.UI.Screens.Root;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Settings.Updating;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.Workshop;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Screens.Workshop.Home;
using Artemis.UI.Screens.Workshop.Layout;
using Artemis.UI.Screens.Workshop.Library;
using Artemis.UI.Screens.Workshop.Library.Tabs;
using Artemis.UI.Screens.Workshop.Plugins;
using Artemis.UI.Screens.Workshop.Profile;
using Artemis.UI.Shared.Routing;
using PluginDetailsViewModel = Artemis.UI.Screens.Workshop.Plugins.PluginDetailsViewModel;

namespace Artemis.UI.Routing;

public static class Routes
{
    public static List<IRouterRegistration> ArtemisRoutes =
    [
        new RouteRegistration<BlankViewModel>("blank"),
        new RouteRegistration<HomeViewModel>("home"),
        new RouteRegistration<WorkshopViewModel>("workshop")
        {
            Children =
            [
                new RouteRegistration<WorkshopOfflineViewModel>("offline/{message:string}"),
                new RouteRegistration<EntriesViewModel>("entries")
                {
                    Children =
                    [
                        new RouteRegistration<PluginListViewModel>("plugins")
                        {
                            Children = [new RouteRegistration<PluginDetailsViewModel>("details/{entryId:long}")]
                        },
                        new RouteRegistration<ProfileListViewModel>("profiles")
                        {
                            Children = [new RouteRegistration<ProfileDetailsViewModel>("details/{entryId:long}")]
                        },
                        new RouteRegistration<LayoutListViewModel>("layouts")
                        {
                            Children = [new RouteRegistration<LayoutDetailsViewModel>("details/{entryId:long}")]
                        },
                    ]
                },

                new RouteRegistration<WorkshopLibraryViewModel>("library")
                {
                    Children =
                    [
                        new RouteRegistration<InstalledTabViewModel>("installed"),
                        new RouteRegistration<SubmissionsTabViewModel>("submissions"),
                        new RouteRegistration<SubmissionDetailViewModel>("submissions/{entryId:long}")
                    ]
                }
            ]
        },

        new RouteRegistration<SurfaceEditorViewModel>("surface-editor"),
        new RouteRegistration<SettingsViewModel>("settings")
        {
            Children =
            [
                new RouteRegistration<GeneralTabViewModel>("general"),
                new RouteRegistration<PluginsTabViewModel>("plugins"),
                new RouteRegistration<DevicesTabViewModel>("devices"),
                new RouteRegistration<ReleasesTabViewModel>("releases")
                {
                    Children = [new RouteRegistration<ReleaseDetailsViewModel>("{releaseId:guid}")]
                },

                new RouteRegistration<AccountTabViewModel>("account"),
                new RouteRegistration<AboutTabViewModel>("about")
            ]
        },

        new RouteRegistration<ProfileEditorViewModel>("profile-editor/{profileConfigurationId:guid}")
    ];
}