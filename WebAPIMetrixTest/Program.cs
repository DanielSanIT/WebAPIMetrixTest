using OpenTelemetry.Resources;
using OpenTelemetry.Metrics;
using Serilog;
using OpenTelemetry.Trace;
using WebAPIMetrixTest.Services;
using WebAPIMetrixTest.Services.MericsHelper;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource
                .AddService(serviceName: builder.Environment.ApplicationName))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddMeter("DemoMeter")
                .AddRuntimeInstrumentation()
                .AddPrometheusExporter()
                .AddHttpClientInstrumentation()
                .AddProcessInstrumentation()
                .AddMeter("Microsoft.AspNetCore.Hosting")
                .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                .AddMeter("Microsoft.AspNetCore.Http.Connections")
                .AddMeter("Microsoft.AspNetCore.Routing")
                .AddMeter("Microsoft.AspNetCore.Diagnostics")
                .AddMeter("Microsoft.AspNetCore.RateLimiting")
                .AddView(instrumentName: "Random-numbers", new ExplicitBucketHistogramConfiguration { Boundaries = [20, 40, 60, 80, 100] })
                .AddView(instrumentName: "Random-times", new ExplicitBucketHistogramConfiguration { Boundaries = [2, 5, 10, 15, 20] })
            )
            .WithTracing(tracing => tracing
                .AddHttpClientInstrumentation(
                // Note: Only called on .NET & .NET Core runtimes.
                (options) => options.FilterHttpRequestMessage =
                    (httpRequestMessage) =>
                    {
                        // Example: Only collect telemetry about HTTP GET requests.
                        return (httpRequestMessage.Method.Equals(HttpMethod.Get) || httpRequestMessage.Method.Equals(HttpMethod.Post));
                    })
                .SetErrorStatusOnException()
                .AddSource("MyCompany.MyProduct.MyLibrary")
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                );

        builder.Services.AddSingleton<DataService>();
        builder.Services.AddSingleton<MeasurementCache>();
        builder.Services.AddSingleton<MetricsService>();
        //builder.Services.AddSingleton<MetricsSink>();

        var metricsService = builder.Services.BuildServiceProvider().GetRequiredService<MetricsService>();
        var metricsSink = new MetricsSink(metricsService);
        Log.Logger = new LoggerConfiguration()
            //.MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.Sink(metricsSink)
            .CreateLogger();
        Log.Logger.Error("Hello, Serilog!");

        builder.Services.AddLogging(loggingBuilder => loggingBuilder.ClearProviders().AddSerilog(Log.Logger, dispose: true));

        var app = builder.Build();

        // Pre-initialize OpenTelemery collector service
        var openTelemetryMeterInstance = app.Services.GetRequiredService<MetricsService>();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();

        app.UseOpenTelemetryPrometheusScrapingEndpoint();

        app.Run();
    }
}