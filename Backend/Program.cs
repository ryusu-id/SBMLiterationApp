using FastEndpoints;
using FastEndpoints.Security;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Minio;
using PureTCOWebApp.Data;
using PureTCOWebApp.Features.Auth;
using PureTCOWebApp.Features.Auth.Domain;
using PureTCOWebApp.Features.FileSystem;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Add Entity Framework
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services
    .AddIdentity<User, IdentityRole<int>>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// Configure Identity to return status codes instead of redirecting
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

// Add UnitOfWork
builder.Services.AddScoped<UnitOfWork>();

// Add JWT Token Service
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();

// Configure MinIO
builder.Services.Configure<MinIOSettings>(builder.Configuration.GetSection("MinIO"));
var minioSettings = builder.Configuration.GetSection("MinIO").Get<MinIOSettings>();
if (minioSettings != null)
{
    builder.Services.AddSingleton<IMinioClient>(sp =>
    {
        var client = new MinioClient()
            .WithEndpoint(minioSettings.Endpoint)
            .WithCredentials(minioSettings.AccessKey, minioSettings.SecretKey);

        if (minioSettings.UseSSL)
        {
            client = client.WithSSL();
        }

        return client.Build();
    });

    builder.Services.AddScoped<IMinIOService, MinIOService>();
}

// Add HttpClient factory for OAuth
builder.Services.AddHttpClient();

// Add Google Books Service
builder.Services.AddHttpClient<PureTCOWebApp.Features.TestModule.GoogleBook.GoogleBooksService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["GoogleBooks:BaseUrl"] ?? "https://www.googleapis.com/books/v1/");
});

// Add CrossRef Service
builder.Services.AddHttpClient<PureTCOWebApp.Features.TestModule.JournalDoi.CrossRefService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["CrossRef:BaseUrl"] ?? "https://api.crossref.org/");
});

// Add session support for OAuth state management
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(10);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services
    .AddAuthenticationJwtBearer(s => s.SigningKey = builder.Configuration["Jwt:SecretKey"])
    .AddFastEndpoints()
    .SwaggerDocument();

builder.Services.AddAuthorization();

var googleClientId = builder.Configuration["GoogleAuthCredentials:ClientId"];
var googleClientSecret = builder.Configuration["GoogleAuthCredentials:ClientSecret"];
if (string.IsNullOrEmpty(googleClientId) || string.IsNullOrEmpty(googleClientSecret))
{
    throw new Exception("Google Auth Credentials are not set in configuration.");
}

builder.Services
    .AddAuthentication(o =>
    {
        o.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddGoogle(options =>
    {
        options.ClientId = googleClientId;
        options.ClientSecret = googleClientSecret;
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.

if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseCors();
}

app.UseSession();

app.UseAuthentication();
app.UseAuthorization();
app.UseFastEndpoints();

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerGen();
}

{
    await using var scope = app.Services.CreateAsyncScope();
    await scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database.MigrateAsync();
}

// Health check endpoint for Kubernetes liveness/readiness probes
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
    .AllowAnonymous();

app.Run();