using FastEndpoints;
using FluentValidation;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.AssignmentModule.Domain;

namespace PureTCOWebApp.Features.AssignmentModule.Endpoints.AssignmentEndpoints;

public class CreateAssignmentRequestValidator : AbstractValidator<CreateAssignmentRequest>
{
    public CreateAssignmentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters.");
    }
}

public record CreateAssignmentRequest(string Title, string? Description, DateTime? DueDate);

public record CreateAssignmentResponse(int Id, string Title, string? Description, DateTime? DueDate, DateTime CreateTime);

public class CreateAssignmentEndpoint(
    ApplicationDbContext dbContext,
    UnitOfWork unitOfWork
) : Endpoint<CreateAssignmentRequest, ApiResponse<CreateAssignmentResponse>>
{
    public override void Configure()
    {
        Post("");
        Group<AssignmentEndpointGroup>();
        Roles("admin");
        Validator<CreateAssignmentRequestValidator>();
    }

    public override async Task HandleAsync(CreateAssignmentRequest req, CancellationToken ct)
    {
        var assignment = Assignment.Create(req.Title, req.Description, req.DueDate);

        await dbContext.AddAsync(assignment, ct);
        var result = await unitOfWork.SaveChangesAsync(ct);

        if (result.IsFailure)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse>(result));
            return;
        }

        await Send.OkAsync(Result.Success(
            new CreateAssignmentResponse(assignment.Id, assignment.Title, assignment.Description, assignment.DueDate, assignment.CreateTime)
        ), cancellation: ct);
    }
}
