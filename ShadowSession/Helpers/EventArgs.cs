namespace ShadowSession.Helpers
{
    public class EventArgs<T>(T? value) : EventArgs
    {
        public T? Value { get; } = value;
    }
}
