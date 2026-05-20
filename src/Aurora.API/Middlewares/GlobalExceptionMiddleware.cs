using Aurora.Application.Common;
using Aurora.Domain.Exceptions;

namespace Aurora.API.Middlewares;
public class GlobalExceptionMiddleware(RequestDelegate next){
 public async Task Invoke(HttpContext context){
  try{ await next(context); }
  catch(ValidationException ex){ context.Response.StatusCode=400; await context.Response.WriteAsJsonAsync(new ErrorResponse(ex.Message,context.TraceIdentifier,ex.Errors)); }
  catch(ConflictException ex){ context.Response.StatusCode=409; await context.Response.WriteAsJsonAsync(new ErrorResponse(ex.Message,context.TraceIdentifier)); }
  catch(NotFoundException ex){ context.Response.StatusCode=404; await context.Response.WriteAsJsonAsync(new ErrorResponse(ex.Message,context.TraceIdentifier)); }
  catch(Exception ex){ context.Response.StatusCode=500; await context.Response.WriteAsJsonAsync(new ErrorResponse(ex.Message,context.TraceIdentifier)); }
 }
}
