using Bb.ActionBus;
using Bb.ComponentModel;
using Bb.ComponentModel.Attributes;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace ServiceBusAction
{
    public class StartupWeb
    {

        public StartupWeb(IConfiguration configuration)
        {
            Configuration = configuration;
            _useSwagger = configuration.GetValue<bool>("useSwagger");
        }

        public IConfiguration Configuration { get; }

        private readonly bool _useSwagger;

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            Configuration.RegisterCustomCode();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            if (_useSwagger)
            {
                services.AddSwaggerGen(swagger =>
                {
                    swagger.DescribeAllEnumsAsStrings();
                    swagger.DescribeAllParametersInCamelCase();
                    swagger.IgnoreObsoleteActions();
                    swagger.AddSecurityDefinition("key", new Swashbuckle.AspNetCore.Swagger.ApiKeyScheme { Name = "ApiKey" });

                    //swagger.TagActionsBy(a => a.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor b
                    //    ? b.ControllerTypeInfo.Assembly.FullName.Split('.')[2].Split(',')[0].Replace("Web", "")
                    //    : a.ActionDescriptor.DisplayName
                    //);

                    //swagger.DocInclusionPredicate((f, a) =>
                    //{
                    //    return a.ActionDescriptor is ControllerActionDescriptor b && b.MethodInfo.GetCustomAttributes<ExternalApiRouteAttribute>().Any();
                    //});

                    swagger.SwaggerDoc("v1", new Swashbuckle.AspNetCore.Swagger.Info
                    {
                        Title = "Black Beard Action APIs",
                        License = new License() { Name = "Only usable with a valid partner contract." },
                        Version = "1.0.0",
                    });

                    var doc = DocumentationHelpers.ConcateDocumentations("Black.Beard*.xml");
                    if (doc != null)
                        swagger.IncludeXmlComments(() => doc);

                });
            }

            services.RegisterBusinessActions(Configuration);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseMvc();

            // Load static content from module assemblies
            var staticProviders = AppDomain.CurrentDomain.GetAssemblies().AsParallel()
               .Where(a => a.FullName.StartsWith("Black.Beard.") && !a.FullName.Contains("Tests,"))
               //.Where(a => a.GetTypes().Any(t => t.GetCustomAttributes(typeof(ModuleTopLevelWebPageAttribute)).Any()))
               .Select(a => new EmbeddedFileProvider(a)).ToList<IFileProvider>();
            staticProviders.Add(new PhysicalFileProvider(Path.Join(env.ContentRootPath, "wwwroot")));

            app.UseMiddleware<LoggingMiddleware>();

            if (_useSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "My First Swagger Environment");
                    c.DefaultModelsExpandDepth(0);
                });

            }

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Action}/{action=routes}");

                //routes.MapSpaFallbackRoute(
                //    name: "spe-fallback",
                //    defaults: new { controller = "Home", action = "Index" });                

            });

            //app.MapWhen(r => !r.Request.Path.Value.StartsWith("/Swagger"), builders =>
            //{

            //    builders.UseMvc(routes =>
            //    {

            //        routes.MapSpaFallbackRoute(
            //            name: "spe-fallback",
            //            defaults: new { controller = "Action", action = "Index" });

            //    });

            //});

        }
    }


    public static class ActionRepositoriesBuilderExtensions
    {
        /// <summary>
        /// Add a service which will crawl all modules for top level main menu pages.
        /// </summary>
        public static IServiceCollection RegisterBusinessActions(this IServiceCollection services, IConfiguration configuration)
        {

            var types = TypeDiscovery.Instance.GetTypesWithAttributes(typeof(ExposeClassAttribute));

            var reps = new ActionRepositories(configuration, services, AcquitmentQueue, DeadQueue, 10);

            foreach (var item in types)
                reps.Register(item);

            services.AddSingleton(reps);

            return services;
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

        private static void AcquitmentQueue(object sender, ActionOrderEventArgs e)
        {

        }

        private static void DeadQueue(object sender, ActionOrderEventArgs e)
        {

        }

    }

}
