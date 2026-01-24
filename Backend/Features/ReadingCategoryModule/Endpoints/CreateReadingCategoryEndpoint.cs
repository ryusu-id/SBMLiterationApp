using FastEndpoints;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.ReadingCategoryModule.Domain;

namespace PureTCOWebApp.Features.ReadingCategoryModule.Endpoints;

public class CreateReadingCategoryRequestValidator : AbstractValidator<CreateReadingCategoryRequest>
{
    public CreateReadingCategoryRequestValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category Name is required.")
            .MaximumLength(100).WithMessage("Category Name must not exceed 100 characters.");
    }
}

public record CreateReadingCategoryRequest(string CategoryName);

public record CreateReadingCategoryResponse(int Id, string CategoryName);

public class CreateReadingCategoryEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<CreateReadingCategoryRequest, ApiResponse<CreateReadingCategoryResponse>>
{
    public override void Configure()
    {
        Post("");
        Group<ReadingCategoryEndpointGroup>();
    }

    public override async Task HandleAsync(CreateReadingCategoryRequest req, CancellationToken ct)
    {
        if (await dbContext.ReadingCategories.AnyAsync(x => x.CategoryName == req.CategoryName, ct))
        {
            await Send.ResultAsync(TypedResults.Conflict<ApiResponse>((Result)CrudDomainError.Duplicate("ReadingCategory", "CategoryName")));
            return;
        }

        var category = ReadingCategory.Create(req.CategoryName);

        await dbContext.AddAsync(category, ct);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateReadingCategoryResponse(category.Id, category.CategoryName)
        ), cancellation: ct);
    }
}
