using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Serialization;
using Microsoft.AspNetCore.Mvc.Formatters;
using NLog.Extensions.Logging;
using NLog.Web;
using System.IO;
using CityInfo.API.Services;
using Microsoft.Extensions.Configuration;
using CityInfo.API.Entities;
using Microsoft.EntityFrameworkCore;
using CityInfo.API.Models;
namespace CityInfo.API
{
    public class Startup
    {
        public static IConfiguration Configuration { get; private set; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc()
                //add option for XML outcome - application/xml
                .AddMvcOptions(o => o.OutputFormatters.Add(
                    new XmlDataContractSerializerOutputFormatter()));
            //set Property casing instead of the java llow casing
            //.AddJsonOptions(o =>
            //{
            //if (o.SerializerSettings.ContractResolver != null)
            //    {
            //        var castedResolver = o.SerializerSettings.ContractResolver 
            //        as DefaultContractResolver;
            //        castedResolver.NamingStrategy = null;
            //    }
            //});

            #region NLog
            //needed for NLog.Web
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            #endregion
            //services.AddTransient - created each time they requested - for lightweight
            //services.AddScoped - created once per request
            //services.AddSingleton - first time they requested
#if DEBUG
            services.AddTransient<IMailService, LocalMailService>();
#else    
            services.AddTransient<IMailService, CloudMailService>();
#endif
            var connectionString = Configuration["connectionStrings:cityInfoDBConnectionStringLoc"];
            services.AddDbContext<CityInfoContext>(o=>o.UseSqlServer(connectionString));

            services.AddScoped<ICityInfoRepository, CityInfoRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory,
            CityInfoContext cityInfoContext)
        {
            loggerFactory.AddConsole();

            loggerFactory.AddDebug();

#region NLog
            //add NLog to .NET Core
            loggerFactory.AddNLog();

            ////Enable ASP.NET Core features (NLog.web) - only needed for ASP.NET Core users
            //app.AddNLogWeb();

            //needed for non-NETSTANDARD platforms: configure nlog.config in your project root. NB: you need NLog.Web.AspNetCore package for this. 
            env.ConfigureNLog(Path.Combine(env.ContentRootPath, "nlog.config"));
#endregion

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler();
            }

            cityInfoContext.EnsureSeedDataForContext();

            app.UseStatusCodePages();
            AutoMapper.Mapper.Initialize(cfg =>
            {
                cfg.CreateMap<City, CityWithoutPointsOfInterestDto>();
                cfg.CreateMap<City, CityDto>();
                cfg.CreateMap<PointOfInterest, PointOfInterestDto>();
                cfg.CreateMap<PointOfInterestDto, PointOfInterest>();
                cfg.CreateMap<PointOfInterestForCreationDto, PointOfInterest>();
                cfg.CreateMap<PointOfInterestForUpdateDto, PointOfInterest>();
                cfg.CreateMap<PointOfInterest, PointOfInterestForUpdateDto>();


            });
            app.UseMvc();

            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}
