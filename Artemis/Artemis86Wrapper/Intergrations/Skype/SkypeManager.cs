using System;
using System.Timers;
using SKYPE4COMLib;

namespace Artemis86Wrapper.Intergrations.Skype
{
    public class SkypeManager : IIntergrationManager
    {
        public SkypeManager()
        {
            IntergrationModel = new SkypeModel();
            LoopTimer = new Timer(5000);
            LoopTimer.Elapsed += UpdateSkype;
        }

        public SKYPE4COMLib.Skype Skype { get; set; }
        public Timer LoopTimer { get; set; }

        public IIntergrationModel IntergrationModel { get; set; }

        public void Start()
        {
            Skype = new SKYPE4COMLib.Skype();
            Skype.Attach();

            LoopTimer.Start();
        }

        public void Stop()
        {
            LoopTimer.Stop();
            Skype = null;
        }

        private void UpdateSkype(object sender, ElapsedEventArgs e)
        {
            try
            {
                var model = (SkypeModel) IntergrationModel;

                model.Name = Skype.CurrentUser.FullName;
                model.Status = Skype.CurrentUserStatus;
                model.MissedCalls = Skype.MissedCalls.Count;
                model.ActiveCalls = Skype.ActiveCalls.Count;
                model.UnreadMessages = 0;
                model.MissedCalls = model.MissedCalls = model.ActiveCalls;
                foreach (ChatMessage skypeMissedMessage in Skype.MissedMessages)
                    if ((skypeMissedMessage.Type == TChatMessageType.cmeSaid) &&
                        (skypeMissedMessage.Status != TChatMessageStatus.cmsRead) &&
                        (skypeMissedMessage.FromHandle != Skype.CurrentUser.Handle))
                        model.UnreadMessages++;
            }
            catch (Exception ex)
            {
                // TODO: Log exception to main program
            }
        }
    }
}