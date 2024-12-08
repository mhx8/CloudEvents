using CloudEvents.Publisher;
using CloudEvents.Shared;
using Microsoft.Extensions.Logging.Console;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Logging.AddSimpleConsole(
        options =>
        {
            options.ColorBehavior = LoggerColorBehavior.Enabled;
            options.SingleLine = true;
        })
    .AddFilter(
        "Azure",
        LogLevel.None)
    .AddFilter(
        "Microsoft",
        LogLevel.None);
builder.Services.AddDistributedMemoryCache();
builder.Services.AddControllers();
builder.Services.AddAzureServiceBusClients(builder.Configuration);
builder.Services.AddScoped<EventPublisher>();

WebApplication app = builder.Build();
app.MapControllers();
app.Run();