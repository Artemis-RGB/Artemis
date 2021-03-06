﻿using MaterialDesignThemes.Wpf;

namespace Artemis.UI.Screens.Sidebar.Dialogs.ProfileEdit
{
    public class ProfileIconViewModel
    {
        public ProfileIconViewModel(PackIconKind icon)
        {
            Icon = icon;
            IconName = icon.ToString();
        }

        public PackIconKind Icon { get; }
        public string IconName { get; }
    }
}