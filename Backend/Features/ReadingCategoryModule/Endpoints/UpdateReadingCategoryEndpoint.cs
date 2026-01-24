using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.ReadingCategoryModule.Endpoints;

public class UpdateReadingCategoryRequestValidator : AbstractValidator<UpdateReadingCategoryRequest>
{
    public UpdateReadingCategoryRequestValidator()
    {
        RuleFor(x => x.CategoryName)
            .NotEmpty().WithMessage("Category Name is required.")
            .MaximumLength(100).WithMessage("Category Name must not exceed 100 characters.");
    }
}

public record UpdateReadingCategoryRequest(int Id, string CategoryName);

public record UpdateReadingCategoryResponse(int Id, string CategoryName);

public class UpdateReadingCategoryEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UpdateReadingCategoryRequest, ApiResponse<UpdateReadingCategoryResponse>>
{
    public override void Configure()
    {
        Put("{id}");
        Group<ReadingCategoryEndpointGroup>();
    }

    public override async Task HandleAsync(UpdateReadingCategoryRequest req, CancellationToken ct)
    {
        var category = await dbContext.ReadingCategories.FindAsync([req.Id], ct);

        if (category is null)
        {
            var error = CrudDomainError.NotFound("ReadingCategory", req.Id);
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>((Result)error));
            return;
        }

        category.Update(req.CategoryName);

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new UpdateReadingCategoryResponse(category.Id, category.CategoryName)
        ), cancellation: ct);
    }
}
