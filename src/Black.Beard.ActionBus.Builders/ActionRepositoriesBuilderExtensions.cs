﻿using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Bb.ActionBus.Builders
{

    internal static class ActionRepositoriesBuilderExtensions
    {

        /// <summary>
        /// Registers the business actions found or explicited specified.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
        /// <param name="explicitTypes">The explicit types.</param>
        /// <returns></returns>
        public static IServiceProvider RegisterBusinessActions(this IServiceProvider serviceProvider, params Type[] explicitTypes)
        {

            var actionRepositories = serviceProvider.GetService(typeof(Bb.ActionBus.ActionRepositories)) as ActionRepositories;
            actionRepositories.Inject(serviceProvider);

            var key = ActionOrder.BusinessActionBusContants;

            var types = new HashSet<Type>(TypeDiscovery.Instance.GetTypesWithAttributes<ExposeClassAttribute>(typeof(object), (attr) => attr.Context == key).ToArray());
            foreach (var item in explicitTypes)
                types.Add(item);

            foreach (var item in types)
                actionRepositories.Register(item);

            return serviceProvider;

        }

        /// <summary>
        /// Add a service which will crawl all modules for top level main menu pages.
        /// </summary>
        public static ActionRepositories CreateActionRepositories(this IServiceCollection services)
        {
            var reps = new ActionRepositories();
            services.AddSingleton(reps);
            return reps;
        }

        public static void RegisterCustomCode(this IConfiguration configuration)
        {

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

    }

}
