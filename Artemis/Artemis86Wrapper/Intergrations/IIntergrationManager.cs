namespace Artemis86Wrapper.Intergrations
{
    public interface IIntergrationManager
    {
        IIntergrationModel IntergrationModel { get; set; }

        void Start();
        void Stop();
    }
}