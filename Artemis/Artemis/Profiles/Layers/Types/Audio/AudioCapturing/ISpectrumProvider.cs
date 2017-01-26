namespace Artemis.Profiles.Layers.Types.Audio.AudioCapturing
{
    public interface ISpectrumProvider
    {
        bool GetFftData(float[] fftBuffer, object context);
        int GetFftBandIndex(float frequency);
    }
}