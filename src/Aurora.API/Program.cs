using System.Text;
using Aurora.API.Extensions;
using Aurora.API.Health;
using Aurora.API.Middlewares;
using Aurora.Application.Abstractions.Common;
using Aurora.Application.Behaviors;
using Aurora.Application.Features.Auth.Register;
using Aurora.Infrastructure.DependencyInjection;
using Aurora.Infrastructure.Security;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Application", "Aurora")
    .WriteTo.Console());

builder.Services.AddControllers(options =>
{
    options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
});
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUserContext, UserContext>();

builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(RegisterUserCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(RegisterUserCommand).Assembly);
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddTransient(typeof(IPipelineBehavior<,>), typeof(UnitOfWorkBehavior<,>));

builder.Services.AddInfrastructure(builder.Configuration);

var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
var jwtKeys = jwt.Keys.Count > 0 ? jwt.Keys : [new JwtSigningKey { KeyId = jwt.CurrentKeyId, Key = jwt.Key }];
if (jwtKeys.Any(x => string.IsNullOrWhiteSpace(x.Key) || x.Key.Length < 32))
{
    throw new InvalidOperationException("All JWT signing keys must have at least 32 characters.");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwt.Issuer,
        ValidAudience = jwt.Audience,
        IssuerSigningKeyResolver = (token, securityToken, kid, validationParameters) =>
            jwtKeys
                .Where(x => string.IsNullOrWhiteSpace(kid) || x.KeyId == kid)
                .Select(x => new SymmetricSecurityKey(Encoding.UTF8.GetBytes(x.Key)) { KeyId = x.KeyId })
    });
builder.Services.AddAuthorization();
builder.Services.AddHealthChecks()
    .AddCheck<MongoHealthCheck>("mongo")
    .AddCheck<RedisHealthCheck>("redis");
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddFixedWindowLimiter("global", limiter =>
    {
        limiter.PermitLimit = 120;
        limiter.Window = TimeSpan.FromMinutes(1);
        limiter.QueueLimit = 0;
    });
});

var frontUrl = builder.Configuration["Cors:FrontendUrl"] ?? "http://localhost:5173";
var extraOrigins = (builder.Configuration["Cors:ExtraOrigins"] ?? "")
    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
var allOrigins = new[] { frontUrl }.Concat(extraOrigins).ToArray();
builder.Services.AddCors(o => o.AddPolicy(
    "front",
    p => p.WithOrigins(allOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("Bearer", new()
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header
    });
    c.AddSecurityRequirement(new()
    {
        {
            new()
            {
                Reference = new()
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("CorrelationId", httpContext.Response.Headers[CorrelationIdMiddleware.HeaderName].ToString());
        diagnosticContext.Set("UserId", httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value);
    };
});

if (app.Environment.IsProduction()) app.UseHsts();

app.Use(async (ctx, next) =>
{
    ctx.Response.Headers.TryAdd("X-Content-Type-Options", "nosniff");
    ctx.Response.Headers.TryAdd("X-Frame-Options", "DENY");
    ctx.Response.Headers.TryAdd("Referrer-Policy", "no-referrer");
    await next();
});

app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseCors("front");
app.UseRateLimiter();
app.UseSwagger();
app.UseSwaggerUI();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapHealthChecks("/health");
app.MapHealthChecks("/health/ready");
app.MapControllers().RequireRateLimiting("global");

using (var scope = app.Services.CreateScope())
{
    await scope.ServiceProvider.EnsureIndexesAsync();
    await scope.ServiceProvider.SeedAccessControlAsync(app.Configuration);
}

app.Run();
