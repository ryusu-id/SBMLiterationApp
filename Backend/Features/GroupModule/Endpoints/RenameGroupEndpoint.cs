using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public class RenameGroupRequestValidator : AbstractValidator<RenameGroupRequest>
{
    public RenameGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");
    }
}

public record RenameGroupRequest(int Id, string Name);

public record RenameGroupResponse(int Id, string Name, DateTime? UpdateTime);

public class RenameGroupEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<RenameGroupRequest, ApiResponse<RenameGroupResponse>>
{
    public override void Configure()
    {
        Put("{id}");
        Group<GroupEndpointGroup>();
        Roles("admin");
        Validator<RenameGroupRequestValidator>();
    }

    public override async Task HandleAsync(RenameGroupRequest req, CancellationToken ct)
    {
        var group = await dbContext.Groups.FindAsync([req.Id], ct);

        if (group is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Group", req.Id)));
            return;
        }

        group.Rename(req.Name);

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new RenameGroupResponse(group.Id, group.Name, group.UpdateTime)
        ), cancellation: ct);
    }
}
