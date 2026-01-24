using FastEndpoints;

namespace PureTCOWebApp.Features.FileSystem;

public class FileSystemEndpointGroup : Group
{
    public FileSystemEndpointGroup()
    {
        Configure("/api/files", ep =>
        {
            ep.Description(x => x
                .WithTags("FileSystem"));
        });
    }
}
