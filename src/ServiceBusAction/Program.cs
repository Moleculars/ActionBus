using Bb.Helpers;
using Bb.Web;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
using System.Reflection;

namespace ServiceBusAction
{
    public class Program
    {

        public static void Main(string[] args)
        {


            using (Bb.Logs.Serilog.SerilogTraceListener.Initialize())
            {

                var serviceHostBuilder = CreateServiceHostBuilder(args)
                    .UseStartup<StartupWeb>()
                    ;

                // Start service
                using (var serviceHost = serviceHostBuilder.Build())
                {

                    IConfiguration configuration = (IConfiguration)serviceHost.Services.GetService(typeof(IConfiguration));
                    Globals.SetCulture(configuration.GetValue<string>("Culture"));
                    Globals.SetFormatDateCulture(configuration.GetValue<string>("FormatDateCulture"));

                    var task = serviceHost.RunAsync();

                    // Wait exit
                    Console.CancelKeyPress += (sender, eventArgs) =>
                    {

                        Trace.WriteLine("Received a stop notification, engine shutdown");
                        // do something

                        Trace.WriteLine("engine main service has stopped.");
                        eventArgs.Cancel = true;
                        // Environment.Exit(0);

                    };

                    task.Wait();

                }

            }

        }

        public static IWebHostBuilder CreateServiceHostBuilder(string[] args)
        {

            var _config = Initialization.Load();

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {

                    var env = context.HostingEnvironment;

                    config.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                          .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                          .LoadDirectoriesJsonFiles(_config)
                          .AddEnvironmentVariables()
                          .AddCommandLine(args)
                    ;

                    if (env.IsDevelopment())
                    {

                        var appAssembly = Assembly.Load(new AssemblyName(env.ApplicationName));
                        if (appAssembly != null)
                            config.AddUserSecrets(appAssembly, optional: true);

                    }

                })
                .ConfigureLogging((hostingContext, logging) =>
                {

                    var env = hostingContext.HostingEnvironment;
                    var configLogging = hostingContext.Configuration.GetSection("Logging");
                    logging.AddConfiguration(configLogging);
                    if (env.IsDevelopment())
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    }

                })
                .UseDefaultServiceProvider((context, options) =>
                {
                    options.ValidateScopes = context.HostingEnvironment.IsDevelopment();
                });

            ;

        }

    }


    //public class Provider : ConfigurationProvider
    //{

    //    public override void Load()
    //    {
    //        base.Load();
    //    }

    //    public override IEnumerable<string> GetChildKeys(IEnumerable<string> earlierKeys, string parentPath)
    //    {
    //        return base.GetChildKeys(earlierKeys, parentPath);
    //    }

    //}

}
