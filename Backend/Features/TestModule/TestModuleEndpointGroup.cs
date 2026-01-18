using FastEndpoints;
using PureTCOWebApp.Features;

namespace PureTCOWebApp.Features.TestModule;

public class TestModuleEndpointGroup : Group
{
    public TestModuleEndpointGroup()
    {
        Configure("test-items", ep =>
        {
            ep.Group<GlobalApiEndpointGroup>();
            ep.Tags("Test Module");
            ep.Description(e => e.WithTags("Test Module"));
        });
    }
}