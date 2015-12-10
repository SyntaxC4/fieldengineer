using System;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Web.Http;
using FieldEngineerLiteService.Models;
using Microsoft.Azure.Mobile.Server.Config;
using Microsoft.Owin;
using Owin;

[assembly: OwinStartup(typeof(FieldEngineer.Startup))]

namespace FieldEngineer
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration config = new HttpConfiguration();

            //For more information on Web API tracing, see http://go.microsoft.com/fwlink/?LinkId=620686 
            config.EnableSystemDiagnosticsTracing();

            new MobileAppConfiguration()
                .UseDefaultConfiguration()
                .ApplyTo(config);

            // Use Entity Framework Code First to create database tables based on your DbContext
            Database.SetInitializer<JobDbContext>(new JobDbContextInitializer());

            app.UseWebApi(config);
        }
    }
}
