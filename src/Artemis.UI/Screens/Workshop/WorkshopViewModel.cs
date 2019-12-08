using System;
using System.ComponentModel;
using System.Windows.Media;
using Stylet;

namespace Artemis.UI.Screens.Workshop
{
    public class WorkshopViewModel : Screen, IScreenViewModel
    {
        public string Title => "Workshop";

        public WorkshopViewModel()
        {
            PropertyChanged += OnPropertyChanged;
        }

        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            Console.WriteLine("Property changed:" + e.PropertyName);
            Console.WriteLine(TestColor);
        }

        public Color TestColor { get; set; }


        protected override void OnActivate()
        {
            TestColor = Color.FromRgb(255, 0, 0);
            base.OnActivate();
        }

        protected override void OnDeactivate()
        {
            Console.WriteLine(TestColor);
            base.OnDeactivate();
        }
    }
}