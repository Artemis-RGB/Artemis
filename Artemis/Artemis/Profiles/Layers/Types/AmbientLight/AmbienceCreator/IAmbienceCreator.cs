namespace Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator
{
    public interface IAmbienceCreator
    {
        byte[] GetAmbience(byte[] data, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight);
    }
}
