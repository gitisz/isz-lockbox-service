using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace isz.lockbox.service;

public class Programm
{

  public static void Main(string[] args)
  {
    Log.Logger = new LoggerConfiguration()
      .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
      .Enrich.FromLogContext()
      .WriteTo.Console()
      .CreateBootstrapLogger();

    Log.Information("Starting API...");

    var switchMappings = new Dictionary<string, string>()
    {
        { "--host", "HostPort" }
    };

    if (args.Length == 0)
    {
      args = new string[] { "--host", "5001" };
    };

    CreateWebHostBuilder(args, switchMappings)
        .Build()
        .Run();
  }

  public static IHostBuilder CreateWebHostBuilder(string[] args, Dictionary<string, string> switchMappings) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureAppConfiguration((hostingContext, config) =>
          {
            var env = hostingContext.HostingEnvironment;
            config.Sources.Clear();
            config.SetBasePath(env.ContentRootPath);
            config.AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true);
            config.AddEnvironmentVariables();

            if (args != null)
            {
              config.AddCommandLine(args, switchMappings);
            }
          })
          .ConfigureWebHostDefaults(builder =>
          {
            builder.UseKestrel((config, opts) =>
            {
              opts.ListenAnyIP(int.Parse(config.Configuration["HostPort"]));
            });
            builder.UseStartup<StartUp>();
            builder.UseUrls($"http://http//*:{args[0]}");
          })
          .UseSerilog((context, services, configuration) =>
          {
            configuration
              .ReadFrom.Configuration(context.Configuration)
              .ReadFrom.Services(services)
              .Enrich.FromLogContext()
              .WriteTo.Console(new RenderedCompactJsonFormatter());
          });

}