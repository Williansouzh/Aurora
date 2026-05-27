using Aurora.Application.Abstractions.Common;

namespace Aurora.Infrastructure.Time;

public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
