namespace ShadowSession.Messages
{
    public abstract class MessageBase
    {
        protected MessageBase(object? sender)
        {
            Sender = sender;
        }

        public object? Sender { get; }
    }
}
