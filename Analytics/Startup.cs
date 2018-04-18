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
using AspNet.Security.OAuth.Validation;
using Analytics.Models.Property;

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
            services.AddMvc().AddMvcOptions(o => o.OutputFormatters.Add(new XmlDataContractSerializerOutputFormatter())) // Adds XML as an output format as well as JSON (which is included by default)
                .AddJsonOptions(o => o.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);

            var connectionString = Configuration["ConnectionStrings:analyticsDbConnectionString"];
            services.AddDbContext<AnalyticsContext>(o => 
            {
                o.UseSqlServer(connectionString);
                o.UseOpenIddict();
            });

            services.AddOpenIddict(o =>
            {
                // Register Entity Framework stores
                o.AddEntityFrameworkCoreStores<AnalyticsContext>();
                o.AddMvcBinders();
                o.EnableTokenEndpoint("/api/users/login");
                // Password flow is: User --Password--> Client --Password--> Auth Server --Access Token--> Client
                o.AllowPasswordFlow();
                o.DisableHttpsRequirement();
            });

            services.AddAuthentication(o =>
            {
                // Bearer authentication scheme
                o.DefaultScheme = OAuthValidationDefaults.AuthenticationScheme;
            })
            .AddOAuthValidation();

            services.AddScoped<IAnalyticsRepository, AnalyticsRepository>(); // Scoped is created once per request
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IProjectRepository, ProjectRepository>();
            services.AddScoped<IEventRepository, EventRepository>();
            services.AddScoped<IProjectUserRepository, ProjectUserRepository>();
            services.AddScoped<ISessionRepository, SessionRepository>();
            services.AddScoped<IPropertyRepository, PropertyRepository>();
            services.AddScoped<IMetricRepository, MetricRepository>();
            services.AddScoped<IGraphRepository, GraphRepository>();
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
                cfg.CreateMap<Entities.User, Models.UserDto>(); // Convention based; will map property names on the source object to the same names on the destination object.
                                                                // If property doesn't exist, it will be ignored.
                cfg.CreateMap<Entities.User, Models.UserForCreationDto>();
                //cfg.CreateMap<Entities.User, Models.User.UserResetPasswordDto>();
                cfg.CreateMap<Entities.Project, Models.ProjectForCreationDto>();
                cfg.CreateMap<Entities.Analyser, Models.AnalyserDto>();
                cfg.CreateMap<Entities.Project, Models.ProjectDetailsDto>();
                cfg.CreateMap<Entities.Event, Models.EventForCreationDto>();
                cfg.CreateMap<Entities.Property, Models.PropertyDto>();
                cfg.CreateMap<Entities.Property, PropertyForDisplayDto>();
                cfg.CreateMap<Entities.Metric, MetricPartDto>();
                cfg.CreateMap<Entities.MetricPart, MetricPartDto>();
                cfg.CreateMap<Entities.ProjectUser, Models.ProjectUserDto>();
                cfg.CreateMap<Entities.Graph, Models.Graph.GraphDto>();
            });

            app.UseCors(corsPolicyBuilder =>
               corsPolicyBuilder
              .AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials()
            );
            app.UseAuthentication();
            app.UseMvc();            
        }
    }
}
