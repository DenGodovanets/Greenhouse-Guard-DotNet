using FluentAssertions;
using GreenhouseGuard.Application.Configuration;
using GreenhouseGuard.Infrastructure.Services;
using Microsoft.Extensions.Options;

namespace GreenhouseGuard.Tests;

public sealed class InMemorySensorValuesHistoryTests
{
    private const string TemperatureSensor = "Temperature";
    private const string HumiditySensor = "Humidity";
    private const int WindowSize = 5;

    private InMemorySensorValuesHistory CreateHistoryWithWindowSize(int size)
    {
        var options = Options.Create(new SensorMonitoringOptions { WindowSize = size });
        return new InMemorySensorValuesHistory(options);
    }

    [Fact]
    public void AddValue_WithMultipleReadings_ShouldMaintainOrderedHistory()
    {
        // Arrange: Create history buffer with capacity of 5 readings
        var history = CreateHistoryWithWindowSize(WindowSize);
        var readings = new[] { 20m, 21m, 22m, 23m, 24m };

        // Act: Add temperature readings sequentially
        foreach (var reading in readings) history.AddValue(TemperatureSensor, reading);

        // Get the stored values
        var storedValues = history.GetValues(TemperatureSensor);

        // Assert: Verify all readings stored in insertion order
        storedValues.Should()
            .HaveCount(5)
            .And.ContainInOrder(readings);
    }

    [Fact]
    public void AddValue_WhenCapacityExceeded_ShouldRemoveOldestValue()
    {
        // Arrange: Fill history to capacity
        var history = CreateHistoryWithWindowSize(WindowSize);
        var initialReadings = new[] { 20m, 21m, 22m, 23m, 24m };

        foreach (var reading in initialReadings) history.AddValue(TemperatureSensor, reading);

        // Act: Add one more reading beyond capacity
        history.AddValue(TemperatureSensor, 25m);

        var storedValues = history.GetValues(TemperatureSensor);

        // Assert: Oldest value (20m) removed, window shifted forward
        storedValues.Should()
            .HaveCount(WindowSize, "window size must remain constant after overflow")
            .And.ContainInOrder(21m, 22m, 23m, 24m, 25m)
            .And.NotContain(20m, "oldest value should be discarded");
    }

    [Fact]
    public void AddValue_WithMultipleSensorTypes_ShouldMaintainSeparateHistories()
    {
        // Arrange: Create history for multiple sensors
        var history = CreateHistoryWithWindowSize(WindowSize);
        var temperatureReadings = new[] { 20m, 21m, 22m };
        var humidityReadings = new[] { 60m, 62m, 65m };

        // Act: Add readings to different sensor types
        foreach (var temp in temperatureReadings) history.AddValue(TemperatureSensor, temp);

        foreach (var humidity in humidityReadings) history.AddValue(HumiditySensor, humidity);

        // Assert: Each sensor type maintains independent history
        var tempHistory = history.GetValues(TemperatureSensor);
        var humidityHistory = history.GetValues(HumiditySensor);

        tempHistory.Should().ContainInOrder(temperatureReadings);
        humidityHistory.Should().ContainInOrder(humidityReadings);
        tempHistory.Should().NotContain(humidityReadings);
    }

    [Fact]
    public void GetValues_ForUnknownSensorType_ShouldReturnEmptyCollection()
    {
        // Arrange: Create history, add only Temperature readings
        var history = CreateHistoryWithWindowSize(WindowSize);
        history.AddValue(TemperatureSensor, 22m);

        // Act: Request history for sensor type that was never used
        var unknownSensorHistory = history.GetValues("Pressure");

        // Assert: Return empty array, not null
        unknownSensorHistory.Should()
            .NotBeNull()
            .And.BeEmpty("querying non-existent sensor should return empty collection");
    }

    [Fact]
    public void AddValue_WithDifferentCasings_ShouldTreatAsSameSensor()
    {
        // Arrange: Create history
        var history = CreateHistoryWithWindowSize(WindowSize);

        // Act: Add values using different case variations
        history.AddValue("Temperature", 20m);
        history.AddValue("TEMPERATURE", 21m);
        history.AddValue("temperature", 22m);

        // Assert: All added to same buffer (case-insensitive)
        // Query using any case variation should return all 3 values
        var storedValuesLower = history.GetValues("temperature");
        var storedValuesUpper = history.GetValues("TEMPERATURE");
        var storedValuesMixed = history.GetValues("TeMpErAtUrE");

        storedValuesLower.Should()
            .HaveCount(3)
            .And.ContainInOrder(20m, 21m, 22m);

        storedValuesUpper.Should()
            .HaveCount(3)
            .And.ContainInOrder(20m, 21m, 22m);

        storedValuesMixed.Should()
            .HaveCount(3)
            .And.ContainInOrder(20m, 21m, 22m);
    }

    [Fact]
    public async Task AddValue_WithConcurrentWrites_ShouldBeThreadSafe()
    {
        // Arrange: Create history
        var history = CreateHistoryWithWindowSize(100);
        const int threadCount = 10;
        const int writesPerThread = 20;
        var tasks = new List<Task>();

        // Act: Multiple threads adding values concurrently
        for (var t = 0; t < threadCount; t++)
            tasks.Add(Task.Run(() =>
            {
                for (var i = 0; i < writesPerThread; i++) history.AddValue(TemperatureSensor, i + 20m);
            }));

        await Task.WhenAll(tasks.ToArray());

        // Assert: Verify all writes succeeded without exceptions
        var finalHistory = history.GetValues(TemperatureSensor);
        finalHistory.Should()
            .HaveCount(100, "capacity is 100, should be full after 200 concurrent writes")
            .And.AllSatisfy(v => v.Should().BeGreaterThanOrEqualTo(20m));
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddValue_WithInvalidSensorType_ShouldThrowArgumentException(string invalidSensorType)
    {
        // Arrange: Create history
        var history = CreateHistoryWithWindowSize(WindowSize);

        // Act & Assert: Verify exception thrown for invalid sensor type
        var action = () => history.AddValue(invalidSensorType, 22.5m);

        action.Should()
            .Throw<ArgumentException>("sensor type must be valid non-empty string");
    }


    [Fact]
    public void AddValue_WithNullSensorType_ShouldThrowArgumentNullException()
    {
        // Arrange: Create history
        var history = CreateHistoryWithWindowSize(WindowSize);

        // Act & Assert: Verify exception thrown for null sensor type
        var action = () => history.AddValue(null!, 22.5m);

        action.Should()
            .Throw<ArgumentException>("sensor type cannot be null");
    }

    [Fact]
    public void Constructor_WithDefaultOptions_ShouldUseConfiguredWindowSize()
    {
        // Arrange: Create history with custom window size
        var customWindowSize = 15;
        var options = Options.Create(new SensorMonitoringOptions { WindowSize = customWindowSize });
        var history = new InMemorySensorValuesHistory(options);

        // Act: Fill beyond default size with custom size
        for (var i = 0; i < customWindowSize + 5; i++) history.AddValue(TemperatureSensor, 20m + i);

        var storedValues = history.GetValues(TemperatureSensor);

        // Assert: Window size is respected from configuration
        storedValues.Should()
            .HaveCount(customWindowSize,
                $"should respect configured window size of {customWindowSize}");
    }
}