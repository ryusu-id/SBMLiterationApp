using FastEndpoints;
using PureTCOWebApp.Features;

namespace PureTCOWebApp.Features.PushNotificationModule;

public class PushNotificationEndpointGroup : Group
{
    public PushNotificationEndpointGroup()
    {
        Configure("push", ep =>
        {
            ep.Group<GlobalApiEndpointGroup>();
            ep.Tags("Push Notification");
            ep.Description(e => e.WithTags("Push Notification"));
        });
    }
}
