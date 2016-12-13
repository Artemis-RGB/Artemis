using SKYPE4COMLib;

namespace Artemis86Wrapper.Intergrations.Skype
{
    public class SkypeModel : IIntergrationModel
    {
        public string Name { get; set; }
        public int UnreadMessages { get; set; }
        public int MissedCalls { get; set; }
        public int ActiveCalls { get; set; }
        public TUserStatus Status { get; set; }
    }
}