using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using ServiceBusAction.Builders;
using Swashbuckle.AspNetCore.Swagger;
using System;
using System.IO;
using System.Linq;

namespace ServiceBusAction
{

    public class StartupWeb
    {

        public StartupWeb(IConfiguration configuration)
        {
            Configuration = configuration;
            _useSwagger = configuration.GetValue<bool>(ServiceConstants.UseSwagger);
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            if (_useSwagger)
            {
                services.AddSwaggerGen(swagger =>
                {
                    swagger.DescribeAllEnumsAsStrings();
                    swagger.DescribeAllParametersInCamelCase();
                    swagger.IgnoreObsoleteActions();
                    swagger.AddSecurityDefinition(ServiceConstants.Key, new Swashbuckle.AspNetCore.Swagger.ApiKeyScheme { Name = ServiceConstants.ApiKey });

                    //swagger.TagActionsBy(a => a.ActionDescriptor is Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor b
                    //    ? b.ControllerTypeInfo.Assembly.FullName.Split('.')[2].Split(',')[0].Replace("Web", "")
                    //    : a.ActionDescriptor.DisplayName
                    //);

                    //swagger.DocInclusionPredicate((f, a) =>
                    //{
                    //    return a.ActionDescriptor is ControllerActionDescriptor b && b.MethodInfo.GetCustomAttributes<ExternalApiRouteAttribute>().Any();
                    //});

                    swagger.SwaggerDoc(ServiceConstants.VersionUmber, new Swashbuckle.AspNetCore.Swagger.Info
                    {
                        Title = ServiceConstants.Title,
                        License = new License() { Name = ServiceConstants.LicenceName },
                        Version = ServiceConstants.Version,
                    });

                    var doc = DocumentationHelpers.ConcateDocumentations(ServiceConstants.AssemblyDocumentations);
                    if (doc != null)
                        swagger.IncludeXmlComments(() => doc);

                });
            }

            var brokers = services.RegisterBrokers(Configuration);
            Configuration.RegisterLogToBrokers(brokers);
            Configuration.RegisterCustomCode();

            services.CreateActionRepositories();


            Configuration.CreateSubscriptionInstances(services, brokers);

            //services.BuildServiceProvider()

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {

            app.ApplicationServices
                .RegisterBusinessActions()
                .RegisterListeners();

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

            string Namespace = Configuration.GetValue<string>(ServiceConstants.Namespace);

            // Load static content from module assemblies
            var staticProviders = AppDomain.CurrentDomain.GetAssemblies().AsParallel()
               .Where(a => a.FullName.StartsWith(Namespace) && !a.FullName.Contains("Tests,"))
               //.Where(a => a.GetTypes().Any(t => t.GetCustomAttributes(typeof(ModuleTopLevelWebPageAttribute)).Any()))
               .Select(a => new EmbeddedFileProvider(a)).ToList<IFileProvider>();

            staticProviders.Add(new PhysicalFileProvider(Path.Join(env.ContentRootPath, "wwwroot")));

            app.UseMiddleware<LoggingMiddleware>();

            if (_useSwagger)
            {
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    string p = @"/" + Path.Combine("swagger", ServiceConstants.VersionUmber, "swagger.json").Replace("\\", "/");
                    c.SwaggerEndpoint(p, ServiceConstants.SawggerName);
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


        private readonly bool _useSwagger;

    }

}
