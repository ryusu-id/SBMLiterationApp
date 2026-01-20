using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.ReadingResourceModule.Endpoints;

public record DeleteReadingResourceRequest(int Id);

public class DeleteReadingResourceEndpoint(
    ApplicationDbContext _dbContext,
    UnitOfWork _unitOfWork
) : Endpoint<DeleteReadingResourceRequest, ApiResponse>
{
    public override void Configure()
    {
        Delete("{id}");
        Group<ReadingResourceEndpointGroup>();
    }

    public override async Task HandleAsync(DeleteReadingResourceRequest req, CancellationToken ct)
    {
        var userId = int.Parse(User.FindFirst("sub")!.Value);
        
        // Try to find as Book first
        var book = await _dbContext.Books.FindAsync([req.Id], ct);
        if (book != null)
        {
            if (book.UserId != userId)
            {
                await Send.ForbiddenAsync(ct);
                return;
            }
            
            _dbContext.Remove(book);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            if (result.IsFailure)
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
                return;
            }

            await Send.OkAsync(Result.Success(), cancellation: ct);
            return;
        }

        // Try to find as JournalPaper
        var journal = await _dbContext.JournalPapers.FindAsync([req.Id], ct);
        if (journal != null)
        {
            if (journal.UserId != userId)
            {
                await Send.ForbiddenAsync(ct);
                return;
            }
            
            _dbContext.Remove(journal);
            var result = await _unitOfWork.SaveChangesAsync(ct);

            if (result.IsFailure)
            {
                await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
                return;
            }

            await Send.OkAsync(Result.Success(), cancellation: ct);
            return;
        }

        // Not found
        var error = CrudDomainError.NotFound("ReadingResource", req.Id);
        await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
    }
}