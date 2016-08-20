namespace Artemis.Settings
{
    public interface IArtemisSettings
    {
        string Name { get; }
        void Save();
    }
}