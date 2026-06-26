namespace GreenhouseGuard.Application.Abstractions.Services;

public interface ISensorValuesHistory
{
    void AddValue(string sensorType, decimal value);

    IReadOnlyCollection<decimal> GetValues(string sensorType);
}
