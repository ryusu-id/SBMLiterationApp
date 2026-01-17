using FastEndpoints;

namespace PureTCOWebApp;

public class GlobalApiEndpointGroup : Group
{
    public GlobalApiEndpointGroup()
    {
        Configure("api", ep =>
        {
        });
    }
}