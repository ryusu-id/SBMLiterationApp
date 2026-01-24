using FastEndpoints;

namespace PureTCOWebApp.Features.ReadingRecommendationModule;

public class ReadingRecommendationEndpointGroup : Group
{
    public ReadingRecommendationEndpointGroup()
    {
        Configure("reading-recommendations", ep =>
        {
            ep.Description(x => x.WithTags("Reading Recommendations"));
            ep.Tags("Reading Recommendation Module");
        });
    }
}
