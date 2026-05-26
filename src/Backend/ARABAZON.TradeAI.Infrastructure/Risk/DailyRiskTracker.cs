using ARABAZON.TradeAI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace ARABAZON.TradeAI.Infrastructure.Risk;

public class DailyRiskTracker : IDailyRiskTracker
{
    private readonly ILogger<DailyRiskTracker> _logger;
    private readonly object _lock = new();

    private decimal _dailyLoss;
    private decimal _dailyProfit;
    private bool _emergencyStop;
    private string? _emergencyReason;
    private DateTime _lastResetDate;

    public decimal DailyLoss => _dailyLoss;
    public decimal DailyProfit => _dailyProfit;
    public bool IsEmergencyStop => _emergencyStop;

    public DailyRiskTracker(ILogger<DailyRiskTracker> logger)
    {
        _logger = logger;
        _lastResetDate = DateTime.UtcNow.Date;
    }

    public void RecordTrade(decimal profitLoss)
    {
        lock (_lock)
        {
            EnsureDailyReset();

            if (profitLoss < 0)
                _dailyLoss += Math.Abs(profitLoss);
            else
                _dailyProfit += profitLoss;

            _logger.LogInformation(
                "Trade recorded: PnL={PnL} | DailyLoss={Loss} DailyProfit={Profit}",
                profitLoss, _dailyLoss, _dailyProfit);
        }
    }

    public void Reset()
    {
        lock (_lock)
        {
            _dailyLoss = 0;
            _dailyProfit = 0;
            _lastResetDate = DateTime.UtcNow.Date;
            _logger.LogInformation("Daily risk tracker reset");
        }
    }

    public void TriggerEmergencyStop(string reason)
    {
        lock (_lock)
        {
            _emergencyStop = true;
            _emergencyReason = reason;
            _logger.LogCritical("EMERGENCY STOP triggered: {Reason}", reason);
        }
    }

    public void ClearEmergencyStop()
    {
        lock (_lock)
        {
            _emergencyStop = false;
            _emergencyReason = null;
            _logger.LogWarning("Emergency stop cleared");
        }
    }

    private void EnsureDailyReset()
    {
        if (DateTime.UtcNow.Date > _lastResetDate)
        {
            _logger.LogInformation("New trading day — resetting daily risk tracker");
            _dailyLoss = 0;
            _dailyProfit = 0;
            _lastResetDate = DateTime.UtcNow.Date;
        }
    }
}