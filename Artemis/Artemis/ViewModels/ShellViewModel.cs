using System;
using System.Windows;
using Artemis.Models;
using Caliburn.Micro;
using MessageBox = System.Windows.Forms.MessageBox;

namespace Artemis.ViewModels
{
    internal sealed class ShellViewModel : Conductor<IScreen>.Collection.OneActive
    {
        public ShellViewModel()
        {
            IEventAggregator events = new EventAggregator();
            MainModel = new MainModel(events);

            DisplayName = "Artemis";

            ActivateItem(new EffectsViewModel(MainModel) {DisplayName = "Effects"});
            ActivateItem(new GamesViewModel(MainModel) {DisplayName = "Games"});
            ActivateItem(new OverlaysViewModel(MainModel) {DisplayName = "Overlays"});

            // By now Effects are added to the MainModel so we can savely start one
            ToggleEffects();
        }

        public bool EffectsEnabled
        {
            get { return MainModel.Enabled; }
            private set
            {
                MainModel.Enabled = value;
                NotifyOfPropertyChange(() => EffectsEnabled);
            }
        }

        public MainModel MainModel { get; set; }

        public void ToggleEffects()
        {
            if (EffectsEnabled)
                MainModel.ShutdownEffects();
            else
                MainModel.StartEffects();

            EffectsEnabled = !EffectsEnabled;
        }

        public void OnClose(EventArgs e)
        {
            MainModel.ShutdownEffects();
            Application.Current.Shutdown();
        }

        public void ToggleSettings()
        {
            MessageBox.Show("Test");
        }
    }
}