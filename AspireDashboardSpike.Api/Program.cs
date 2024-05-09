using AspireDashboardSpike.Api;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddOpenTelemetry(x =>
{
	x.IncludeScopes = true;
	x.IncludeFormattedMessage = true;
});

builder.Services.AddOpenTelemetry()
	.WithMetrics(x =>
	{
		x.AddRuntimeInstrumentation()
			.AddMeter(
				"Microsoft.AspNetCore.Hosting",
				"Microsoft.AspNetCore.Server.Kestrel",
				"System.Net.Http",
				"AspireDashboardSpike.Api");
	})
	.WithTracing(x =>
	{
		if (builder.Environment.IsDevelopment())
		{
			x.SetSampler<AlwaysOnSampler>();
		};

		x.AddAspNetCoreInstrumentation()
			.AddGrpcClientInstrumentation()
			.AddHttpClientInstrumentation();
	});

builder.Services.Configure<OpenTelemetryLoggerOptions>(logging => logging.AddOtlpExporter());
builder.Services.ConfigureOpenTelemetryMeterProvider(metrics => metrics.AddOtlpExporter());
builder.Services.ConfigureOpenTelemetryTracerProvider(tracing => tracing.AddOtlpExporter());

builder.Services.AddHealthChecks()
	.AddCheck("self", () => HealthCheckResult.Healthy(), ["live",]);

builder.Services.ConfigureHttpClientDefaults(http =>
{
	http.AddStandardResilienceHandler();
});

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddMetrics();
builder.Services.AddSingleton<WeatherMetrics>();

var app = builder.Build();

app.MapHealthChecks("/health");

app.MapHealthChecks("/alive", new HealthCheckOptions
{
	Predicate = r => r.Tags.Contains("live"),
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

var summaries = new[]
{
	"Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", async (WeatherMetrics weatherMetrics) =>
{
	using var _ = weatherMetrics.MeasureRequestDuration();
	try
	{
		await Task.Delay(Random.Shared.Next(5, 100));
		var forecast = Enumerable.Range(1, 5).Select(index =>
			new WeatherForecast
			(
				DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
				Random.Shared.Next(-20, 55),
				summaries[Random.Shared.Next(summaries.Length)]
			))
			.ToArray();
		return forecast;
	}
	finally
	{
		weatherMetrics.IncreaseWeatherRequestCount();
	}

})
.WithName("GetWeatherForecast")
.WithOpenApi();

app.Run();

internal record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
	public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
