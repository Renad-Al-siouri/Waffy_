using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Waffy.Interfaces;
using Waffy.Models;

namespace Waffy.Services
{
    /// <summary>
    /// Background service — يعمل كل ساعة ويرسل تذكير 24 ساعة قبل الموعد
    /// التسجيل في Program.cs:
    ///   builder.Services.AddHostedService&lt;AppointmentReminderService&gt;();
    /// </summary>
    public class AppointmentReminderService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AppointmentReminderService> _logger;

        // الفاصل الزمني — كل ساعة
        private static readonly TimeSpan Interval = TimeSpan.FromHours(1);

        public AppointmentReminderService(
            IServiceScopeFactory scopeFactory,
            ILogger<AppointmentReminderService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppointmentReminderService started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await SendReminders(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AppointmentReminderService");
                }

                // انتظر ساعة قبل الجولة التالية
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private async Task SendReminders(CancellationToken ct)
        {
            // يجب استخدام Scope لأن الـ repositories هي Scoped services
            using var scope = _scopeFactory.CreateScope();
            var apptRepo = scope.ServiceProvider.GetRequiredService<IAppointmentRepository>();
            var notifRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

            // جلب مواعيد الغد التي لم يُرسل لها تذكير
            var upcoming = (await apptRepo.GetTomorrowConfirmedAsync()).ToList();

            if (!upcoming.Any())
            {
                _logger.LogDebug("No upcoming appointments to remind.");
                return;
            }

            _logger.LogInformation("Sending reminders for {Count} appointments.", upcoming.Count);

            foreach (var appt in upcoming)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    // إشعار لليوزر
                    await notifRepo.AddAsync(new Notification
                    {
                        IdUser = appt.IdUser,
                        Type = "AppointmentReminder",
                        Title = "⏰ Appointment Tomorrow!",
                        Message = $"Reminder: Your appointment at {appt.ClinicName} " +
                                            $"with Dr. {appt.DoctorName} is tomorrow " +
                                            $"({appt.AppointmentDate:dd MMM yyyy}) " +
                                            $"at {appt.StartTime:hh\\:mm}. " +
                                            $"Please arrive 10 minutes early. 🐾",
                        IsRead = false,
                        RelatedEntityType = "Appointment",
                        RelatedEntityId = appt.Id,
                        CreatedAt = DateTime.UtcNow
                    });

                    // علّم الموعد كـ "تم الإرسال"
                    await apptRepo.MarkReminderSentAsync(appt.Id);

                    _logger.LogInformation(
                        "Reminder sent for Appointment #{Id} to User #{UserId}",
                        appt.Id, appt.IdUser);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to send reminder for Appointment #{Id}", appt.Id);
                }
            }
        }
    }
}