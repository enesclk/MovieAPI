using Case.Core.Interfaces;
using Case.Core.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Case.Data.Model.IOptionsModels;
using Hangfire;
using Hangfire.Common;
//using MovieAPI_CaseStudy.Middleware;
using Microsoft.AspNetCore.Authentication;
using MovieAPI_CaseStudy.Middleware;

namespace MovieAPI_CaseStudy
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
            services.AddTransient<IMovieOperations, MovieOperations>();
            services.AddTransient<INotificationService, MailService>();
            services.Add(ServiceDescriptor.Singleton<IDistributedCache, RedisCache>());

            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = "localhost:6379"; //Redis server:port goes here...
            });

            services.AddHangfire(configuration => configuration.UseRedisStorage());
            services.AddHangfireServer();
            //GlobalConfiguration.Configuration.UseRedisStorage();

            services.Configure<MailSettings>(Configuration.GetSection("MailSettings"));

            services.AddControllers();

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "MovieAPI_CaseStudy", Version = "v1" });
                c.AddSecurityDefinition("basic", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "basic",
                    In = ParameterLocation.Header,
                    Description = "Basic Authorization header using the Bearer scheme."
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "basic"
                                }
                            },
                            new string[] {}
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IRecurringJobManager recurringJobManager, IServiceProvider serviceProvider)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "MovieAPI_CaseStudy v1"));
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            //app.UseAuthorization();

            app.UseHangfireDashboard("/hangfire");
            recurringJobManager.AddOrUpdate("Fetching movies every hour", () => serviceProvider.GetService<IMovieOperations>().FetchMoviesFromAPI(), "0 * * * *");

            app.UseMiddleware<BasicAuthMiddleware>();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            //RecurringJob.AddOrUpdate<IMovieOperations>(x => x.FetchMoviesFromAPI(), Cron.Hourly);
            //backgroundJobClient.Enqueue<IMovieOperations>(x => x.FetchMoviesFromAPI());            
        }
    }
}
