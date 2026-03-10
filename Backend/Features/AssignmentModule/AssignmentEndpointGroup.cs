using FastEndpoints;

namespace PureTCOWebApp.Features.AssignmentModule;

public class AssignmentEndpointGroup : Group
{
    public AssignmentEndpointGroup()
    {
        Configure("assignments", ep =>
        {
            ep.Description(x => x.WithTags("Assignments"));
            ep.Tags("Assignment Module");
            ep.Group<GlobalApiEndpointGroup>();
        });
    }
}
