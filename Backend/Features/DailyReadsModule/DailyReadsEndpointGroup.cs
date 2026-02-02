using FastEndpoints;

namespace PureTCOWebApp.Features.DailyReadsModule;

public class DailyReadsEndpointGroup : Group
{
    public DailyReadsEndpointGroup()
    {
        Configure("daily-reads", ep =>
        {
            ep.Description(x => x.WithTags("Daily Reads"));
            ep.Tags("Daily Reads Module");
            ep.Group<GlobalApiEndpointGroup>();
        });
    }
}
