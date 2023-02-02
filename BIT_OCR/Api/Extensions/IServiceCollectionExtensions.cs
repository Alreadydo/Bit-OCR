using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;

namespace BiT.Central.OCR.Api.Extensions
{
    public static class IServiceCollectionExtensions
    {

        /// <summary>
        ///     Add support Swagger.
        ///         - Reading Swagger config files from the XML docs above routes.
        ///         - Grouping Swagger routes by a common (custom) name
        ///         - NewtonSoft JSON support
        /// </summary>
        /// 
        /// <param name="services"></param>
        /// <param name="apiName"></param>
        /// <param name="apiVersion"></param>
        /// <param name="apiDescription"></param>
        /// <param name="tagByGroupName"></param>
        /// 
        public static void AddBitCentralSwagger(
            this IServiceCollection services,
            string apiName,
            string apiVersion,
            string apiDescription,
            bool tagByGroupName = false)
        {
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc(apiVersion, new OpenApiInfo
                {
                    Title = apiName,
                    Version = apiVersion,
                    Description = apiDescription
                });

                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
                c.EnableAnnotations();

                if (tagByGroupName)
                {
                    c.TagActionsBy(api => new[] { api.GroupName });
                    c.DocInclusionPredicate((_, api) => !string.IsNullOrWhiteSpace(api.GroupName));
                }
            });
            services.AddSwaggerGenNewtonsoftSupport();
        }
    }
}
