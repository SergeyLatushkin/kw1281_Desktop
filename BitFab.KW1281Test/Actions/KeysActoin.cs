namespace BitFab.KW1281Test.Actions
{
    public class KeysActoin
    {
        public event Action<string>? KeyPressed;

        public void Add(string key)
        {
            KeyPressed?.Invoke(key);
        }
    }
}
