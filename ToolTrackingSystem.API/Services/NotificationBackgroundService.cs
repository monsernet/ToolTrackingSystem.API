using ToolTrackingSystem.API.Models.Entities;
using ToolTrackingSystem.API.Repositories;
using ToolTrackingSystem.API.Services;

public class NotificationBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationBackgroundService> _logger;

    public NotificationBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var issuanceRepo = scope.ServiceProvider.GetRequiredService<IIssuanceRepository>();
                var calibrationService = scope.ServiceProvider.GetRequiredService<ICalibrationService>();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var notificationRepo = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

                try
                {
                    // Check overdue tool issuances
                    var overdueIssuances = await issuanceRepo.GetOverdueIssuancesAsync();
                    foreach (var issuance in overdueIssuances)
                    {
                        // Send email
                        await emailService.SendEmailAsync(
                            to: issuance.IssuedTo.Email,
                            subject: "🔴 Overdue Tool Alert",
                            body: $"Tool {issuance.Tool.Name} is overdue!"
                        );

                        // Create in-app notification
                        await notificationRepo.AddAsync(new Notification
                        {
                            Message = $"Tool {issuance.Tool.Name} is overdue!",
                            RecipientUserId = issuance.IssuedTo.Id
                        });
                    }

                    // Check overdue calibrations
                    var overdueCalibrations = await calibrationService.GetOverdueCalibrationsAsync();
                    foreach (var calibration in overdueCalibrations)
                    {
                        await emailService.SendEmailAsync(
                            to: "maintenance@company.com",
                            subject: "⚠️ Calibration Due",
                            body: $"Tool {calibration.ToolName} needs calibration!"
                        );

                        await notificationRepo.AddAsync(new Notification
                        {
                            Message = $"Tool {calibration.ToolName} needs calibration",
                            RecipientUserId = 1 // Maintenance team ID
                        });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error checking overdue items");
                }
            }

            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
}