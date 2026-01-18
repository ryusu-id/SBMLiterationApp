using FastEndpoints;
using PureTCOWebApp.Features;

namespace PureTCOWebApp.Features.ReadingResourceModule;

public class ReadingResourceEndpointGroup : Group
{
    public ReadingResourceEndpointGroup()
    {
        Configure("reading-resources", ep =>
        {
            ep.Group<GlobalApiEndpointGroup>();
            ep.Tags("Reading Resource Module");
            ep.Description(e => e.WithTags("Reading Resource Module"));
        });
    }
}