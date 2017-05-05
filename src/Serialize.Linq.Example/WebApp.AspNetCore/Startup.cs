using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR.Hubs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serialize.Linq;

namespace WebApp.AspNetCore
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", optional: true)
                .AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfigurationRoot Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddMvc().AddJsonOptions(opt =>
            {
                opt.SerializerSettings.Converters.Add(new ExpressionNodeJsonConverter());
            });
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new ExpressionNodeJsonConverter());

            var serializer = JsonSerializer.Create(settings);
            services.AddSignalR(opt =>
            {
                opt.EnableJSONP = true;
                opt.Hubs.EnableDetailedErrors = true;
            });

            services.Add(new ServiceDescriptor(typeof(JsonSerializer),
                provider => serializer, ServiceLifetime.Transient));

            // Signalr's 'DefaultParameterResolver' uses a hardcoded JsonSerializer
            // override the default
            var hubParamsResolver = new SerializeLinqHubParameterResolver(serializer);
            
            services.Add(new ServiceDescriptor(typeof(IParameterResolver),
                provider => hubParamsResolver, ServiceLifetime.Transient));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseMvc();
            app.UseWebSockets();
            app.UseSignalR();
        }
    }
}
