using System.Collections.Generic;
using Artemis.UI.Screens.Home;
using Artemis.UI.Screens.Profiles.ProfileEditor;
using Artemis.UI.Screens.Root;
using Artemis.UI.Screens.Settings;
using Artemis.UI.Screens.Settings.Updating;
using Artemis.UI.Screens.SurfaceEditor;
using Artemis.UI.Screens.Workshop;
using Artemis.UI.Screens.Workshop.Entries;
using Artemis.UI.Screens.Workshop.EntryReleases;
using Artemis.UI.Screens.Workshop.Home;
using Artemis.UI.Screens.Workshop.Layout;
using Artemis.UI.Screens.Workshop.Library;
using Artemis.UI.Screens.Workshop.Library.Tabs;
using Artemis.UI.Screens.Workshop.Plugins;
using Artemis.UI.Screens.Workshop.Profile;
using Artemis.UI.Shared.Routing;
using PluginDetailsViewModel = Artemis.UI.Screens.Workshop.Plugins.PluginDetailsViewModel;
using ProfileEditorViewModel = Artemis.UI.Screens.Profiles.ProfileEditor.ProfileEditorViewModel;

namespace Artemis.UI.Routing
{
    public static class Routes
    {
        public static readonly List<IRouterRegistration> ArtemisRoutes =
        [
            new RouteRegistration<BlankViewModel>("blank"),
            new RouteRegistration<HomeViewModel>("home"),
            new RouteRegistration<WorkshopViewModel>("workshop", [
                new RouteRegistration<WorkshopOfflineViewModel>("offline/{message:string}"),
                new RouteRegistration<EntriesViewModel>("entries", [
                    new RouteRegistration<PluginListViewModel>("plugins", [
                        new RouteRegistration<PluginDetailsViewModel>("details/{entryId:long}", [
                            new RouteRegistration<PluginManageViewModel>("manage"),
                            new RouteRegistration<EntryReleaseViewModel>("releases/{releaseId:long}")
                        ])
                    ]),
                    new RouteRegistration<ProfileListViewModel>("profiles", [
                        new RouteRegistration<ProfileDetailsViewModel>("details/{entryId:long}", [
                            new RouteRegistration<EntryReleaseViewModel>("releases/{releaseId:long}")
                        ])
                    ]),
                    new RouteRegistration<LayoutListViewModel>("layouts", [
                        new RouteRegistration<LayoutDetailsViewModel>("details/{entryId:long}", [
                            new RouteRegistration<LayoutManageViewModel>("manage"),
                            new RouteRegistration<EntryReleaseViewModel>("releases/{releaseId:long}")
                        ])
                    ])
                ]),
                new RouteRegistration<WorkshopLibraryViewModel>("library", [
                    new RouteRegistration<InstalledTabViewModel>("installed"),
                    new RouteRegistration<SubmissionsTabViewModel>("submissions"),
                    new RouteRegistration<SubmissionManagementViewModel>("submissions/{entryId:long}", [
                        new RouteRegistration<SubmissionReleaseViewModel>("releases/{releaseId:long}")
                    ])
                ])
            ]),
            new RouteRegistration<SurfaceEditorViewModel>("surface-editor"),
            new RouteRegistration<SettingsViewModel>("settings", [
                new RouteRegistration<GeneralTabViewModel>("general"),
                new RouteRegistration<PluginsTabViewModel>("plugins"),
                new RouteRegistration<DevicesTabViewModel>("devices"),
                new RouteRegistration<ReleasesTabViewModel>("releases", [
                    new RouteRegistration<ReleaseDetailsViewModel>("{releaseId:guid}")
                ]),
                new RouteRegistration<AccountTabViewModel>("account"),
                new RouteRegistration<AboutTabViewModel>("about")
            ]),
            new RouteRegistration<ProfileEditorViewModel>("profile-editor/{profileConfigurationId:guid}")
        ];
    }
}