using Application.IRepositories;
using Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Infrastructure.BackgroundJobs;

/// <summary>
/// Background service kiểm tra các phiên thi quá hạn (ExpiresAt <= UtcNow)
/// và tự động chuyển trạng thái sang TimedOut
/// </summary>
public class ExamSessionTimeoutService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ExamSessionTimeoutService> _logger;

    // Kiểm tra mỗi 60 giây
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(60);

    public ExamSessionTimeoutService(
        IServiceScopeFactory scopeFactory,
        ILogger<ExamSessionTimeoutService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("ExamSessionTimeoutService đã khởi động");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessExpiredSessionsAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi xử lý phiên thi quá hạn");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }

        _logger.LogInformation("ExamSessionTimeoutService đã dừng");
    }

    private async Task ProcessExpiredSessionsAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var expiredSessions = await unitOfWork.ExamSessions.GetExpiredSessionsAsync();

        if (expiredSessions.Count == 0)
            return;

        _logger.LogInformation(
            "Tìm thấy {Count} phiên thi quá hạn cần xử lý",
            expiredSessions.Count);

        foreach (var session in expiredSessions)
        {
            session.Status = ExamSessionStatus.TimedOut;
            session.UpdatedAt = DateTime.UtcNow;
            unitOfWork.ExamSessions.UpdateAsync(session);
        }

        await unitOfWork.SaveChangesAsync();

        _logger.LogInformation(
            "Đã chuyển {Count} phiên thi sang trạng thái TimedOut",
            expiredSessions.Count);
    }
}
