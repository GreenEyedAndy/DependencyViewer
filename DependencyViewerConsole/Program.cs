// See https://aka.ms/new-console-template for more information

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace DependencyViewerConsole;

partial class Program
{
    public static void Main(string[] args)
    {
        var builder = new ConfigurationBuilder();
        BuildConfig(builder, args);

        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(builder.Build())
            .Enrich.FromLogContext()
            .WriteTo.Console()
            .CreateLogger();

        Log.Logger.Information("Application starting");
        
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                services.AddTransient<IGreetingService, GreetingService>();
                services.AddTransient<IDependencyViewer, DependencyViewer>();
                services.AddTransient<CsProjParser>();
            })
            .UseSerilog()
            .Build();

        var svc = ActivatorUtilities.GetServiceOrCreateInstance<IDependencyViewer>(host.Services);
        var result = svc.Walk();
    }

    static void BuildConfig(IConfigurationBuilder builder, string[] args)
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    }
}