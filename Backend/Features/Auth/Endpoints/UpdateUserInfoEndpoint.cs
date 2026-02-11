using FastEndpoints;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;
using PureTCOWebApp.Features.Auth.Domain;

namespace PureTCOWebApp.Features.Auth.Endpoints;

public record UpdateUserInfoRequest(
    string Fullname,
    string ProgramStudy,
    string Faculty,
    string Nim,
    string GenerationYear);

public class UpdateUserInfoRequestValidator : Validator<UpdateUserInfoRequest>
{
    public UpdateUserInfoRequestValidator()
    {
        RuleFor(x => x.Fullname)
            .MaximumLength(100)
            .WithMessage("Fullname must not exceed 100 characters")
            .NotEmpty()
            .WithMessage("Fullname is required");
        RuleFor(x => x.ProgramStudy)
            .MaximumLength(100)
            .WithMessage("Program Study must not exceed 100 characters")
            .NotEmpty()
            .WithMessage("Program Study is required");
        RuleFor(x => x.Faculty)
            .MaximumLength(100)
            .WithMessage("Faculty must not exceed 100 characters")
            .NotEmpty()
            .WithMessage("Faculty is required");
        RuleFor(x => x.Nim)
            .MaximumLength(50)
            .WithMessage("NIM must not exceed 50 characters")
            .NotEmpty()
            .WithMessage("NIM is required");
        RuleFor(x => x.GenerationYear)
            .NotEmpty()
            .WithMessage("Class is required");
    }
}

public class UpdateUserInfoEndpoint : Endpoint<UpdateUserInfoRequest, ApiResponse<User>>
{
    private readonly UserManager<User> _userManager;

    public UpdateUserInfoEndpoint(
        UserManager<User> userManager
    )
    {
        _userManager = userManager;
    }

    public override void Configure()
    {
        Put("auth/site");
        Group<GlobalApiEndpointGroup>();
    }

    public override async Task HandleAsync(UpdateUserInfoRequest req, CancellationToken ct)
    {
        var userId = User.FindFirst("sub")!.Value;
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        user.Fullname = req.Fullname;
        user.ProgramStudy = req.ProgramStudy;
        user.Faculty = req.Faculty;
        user.GenerationYear = req.GenerationYear;
        user.Nim = req.Nim;

        var result = await _userManager.UpdateAsync(user);

        if (!result.Succeeded)
        {
            await Send.ResultAsync(TypedResults.BadRequest<ApiResponse<User>>(Result.Failure<User>(new Error(result.Errors.First().Code, result.Errors.First().Description))));
            return;
        }

        await Send.OkAsync(Result.Success(user), ct);
    }
}
