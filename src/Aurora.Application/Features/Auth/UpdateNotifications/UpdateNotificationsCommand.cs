using Aurora.Application.Abstractions.Persistence;
using Aurora.Domain.Entities;
using Aurora.Domain.Exceptions;
using MediatR;

namespace Aurora.Application.Features.Auth.UpdateNotifications;

public record UpdateNotificationsCommand(
    string UserId,
    bool HabitReminderEnabled,
    int HabitReminderHour,
    bool WeeklyPlanningReminderEnabled,
    int WeeklyPlanningReminderHour) : IRequest<NotificationPreferences>;

public class UpdateNotificationsHandler(IUserRepository repo)
    : IRequestHandler<UpdateNotificationsCommand, NotificationPreferences>
{
    public async Task<NotificationPreferences> Handle(UpdateNotificationsCommand cmd, CancellationToken ct)
    {
        var user = await repo.GetByIdAsync(cmd.UserId)
            ?? throw new NotFoundException("Usuário não encontrado.");

        user.Notifications = new NotificationPreferences
        {
            HabitReminderEnabled = cmd.HabitReminderEnabled,
            HabitReminderHour = Math.Clamp(cmd.HabitReminderHour, 0, 23),
            WeeklyPlanningReminderEnabled = cmd.WeeklyPlanningReminderEnabled,
            WeeklyPlanningReminderHour = Math.Clamp(cmd.WeeklyPlanningReminderHour, 0, 23),
        };

        await repo.UpdateAsync(user);
        return user.Notifications;
    }
}
