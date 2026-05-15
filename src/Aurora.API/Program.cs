using Aurora.API.Middlewares;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();builder.Services.AddSwaggerGen();
var app = builder.Build();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseSwagger();app.UseSwaggerUI();
app.MapControllers();
app.Run();
