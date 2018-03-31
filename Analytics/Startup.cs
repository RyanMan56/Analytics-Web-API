using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc.Formatters;
using Analytics.Entities;
using Microsoft.EntityFrameworkCore;
using Analytics.Services;

namespace Analytics
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc().AddMvcOptions(o => o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter())); // Adds XML as an output format as well as JSON (which is included by default)

            var connectionString = Configuration["ConnectionStrings:analyticsDbConnectionString"];
            services.AddDbContext<AnalyticsContext>(o => o.UseSqlServer(
                connectionString));
            

            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>(); // Scoped is created once per request
            services.AddScoped<IUserRepository, UserRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, AnalyticsContext analyticsContext)
        {
            loggerFactory.AddConsole().AddDebug();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            analyticsContext.EnsureSeedDataForContext();

            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<Entities.User, Models.User.UserDto>(); // Convention based; will map property names on the source object to the same names on the destination object.
                                                                // If property doesn't exist, it will be ignored.
                cfg.CreateMap<Entities.User, Models.User.UserForCreationDto>();
            });

            app.UseMvc();
        }
    }
}
