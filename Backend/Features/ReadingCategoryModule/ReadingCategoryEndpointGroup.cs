using FastEndpoints;

namespace PureTCOWebApp.Features.ReadingCategoryModule;

public class ReadingCategoryEndpointGroup : Group
{
    public ReadingCategoryEndpointGroup()
    {
        Configure("reading-categories", ep =>
        {
            ep.Description(x => x.WithTags("Reading Categories"));
            ep.Tags("Reading Category Module");
            ep.Group<GlobalApiEndpointGroup>();
        });
    }
}
