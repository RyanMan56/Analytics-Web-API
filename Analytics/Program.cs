using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using Analytics.Utils;
using AutoMapper.Configuration;
using Microsoft.Extensions.Configuration;

namespace Analytics
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = BuildWebHost(args);
            //using (var scope = host.Services.CreateScope())
            //{
            //    // Initialising roles
            //    var services = scope.ServiceProvider;                
            //    //var serviceProvider = services.GetRequiredService<IServiceProvider>();
            //    //SeedRoles.CreateRoles(serviceProvider).Wait();
            //}
            host.Run();
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .Build();
    }
}
