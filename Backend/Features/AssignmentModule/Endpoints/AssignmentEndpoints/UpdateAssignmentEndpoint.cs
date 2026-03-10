using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.AssignmentEndpoints;

public class UpdateAssignmentRequestValidator : AbstractValidator<UpdateAssignmentRequest>
{
    public UpdateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
    }
}

public record UpdateAssignmentRequest(int Id, string Title, string? Description, DateTime? DueDate);

public record UpdateAssignmentResponse(int Id, string Title, string? Description, DateTime? DueDate, DateTime? UpdateTime);

public class UpdateAssignmentEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<UpdateAssignmentRequest, ApiResponse<UpdateAssignmentResponse>>
{
    public override void Configure()
    {
        Put("{id}");
        Group<AssignmentEndpointGroup>();
        Roles("admin");
        Validator<UpdateAssignmentRequestValidator>();
    }

    public override async Task HandleAsync(UpdateAssignmentRequest req, CancellationToken ct)
    {
        var assignment = await dbContext.Assignments.FindAsync([req.Id], ct);

        if (assignment is null)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(
                (Result)CrudDomainError.NotFound("Assignment", req.Id)));
            return;
        }

        assignment.Update(req.Title, req.Description, req.DueDate);

        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new UpdateAssignmentResponse(assignment.Id, assignment.Title, assignment.Description, assignment.DueDate, assignment.UpdateTime)
        ), cancellation: ct);
    }
}
