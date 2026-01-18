using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.TestModule.Endpoints;

public class CreateTestItemRequestValidator : AbstractValidator<CreateTestItemRequest>
{
    public CreateTestItemRequestValidator()
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

public record CreateTestItemRequest(
    string Name,
    string? Description,
    decimal Price
);

public record CreateTestItemResponse(
    int Id,
    string Name,
    string? Description,
    decimal Price,
    bool IsActive
);

public class CreateTestItemEndpoint(
    ApplicationDbContext _dbContext,
    UnitOfWork _unitOfWork
) : Endpoint<CreateTestItemRequest, ApiResponse<CreateTestItemResponse>>
{
    public override void Configure()
    {
        Post("");
        Group<TestModuleEndpointGroup>();
    }

    public override async Task HandleAsync(CreateTestItemRequest req, CancellationToken ct)
    {
        if (await _dbContext.TestItems.AnyAsync(x => x.Name == req.Name, ct))
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>((Result)CrudDomainError.Duplicate("TestItem", "Name")));
            return;
        }

        var testItem = TestItem.Create(
            req.Name,
            req.Description,
            req.Price
        );

        await _dbContext.AddAsync(testItem, ct);
        var result = await _unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateTestItemResponse(
                testItem.Id,
                testItem.Name,
                testItem.Description,
                testItem.Price,
                testItem.IsActive
            )
        ), cancellation: ct);
    }
}