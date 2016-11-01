namespace Artemis.Profiles.Layers.Types.AmbientLight.AmbienceCreator
{
    public interface IAmbienceCreator
    {
        byte[] GetAmbience(byte[] pixels, int sourceWidth, int sourceHeight, int targetWidth, int targetHeight, AmbientLightPropertiesModel settings);
    }
}
