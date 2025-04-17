using ShadowSession.Helpers;

namespace ShadowSession.Messages
{
    public class SendNotificationMessage(object? sender, string title, string text, NotificationSeverity severity) : MessageBase(sender)
    {
        public string Title { get; } = title;

        public string Text { get; } = text;

        public NotificationSeverity Severity { get; } = severity;
    }
}
