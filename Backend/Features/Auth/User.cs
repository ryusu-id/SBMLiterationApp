using Microsoft.AspNetCore.Identity;

namespace PureTCOWebApp.Features.Auth;

public class User : IdentityUser<int>
{
    #pragma warning disable 
    public User()
    {
    }
    #pragma warning restore
    public string Nim { get; set; }
    public string ProgramStudy { get; set; }
}