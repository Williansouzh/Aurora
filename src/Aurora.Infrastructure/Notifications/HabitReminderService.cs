using Aurora.Application.Abstractions.Messaging;
using Aurora.Application.Abstractions.Persistence;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Aurora.Infrastructure.Notifications;

public class HabitReminderService(IServiceScopeFactory scopeFactory, ILogger<HabitReminderService> logger)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                await CheckAndSendRemindersAsync(ct);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro ao processar lembretes de hábitos");
            }

            await Task.Delay(TimeSpan.FromHours(1), ct);
        }
    }

    private async Task CheckAndSendRemindersAsync(CancellationToken ct)
    {
        var nowUtc = DateTime.UtcNow;
        using var scope = scopeFactory.CreateScope();
        var userRepo       = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var habitRepo      = scope.ServiceProvider.GetRequiredService<IHabitRepository>();
        var checkInRepo    = scope.ServiceProvider.GetRequiredService<IHabitCheckInRepository>();
        var emailSender    = scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var users = await userRepo.GetAllAsync();

        foreach (var user in users.Where(u =>
            u.Notifications.HabitReminderEnabled &&
            nowUtc.Hour == u.Notifications.HabitReminderHour &&
            (u.LastHabitReminderSentAt is null || u.LastHabitReminderSentAt.Value.Date < nowUtc.Date)))
        {
            var habits   = await habitRepo.GetActiveAsync(user.Id);
            var checkIns = await checkInRepo.GetByUserAndDateAsync(user.Id, nowUtc.Date);
            var doneIds  = checkIns.Select(c => c.HabitId).ToHashSet();
            var pending  = habits.Where(h => !doneIds.Contains(h.Id)).ToList();

            if (pending.Count == 0) continue;

            var list = string.Join("\n", pending.Take(5).Select(h => $"  • {h.Name}"));
            await emailSender.SendAsync(
                user.Email,
                "Lembrete Aurora — Hábitos pendentes",
                $"Olá {user.Name},\n\nVocê ainda tem {pending.Count} hábito(s) pendente(s) hoje:\n\n{list}\n\nAbra o Aurora para fazer o check-in!\n\n— Aurora Life OS");

            user.LastHabitReminderSentAt = nowUtc;
            await userRepo.UpdateAsync(user);

            logger.LogInformation("Lembrete de hábito enviado para {UserId}", user.Id);
        }
    }
}
