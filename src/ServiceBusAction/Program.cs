using Bb.Core.Helpers;
using Bb.Logs.Serilog;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System;
using System.Diagnostics;
using System.IO;

namespace ServiceBusAction
{
    public class Program
    {

        public static void Main(string[] args)
        {

            var serviceHostBuilder = CreateServiceHostBuilder(args)
                .UseStartup<StartupWeb>()
                ;

            using (var listener = SerilogTraceListener.Initialize()) // Register log
            {

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

            string path = @"D:\Src\ActionBus\src\ServiceBusAction\Configurations\";
            var dir = new DirectoryInfo(path);

            return WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {

                    var e = context.HostingEnvironment;

                    foreach (var file in dir.GetFiles("*.json"))
                        config.AddJsonFile(file.FullName, optional: false, reloadOnChange: false);

                    config.AddEnvironmentVariables();


                    //config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                    //      .AddJsonFile("appsettings.developement.json", optional: true, reloadOnChange: false)
                    //      .AddEnvironmentVariables();

                });

        }

    }

}
