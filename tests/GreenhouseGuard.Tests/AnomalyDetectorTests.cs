using FluentAssertions;
using GreenhouseGuard.Domain.Services;

namespace GreenhouseGuard.Tests;

public sealed class AnomalyDetectorTests
{
    private const string TemperatureSensor = "Temperature";
    private const int RequiredSampleSize = 3;
    private const decimal ZScoreThreshold = 2.5m;


    [Fact]
    public void Detect_WithTemperatureSpike_ShouldReturnAnomaly()
    {
        // Arrange: Setup stable historical data (e.g., normal greenhouse conditions)
        var historicalTemperatures = new List<decimal>
        {
            22.1m, // Normal morning
            22.3m, // Stable
            22.0m, // Consistent
            22.2m, // Normal
            22.1m // Stable
        };

        var anomalousReading = 35.8m; // Sudden spike (13.8°C increase)
        var detector = new AnomalyDetector();

        // Act: Detect anomaly with the spiked value
        var anomaly = detector.Detect(
            TemperatureSensor,
            anomalousReading,
            historicalTemperatures,
            RequiredSampleSize,
            ZScoreThreshold);

        // Assert: Verify anomaly was detected with correct properties
        anomaly.Should().NotBeNull("temperature spike should trigger anomaly detection");
        anomaly!.SensorType.Should().Be(TemperatureSensor);
        anomaly.Value.Should().Be(anomalousReading);
        anomaly.ZScore.Should().BeGreaterThan(ZScoreThreshold);
        anomaly.Reason.Should().Contain("Z-score").And.Contain("exceeded");
    }

    [Fact]
    public void Detect_WithNormalReading_ShouldReturnNull()
    {
        // Arrange: Setup historical data with natural variance across day/night
        var historicalTemperatures = new List<decimal>
        {
            20.0m, // Night minimum
            21.5m, // Early morning
            22.5m, // Midday
            23.5m, // Afternoon peak
            24.0m // Late afternoon
        };

        var normalReading = 22.8m; // Within observed range (Z-score < 2.5)
        var detector = new AnomalyDetector();

        // Act: Process normal reading
        var anomaly = detector.Detect(
            TemperatureSensor,
            normalReading,
            historicalTemperatures,
            RequiredSampleSize,
            ZScoreThreshold);

        // Assert: Verify no anomaly detected
        anomaly.Should().BeNull("reading within 1-2 standard deviations should be considered normal");
    }

    [Fact]
    public void Detect_WithInsufficientHistory_ShouldReturnNull()
    {
        // Arrange: Setup with only 1 historical reading (less than required 3)
        var historicalTemperatures = new List<decimal>
        {
            22.0m // Only 1 data point - not enough to calculate meaningful statistics
        };

        var newReading = 35.0m; // Even if spike seems large...
        var detector = new AnomalyDetector();

        // Act: Attempt anomaly detection with insufficient data
        var anomaly = detector.Detect(
            TemperatureSensor,
            newReading,
            historicalTemperatures,
            RequiredSampleSize,
            ZScoreThreshold);

        // Assert: Verify null returned due to insufficient sample size
        anomaly.Should().BeNull(
            "anomaly detection requires at least {0} historical samples to establish reliable baseline, but got {1}",
            RequiredSampleSize,
            historicalTemperatures.Count);
    }

    [Fact]
    public void Detect_WithZeroStandardDeviation_ShouldReturnNull()
    {
        // Arrange: All historical readings identical (sensor stuck or very stable)
        var historicalTemperatures = new List<decimal>
        {
            22.0m,
            22.0m,
            22.0m,
            22.0m,
            22.0m // Perfect consistency = 0 standard deviation
        };

        var newReading = 35.0m; // Even extreme value...
        var detector = new AnomalyDetector();

        // Act: Attempt detection when std dev = 0
        var anomaly = detector.Detect(
            TemperatureSensor,
            newReading,
            historicalTemperatures,
            RequiredSampleSize,
            ZScoreThreshold);

        // Assert: Verify null due to zero standard deviation
        anomaly.Should().BeNull(
            "Z-score calculation requires variance in historical data; " +
            "zero standard deviation indicates insufficient variability for meaningful anomaly detection");
    }
}