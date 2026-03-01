using FastEndpoints;

namespace PureTCOWebApp.Features.EmailModule;

public class EmailModuleEndpointGroup : Group
{
    public EmailModuleEndpointGroup()
    {
        Configure("email", ep =>
        {
            ep.Description(x => x.WithTags("Email"));
            ep.Tags("Email Module");
            ep.Group<GlobalApiEndpointGroup>();
        });
    }
}
