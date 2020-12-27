using Artemis.UI.Installer.Services.Prerequisites;
using Stylet;

namespace Artemis.UI.Installer.Screens.Steps.Prerequisites
{
    public class PrerequisiteViewModel : PropertyChangedBase
    {
        private string _description;
        private bool _isMet;
        private string _title;

        public PrerequisiteViewModel(IPrerequisite prerequisite)
        {
            Prerequisite = prerequisite;
        }

        public IPrerequisite Prerequisite { get; }

        public string Title
        {
            get => _title;
            set => SetAndNotify(ref _title, value);
        }

        public string Description
        {
            get => _description;
            set => SetAndNotify(ref _description, value);
        }

        public bool IsMet
        {
            get => _isMet;
            set => SetAndNotify(ref _isMet, value);
        }

        public void Update()
        {
            Title = Prerequisite.Title;
            Description = Prerequisite.Description;
            IsMet = Prerequisite.IsMet();
        }
    }
}