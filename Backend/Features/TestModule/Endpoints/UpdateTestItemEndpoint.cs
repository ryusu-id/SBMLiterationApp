using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.TestModule.Endpoints;

public class UpdateTestItemRequestValidator : AbstractValidator<UpdateTestItemRequest>
{
    public UpdateTestItemRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must not exceed 500 characters.")
            .When(x => !string.IsNullOrEmpty(x.Description));
    }
}

public record UpdateTestItemRequest(
    int Id,
    string Name,
    string? Description,
    decimal Price
);

public record UpdateTestItemResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive
);

public class UpdateTestItemEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UpdateTestItemRequest, ApiResponse<UpdateTestItemResponse>>
{
    public override void Configure()
    {
        Put("{id}");
        Group<TestModuleEndpointGroup>();
    }

    public override async Task HandleAsync(UpdateTestItemRequest req, CancellationToken ct)
    {
        var testItem = await dbContext.TestItems.FindAsync([req.Id], ct);

        if (testItem is null)
        {
            var error = CrudDomainError.NotFound(nameof(TestItem), req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        testItem.Update(req.Name, req.Description, req.Price);

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        var response = new UpdateTestItemResponse(
            testItem.Id,
            testItem.Name,
            testItem.Description,
            testItem.Price,
            testItem.IsActive
        );

        await Send.OkAsync(Result.Success(response), cancellation: ct);
    }
}