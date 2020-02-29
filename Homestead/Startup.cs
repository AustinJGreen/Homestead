using System;
using System.IO;
using Homestead.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Homestead
{
    public class Startup
    {
        private ILoggerFactory logFactory;
        private ILogger log;

        public Startup()
        {
            logFactory = new LoggerFactory().AddConsole();
            logFactory.AddFile("Logs/Harmonize-{Date}.log");
            log = logFactory.CreateLogger<Startup>();
        }

        /// <summary>
        /// Adds services to the container.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        { 
            log.LogDebug("Configuring services...");

            // Ignore antiforgery tokens
            services.AddMvc().AddRazorPagesOptions(o =>
             {
                 o.Conventions.ConfigureFilter(new IgnoreAntiforgeryTokenAttribute());
             });

            // Load application settings
            var settingsRawData = File.ReadAllText("appsettings.json");
            if (settingsRawData == null)
            {
                log.LogCritical("Failed to load appsettings.json");
                Environment.Exit(1);
            }

            // Create user database service
            var settings = (JObject)JsonConvert.DeserializeObject(settingsRawData);
            UsersDatabase usersDb = new UsersDatabase(settings["ConnectionStrings"]["Users"].Value<string>(), logFactory);
            if (!usersDb.Open())
            {
                log.LogCritical("Failed to connect to users database");
                Environment.Exit(1);
            }

            // Create email service
            if (!File.Exists("EmailTemplate.json"))
            {
                log.LogCritical("Failed to load email template.");
                Environment.Exit(1);
            }

            string htmlTemplate = File.ReadAllText("EmailTemplate.json");
            EmailService emailService = new EmailService(settings["Keys"]["MailjetAPI"].Value<string>(), 
                settings["Keys"]["MailjetSecret"].Value<string>(), htmlTemplate, logFactory);

            // Create attom data service for house estimates
            AttomData attomDataService = new AttomData(settings["Keys"]["Attom"].Value<string>(), logFactory);

            // Add built-in services
            // Add HttpContextAccessor for IP retrieval
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Add custom services
            services.AddSingleton(logFactory);
            services.AddSingleton(usersDb);
            services.AddSingleton(emailService);
            services.AddSingleton(attomDataService);         
        }

        /// <summary>
        /// Configures the HTTP request pipeline.
        /// </summary>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Upload our static content
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "Content")),
                RequestPath = "/Content"
            });

            app.UseMvc();
        }
    }
}
