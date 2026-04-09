using Murder.ApplicationGame;
using Murder.ApplicationIdentity;
using Murder.DomainGame;
using Murder.DomainIdentity;
using Murder.Plugins.AuthenticationMethod.Password;
using Murder.Plugins.AuthenticationMethod.SessionToken;
using Murder.Plugins.CredentialRepository.InMemory;
using Murder.Plugins.GameRepositoy.InMemory;
using Murder.Plugins.IdentityRepository.InMemory;
using Murder.Plugins.WebAPI;
using Murder.Plugins.WebAPI.Authentication;
using Murder.Plugins.WebAPI.Watch;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddControllers();
var corsOrigins = builder.Configuration.GetValue<string>("CORS_ORIGINS")?
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
    ?? ["http://localhost:5173", "http://127.0.0.1:5173"];

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddSingleton<IIdentityRepository, InMemoryIdentityRepository>();
builder.Services.AddSingleton<ICredentialRepository, InMemoryCredentialRepository>();
builder.Services.AddSingleton<IGameRepository, InMemoryGameRepository>();

builder.Services.AddSingleton<IdentityService>();
builder.Services.AddSingleton<AuthenticationService>();
builder.Services.AddSingleton<GameService>();
builder.Services.AddSingleton<GameEventBus>();
builder.Services.AddSingleton<PendingKillStore>();

builder.Services.AddSingleton<PasswordAuthenticationMethod>();
builder.Services.AddSingleton<SessionTokenAuthenticationMethod>();
builder.Services.AddSingleton(serviceProvider =>
{
    var passwordMethod = serviceProvider.GetRequiredService<PasswordAuthenticationMethod>();
    var sessionTokenMethod = serviceProvider.GetRequiredService<SessionTokenAuthenticationMethod>();

    return new AuthenticatorBuilder()
        .Register(
            passwordMethod
        )
        .Register(
            sessionTokenMethod
        )
        .Build();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseMiddleware<ProblemDetailsExceptionMiddleware>();
app.UseCors("Frontend");

app.Use(async (context, next) =>
{
    var sessionToken = context.Request.Cookies[AuthenticationCookieNames.SessionToken];

    if (!string.IsNullOrWhiteSpace(sessionToken))
    {
        var authenticationService = context.RequestServices.GetRequiredService<AuthenticationService>();

        try
        {
            var identityId = authenticationService.Authenticate<SessionTokenMethodKey>(
                new SessionTokenIncomingCredential(sessionToken)
            );

            if (identityId is not null)
            {
                context.Items[AuthenticationHttpContextItems.CurrentIdentityId] = identityId.Value;
                SessionCookie.Append(context.Response, context.Request, sessionToken);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Ignore auth failures and continue pipeline.
        }
        catch (InvalidOperationException)
        {
            // Ignore auth failures and continue pipeline.
        }
        catch (Exception ex) when (ex is not OutOfMemoryException and not StackOverflowException)
        {
            // Ignore unexpected auth parsing errors and continue pipeline.
        }
    }

    await next();
});

app.MapControllers();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapFallbackToFile("index.html");

app.Run();
