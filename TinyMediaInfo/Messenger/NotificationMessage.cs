using Avalonia.Controls.Notifications;

namespace TinyMediaInfo.Messenger;

public class NotificationMessage
{
    public INotification Notification { get; set; }

    public NotificationMessage(INotification notification)
    {
        Notification = notification;
    }
}