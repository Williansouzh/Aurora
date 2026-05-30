using Aurora.Application.Abstractions.Messaging;
using Aurora.Application.Abstractions.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Notifications;

public class WeeklyPlanningReminderService(IServiceScopeFactory scopeFactory, ILogger<WeeklyPlanningReminderService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                if (DateTime.UtcNow.DayOfWeek == DayOfWeek.Monday)
                    await CheckAndSendAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar lembretes de planejamento semanal");
            }

            await Task.Delay(TimeSpan.FromHours(1), ct);
        }
    }

    private async Task CheckAndSendAsync(CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        using var scope = scopeFactory.CreateScope();
        var userRepo    = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var weeklyRepo  = scope.ServiceProvider.GetRequiredService<IWeeklyPlanRepository>();
        var emailSender = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var users = await userRepo.GetAllAsync();

        foreach (var user in users.Where(u =>
            u.Notifications.WeeklyPlanningReminderEnabled &&
            nowUtc.Hour == u.Notifications.WeeklyPlanningReminderHour &&
            (u.LastWeeklyReminderSentAt is null || u.LastWeeklyReminderSentAt.Value.Date < nowUtc.Date)))
        {
            var currentPlan = await weeklyRepo.GetCurrentAsync(user.Id);
            if (currentPlan is not null) continue;

            await emailSender.SendAsync(
                user.Email,
                "Planeje sua semana no Aurora",
                $"Olá {user.Name},\n\nÉ segunda-feira! Que tal planejar sua semana no Aurora?\n\nDefina seu foco, prioridades e objetivos para a semana.\n\n— Aurora Life OS");

            user.LastWeeklyReminderSentAt = nowUtc;
            await userRepo.UpdateAsync(user);

            logger.LogInformation("Lembrete semanal enviado para {UserId}", user.Id);
        }
    }
}
