using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.GroupModule.Domain;

namespace PureTCOWebApp.Features.GroupModule.Endpoints;

public class CreateGroupRequestValidator : AbstractValidator<CreateGroupRequest>
{
    public CreateGroupRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(150).WithMessage("Name must not exceed 150 characters.");
    }
}

public record CreateGroupRequest(string Name, string? Description);

public record CreateGroupResponse(int Id, string Name, string? Description, DateTime CreateTime);

public class CreateGroupEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<CreateGroupRequest, ApiResponse<CreateGroupResponse>>
{
    public override void Configure()
    {
        Post("");
        Group<GroupEndpointGroup>();
        Roles("admin");
        Validator<CreateGroupRequestValidator>();
    }

    public override async Task HandleAsync(CreateGroupRequest req, CancellationToken ct)
    {
        var group = Domain.Group.Create(req.Name, req.Description);

        await dbContext.AddAsync(group, ct);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateGroupResponse(group.Id, group.Name, group.Description, group.CreateTime)
        ), cancellation: ct);
    }
}
