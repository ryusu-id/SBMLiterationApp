using FastEndpoints;

namespace PureTCOWebApp.Features.GroupModule;

public class GroupEndpointGroup : FastEndpoints.Group
{
    public GroupEndpointGroup()
    {
        Configure("groups", ep =>
        {
            ep.Description(x => x.WithTags("Groups"));
            ep.Tags("Group Module");
            ep.Group<GlobalApiEndpointGroup>();
        });
    }
}
