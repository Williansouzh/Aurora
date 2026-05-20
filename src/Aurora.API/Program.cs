using Aurora.Application.Behaviors;
using FluentValidation;
using Aurora.API.Extensions;
using Aurora.API.Middlewares;
using Aurora.Infrastructure.Auth;
using Aurora.Infrastructure.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers(); builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<Aurora.Application.Interfaces.IUserContext, UserContext>();
builder.Services.AddMediatR(cfg=>cfg.RegisterServicesFromAssembly(typeof(Aurora.Application.Features.Auth.RegisterUserCommand).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(Aurora.Application.Features.Auth.RegisterUserCommand).Assembly);
builder.Services.AddTransient(typeof(MediatR.IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
builder.Services.AddInfrastructure(builder.Configuration);
var jwt = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32) throw new InvalidOperationException("Jwt:Key must have at least 32 characters.");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(o=>o.TokenValidationParameters=new TokenValidationParameters{ValidateIssuer=true,ValidateAudience=true,ValidateLifetime=true,ValidateIssuerSigningKey=true,ValidIssuer=jwt.Issuer,ValidAudience=jwt.Audience,IssuerSigningKey=new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key))});
builder.Services.AddAuthorization();
var frontUrl = builder.Configuration["Cors:FrontendUrl"] ?? "http://localhost:5173";
builder.Services.AddCors(o=>o.AddPolicy("front",p=>p.WithOrigins(frontUrl).AllowAnyHeader().AllowAnyMethod()));
builder.Services.AddEndpointsApiExplorer(); builder.Services.AddSwaggerGen(c=>{c.AddSecurityDefinition("Bearer",new(){Name="Authorization",Type=Microsoft.OpenApi.Models.SecuritySchemeType.Http,Scheme="bearer",BearerFormat="JWT",In=Microsoft.OpenApi.Models.ParameterLocation.Header}); c.AddSecurityRequirement(new(){ {new(){Reference=new(){Type=Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,Id="Bearer"}},Array.Empty<string>() }});});
var app = builder.Build();

if (app.Environment.IsProduction()) app.UseHsts();
app.Use(async (ctx,next)=>{
 ctx.Response.Headers.TryAdd("X-Content-Type-Options","nosniff");
 ctx.Response.Headers.TryAdd("X-Frame-Options","DENY");
 ctx.Response.Headers.TryAdd("Referrer-Policy","no-referrer");
 await next();
});
app.UseMiddleware<GlobalExceptionMiddleware>(); app.UseCors("front"); app.UseSwagger(); app.UseSwaggerUI(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
using(var scope=app.Services.CreateScope()) await scope.ServiceProvider.EnsureIndexesAsync();
app.Run();
