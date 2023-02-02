using Microsoft.AspNetCore.Builder;

namespace BiT.Central.OCR.Api.Extensions
{
    public static class IApplicationBuilderExtensions
    {

        /// <summary>
        ///      Configure the API to use custom swagger settings and templates.
        /// </summary>
        /// 
        /// <param name="app">The IApplicationBuilder instance to extend</param>
        /// <param name="apiName">The name of the API</param>
        /// <param name="apiVersion">The version of the API</param>
        /// <param name="showModels">Wheter to show the model section at the bottom of the page or not, default is false.</param>
        /// 
        public static void UseBitCentralSwagger(
            this IApplicationBuilder app,
            string apiName,
            string apiVersion,
            bool showModels = false)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint($"/swagger/{apiVersion}/swagger.json", $"{apiName} ({apiVersion})");
                options.InjectStylesheet("/swagger/custom.css");
                options.InjectJavascript("/swagger/custom.js");
                options.SupportedSubmitMethods();

                if (!showModels)
                {
                    options.DefaultModelsExpandDepth(-1);
                }
            });
        }
    }
}
