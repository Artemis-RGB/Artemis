namespace Artemis.Events
{
    public class ChangeActiveEffect
    {
        public ChangeActiveEffect(string activeEffect)
        {
            ActiveEffect = activeEffect;
        }

        public string ActiveEffect { get; set; }
    }
}