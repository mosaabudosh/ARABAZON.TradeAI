namespace ARABAZON.TradeAI.Domain.Exceptions;

public class RiskViolationException : DomainException
{
    public RiskViolationException(string message)
        : base("RISK_VIOLATION", message) { }
}