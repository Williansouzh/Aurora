using System.Net;
namespace Aurora.API.Middlewares;
public class GlobalExceptionMiddleware(RequestDelegate next){public async Task Invoke(HttpContext context){try{await next(context);}catch(Exception ex){context.Response.StatusCode=(int)HttpStatusCode.BadRequest;await context.Response.WriteAsJsonAsync(new{message=ex.Message,traceId=context.TraceIdentifier});}}}
