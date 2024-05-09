using System.Diagnostics.Metrics;

namespace AspireDashboardSpike.Api;

public class WeatherMetrics
{
	public const string MeterName = "AspireDashboardSpike.Api";

	private readonly Counter<long> _weatherRequestCounter;
	private readonly Histogram<double> _weatherRequestDuration;

	public WeatherMetrics(IMeterFactory meterFactory)
	{
		var meter = meterFactory.Create(MeterName);

		_weatherRequestCounter = meter.CreateCounter<long>(
			"aspiredashboardspike.api.weather_requests.count");

		_weatherRequestDuration = meter.CreateHistogram<double>(
			"aspiredashboardspike.api.weather_requests.duration",
			unit: "ms");
	}

	public void IncreaseWeatherRequestCount() => _weatherRequestCounter.Add(1);
	public TrackedRequestDuration MeasureRequestDuration() => new(_weatherRequestDuration);
}

public sealed class TrackedRequestDuration(Histogram<double> histogram) : IDisposable
{
	private readonly long _requestStartTime = TimeProvider.System.GetTimestamp();

	public void Dispose()
	{
		var elapsed = TimeProvider.System.GetElapsedTime(_requestStartTime);
		histogram.Record(elapsed.TotalMilliseconds);
	}
}
