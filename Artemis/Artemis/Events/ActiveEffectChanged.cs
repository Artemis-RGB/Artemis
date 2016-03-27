namespace Artemis.Events
{
    public class ActiveEffectChanged
    {
        public ActiveEffectChanged(string activeEffect)
        {
            ActiveEffect = activeEffect;
        }

        public string ActiveEffect { get; set; }
    }
}