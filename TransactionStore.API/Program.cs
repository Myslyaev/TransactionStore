using Serilog;
using TransactionStore.API.Configuration;
using TransactionStore.Business;
using TransactionStore.DataLayer;

try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Configuration.AddJsonFile("appsettings.DefaultConfiguration.json", optional: false, reloadOnChange: true);
    await builder.Configuration.ReadSettingsFromConfigurationManager();

    Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
    builder.Logging.ClearProviders();

    builder.Services.ConfigureApiServices(builder.Configuration);
    builder.Services.ConfigureBllServices();
    builder.Services.ConfigureDalServices();

    builder.Host.UseSerilog();

    var app = builder.Build();

    app.UseMiddleware<ExceptionMiddleware>();

    app.UseSwagger();
    app.UseSwaggerUI();

    app.UseHttpsRedirection();
    app.UseSerilogRequestLogging();

    app.MapControllers();

    Log.Information("Running up.");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex.Message);
}
finally
{
    Log.Information("App stopped.");
    Log.CloseAndFlush();
}