using FastEndpoints;

namespace PureTCOWebApp.Features;

public class GlobalApiEndpointGroup : Group
{
    public GlobalApiEndpointGroup()
    {
        Configure("api/v1", ep =>
        {
            ep.AllowAnonymous();
        });
    }
}