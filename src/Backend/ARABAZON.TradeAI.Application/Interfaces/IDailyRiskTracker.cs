namespace ARABAZON.TradeAI.Application.Interfaces;

public interface IDailyRiskTracker
{
    decimal DailyLoss { get; }
    decimal DailyProfit { get; }
    bool IsEmergencyStop { get; }
    void RecordTrade(decimal profitLoss);
    void Reset();
    void TriggerEmergencyStop(string reason);
    void ClearEmergencyStop();
}