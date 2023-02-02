using AutoMapper;
using BiT.Central.Core.Extensions;
using BiT.Central.Core.Mvc;
using BiT.Central.OCR.Api.Extensions;
using CognitiveLibrary;
using CognitiveLibrary.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog.Events;
using System;

namespace BiT.Central.OCR.Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        public LogEventLevel LogLevel { get; }

        public const string API_NAME = "BiT-OCR API";
        public const string API_VERSION = "V2.0";
        public const string API_DESCRIPTION = "Analyse pictures through Cognitive Services";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            LogLevel = Enum.Parse<LogEventLevel>(Configuration["Logging:LogLevel:Default"]);
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddBitCentralSerilog(builder =>
            {
                // Determines where to write logs to
                return builder
                    .WriteToConsole()
                    .WriteToAzure(Configuration["ConnectionStrings:ApplicationInsights"])
                    .SetMinimumLogLevel(LogLevel)
                    .Build();
            });

            // Setup documentation at "{api_url}/swagger"
            services.AddBitCentralSwagger(API_NAME, API_VERSION, API_DESCRIPTION, true);

            // Inject support for validating BiT-Central Tokens
            // Reads config data from "Configuration:BiT:Central:Authentication"
            services.AddBitCentralAuthenticationService();

            // Inject support for various BiTCentral services
            // ex: BitCentralErrorFactory, BitCentralValidator, BitCentralEntityRepository
            services.AddBitCentralServices();

            // Inject a connection to the database with EF-Core
            // Reads its config from "ConnectionStrings:EntityDb"
            //services.AddBitCentralEntityDatabase<EntityContext>(Configuration["ConnectionStrings:EntityDb"]);

            services.AddScoped<TableService>();
            services.AddScoped<StoreService>();
            services.AddScoped<SearchService>();
            services.AddScoped<CognitiveAnalysisService>();
            services.AddScoped<RequestAnalyseService>();
            services.AddScoped<BlobService>();

            // Add support for automapper
            // Also reads Automapper configs embedded in BiT.Central.Core
            services.AddAutoMapper(typeof(Startup).Assembly, typeof(BitCentralMappingProfile).Assembly);

            // Inject core functionality, setup is done in the Configure method
            services.AddCors();

            // Bootstrap controllers to their routes
            services.AddControllers()
                .AddBitCentralJsonSettings() // Apply custom JSON parsing rules (null reference handling, casing strategy, etc...)
                .AddBitCentralModelBinders() // Add query parameter binding for BiT-Central specific options like Filters.
                .AddMvcOptions(options =>
                {
                    options.MaxValidationDepth = 4000;
                });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();
            app.UseCors(cors => cors
                .AllowAnyHeader()
                .AllowAnyMethod()
                .SetIsOriginAllowed(_ => true)
                .AllowCredentials()
            );
            app.UseBitCentralSwagger(API_NAME, API_VERSION, false);
            app.UseRouting();
            app.UseAuthorization();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
