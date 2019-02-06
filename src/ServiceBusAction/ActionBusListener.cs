//using Bb.ActionBus;
//using Bb.ComponentModel;
//using Microsoft.AspNetCore.Builder;
//using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.DependencyInjection;

//namespace ServiceBusAction
//{
//    public class ActionBusListener
//    {

//        public ActionBusListener(IConfiguration configuration)
//        {
//            Configuration = configuration;
//        }

//        public IConfiguration Configuration { get; }

//        // This method gets called by the runtime. Use this method to add services to the container.
//        public void ConfigureServices(IServiceCollection services)
//        {

//            var factory = TypeDiscovery.Initialize();

//            var reps = new ActionRepositories()
//                //.Register(() => new ClassCustom1(), 10)
//                ;

//        }

//        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
//        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
//        {

//            if (env.IsDevelopment())
//            {
//            }
//            else
//            {
//            }

//        }
//    }
//}
