using CommunityToolkit.Mvvm.Messaging;
using ShadowSession.Messages;

namespace ShadowSession.Helpers
{
    public static class NotificationHelper
    {
        public static void Notify(string title, string text, NotificationSeverity severity)
        {

           if (severity != NotificationSeverity.Error && !UserSettingReader.AreNotificationsEnabled())
           {
                return;
           }

            WeakReferenceMessenger.Default.Send(new SendNotificationMessage(null, title, text, severity));
        }
    }
}
