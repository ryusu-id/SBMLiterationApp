using Microsoft.AspNetCore.Identity;

namespace PureTCOWebApp.Features.Auth.Domain;

public class User : IdentityUser<int>
{
    #pragma warning disable 
    public User()
    {
    }
    #pragma warning restore
    public string Nim { get; set; }
    public string Fullname { get; set; }
    public string ProgramStudy { get; set; }
    public string Faculty { get; set; }
    public string GenerationYear { get; set; }
    public string? PictureUrl { get; set; }
}