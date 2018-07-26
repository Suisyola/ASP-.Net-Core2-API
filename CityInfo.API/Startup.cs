using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CityInfo.API.Entities;
using CityInfo.API.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using NLog.Extensions.Logging;

namespace CityInfo.API
{
    public class Startup
    {

        // Start of Implementation for usage of configuration. This is for ASP .Net Core 1
        /*
        public static IConfigurationRoot Configuration;
        
        public Startup(IHostingEnvironment env)
        {
            // This block of code is for allowing usage of config file, appSettings.json or appSettings.Production.json, in the application
            // new ConfigurationBuilder is to create an instance of the Configuration
            var builder = new ConfigurationBuilder()
                // indicate where to find settings file
                .SetBasePath(env.ContentRootPath)
                // optional:false indicates that appSettings.json is required file
                // reloadOnChange:true indicates that appSettings.json must be reloaded on change of settings
                .AddJsonFile("appSettings.json", optional: false, reloadOnChange: true)
                // This adds Production config file. Order is important. If settings appear on both appSettings and appSettings.Production, the value will take from appSettings.Production
                .AddJsonFile($"appSettings.{env.EnvironmentName}.json", optional: true, reloadOnChange: true)
                // Retrieve values from Environment Variables if it is used in application. e.g. connectionStrings:cityInfoDBConnectionString
                .AddEnvironmentVariables();

            // Create a configuration and store in the Configuration variable
            Configuration = builder.Build();
        }
        */
        // End of Implementation for usage of configuration. This is for ASP .Net Core 1


        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit http://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                // This allows response type to output application/xml when API consumer wants application/xml in Accept header of request
                .AddMvcOptions(o => o.OutputFormatters.Add(
                    new XmlDataContractSerializerOutputFormatter()));
            // This outputs JSON variable naming in PascalCase. Without this block of code, the default is camelCase
            //.AddJsonOptions(o => {
            //    if (o.SerializerSettings.ContractResolver != null)
            //    {
            //        var castedResolver = o.SerializerSettings.ContractResolver
            //            as DefaultContractResolver;
            //        castedResolver.NamingStrategy = null;
            //    }
            //});

            // add a custom mail service
            // by using compiler directives, #if etc, application uses different mail service. If using debug build, use LocalMailService.
#if DEBUG
            services.AddTransient<IMailService, LocalMailService>();
#else
            services.AddTransient<IMailService, CloudMailService>();
#endif
            var connectionString = Startup.Configuration["connectionStrings:cityInfoDBConnectionString"];
            services.AddDbContext<CityInfoContext>(o => o.UseSqlServer(connectionString));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            CityInfoContext cityInfoContext)
        {
            // No need to add these loggers in ASP.NET Core 2.0: the call to WebHost.CreateDefaultBuilder(args) 
            // in the Program class takes care of that.
            //loggerFactory.AddConsole();
            //loggerFactory.AddDebug();

            // Use NLog
            loggerFactory.AddNLog();

            // Display detailed exception page, when the environment in Project properties is set to Development
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            // Seed data, if required
            cityInfoContext.EnsureSeedDataForContext();

            // Use simple status code page for HTTP response
            app.UseStatusCodePages();

            app.UseMvc();

            //app.Run((context) =>
            //{
            //    throw new Exception("Example exception");
            //});

            // This will always return Hello World for HTTP response
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}
