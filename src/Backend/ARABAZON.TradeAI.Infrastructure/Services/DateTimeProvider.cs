using ARABAZON.TradeAI.Application.Interfaces;

namespace ARABAZON.TradeAI.Infrastructure.Services;

public class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}