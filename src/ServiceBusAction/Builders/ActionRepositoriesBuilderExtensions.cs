using Bb.ActionBus;
using Bb.Brokers;
using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ServiceBusAction.Builders
{

    public static class ActionRepositoriesBuilderExtensions
    {

        /// <summary>
        /// Add a service which will crawl all modules for top level main menu pages.
        /// </summary>
        public static IServiceCollection RegisterBusinessActions(this IServiceCollection services, IConfiguration configuration, RabbitBrokers brokers)
        {

            ActionRepositoriesBuilderExtensions._acknowledgeQueue = brokers.CreatePublisher(configuration.GetValue<string>("AcknowledgeQueue"));
            ActionRepositoriesBuilderExtensions._deadQueue = brokers.CreatePublisher(configuration.GetValue<string>("DeadQueue"));

            var types = TypeDiscovery.Instance.GetTypesWithAttributes<ExposeClassAttribute>((attr) => attr.Context == "BusinessAction").ToList();

            var reps = new ActionRepositories(configuration, services, AcknowledgeQueue, DeadQueue, 10);

            foreach (var item in types)
                reps.Register(item);

            services.AddSingleton(reps);

            return services;

        }

        public static void RegisterCustomCode(this IConfiguration configuration)
        {


            //brokers


            var currentDirectory = Environment.CurrentDirectory;
            Trace.WriteLine($"current execution directory : {currentDirectory}", TraceLevel.Info.ToString());
            HashSet<string> _paths = new HashSet<string>();

            Bb.ComponentModel.TypeDiscovery.Initialize();

            var folders = CollectFolders(configuration);

            if (folders.Count > 0)
            {
                foreach (var item in folders)
                {
                    var dir = new DirectoryInfo(item);
                    if (dir.Exists)
                        _paths.Add(dir.FullName);
                    else
                    {

                        bool ok = false;
                        try
                        {
                            var newPath = Path.Combine(currentDirectory, item);
                            var dir2 = new DirectoryInfo(item);
                            if (dir2.Exists)
                                _paths.Add(dir2.FullName);

                        }
                        catch (Exception)
                        {

                        }

                        if (!ok)
                        {
                            if (System.Diagnostics.Debugger.IsAttached)
                                System.Diagnostics.Debugger.Break();

                            Trace.WriteLine($"{dir.FullName} doesnt exist", TraceLevel.Error.ToString());
                        }
                    }

                }
            }

            if (_paths.Count > 0)
                TypeDiscovery.Instance.AddDirectories(_paths.ToArray());

        }

        private static List<string> CollectFolders(IConfiguration configuration)
        {

            List<string> folders = new List<string>();

            for (int i = 0; i < 1000; i++)
            {
                var folder = configuration.GetValue<string>($"BusinessMethodFolders:{i}");
                if (!string.IsNullOrEmpty(folder))
                    folders.Add(folder);
                else
                {
                    if (i == 0)
                        Trace.WriteLine("node 'BusinessMethodFolders' not found in the configuration.", TraceLevel.Warning.ToString());
                    break;
                }
            }

            if (folders.Count == 0)
                Trace.WriteLine("no custom code injected.", TraceLevel.Warning.ToString());

            return folders;

        }

        private static void AcknowledgeQueue(object sender, ActionOrderEventArgs e)
        {

            string Result = string.Empty;

            switch (e.Action.Name)
            {

                case "business1.PushScan":
                    Result = ((Guid)e.Action.Result).ToString("B");
                    break;

                default:
                    Result.ToString();
                    break;

            }

            _acknowledgeQueue.Publish(new
            {
                e.Action,
                e.Action.ExecutedAt,                
                Result
            });

        }

        private static void DeadQueue(object sender, ActionOrderEventArgs e)
        {

            var exception = e.Action.Result as Exception;

            _deadQueue.Publish(
                new
                {
                    e.Action,                
                    e.Action.ExecutedAt,
                    e.Action.PushedAt,
                    exception
                }
                );
        }

        private static IBrokerPublisher _acknowledgeQueue;
        private static IBrokerPublisher _deadQueue;

    }

}
