using System.Reflection;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Backend.Infrastructure.Filters
{
    public class SwaggerFilter
    {
        public class RemoveVersionFromParameter : IOperationFilter
        {
            /// <summary>
            /// remove version from swagger
            /// </summary>
            /// <param name="operation"></param>
            /// <param name="context"></param>
            public void Apply(OpenApiOperation operation, OperationFilterContext context)
            {
                if (operation.Parameters.Count > 0)
                {
                    var versionParameter = operation.Parameters.SingleOrDefault(p => p.Name == "version");
                    operation.Parameters.Remove(versionParameter);
                }
            }
        }

        /// <summary>
        /// replaced removed swagger version with version from documentation
        /// </summary>
        public class ReplaceVersionWithExactValueInPath : IDocumentFilter
        {
            /// <summary>
            /// Apply document filter to replace version number
            /// </summary>
            /// <param name="swaggerDoc"></param>
            /// <param name="context"></param>
            public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
            {
                var paths = swaggerDoc.Paths;
                swaggerDoc.Paths = new OpenApiPaths();

                foreach (var path in paths)
                {
                    var key = path.Key.Replace("v{version}", swaggerDoc.Info.Version);
                    var value = path.Value;
                    swaggerDoc.Paths.Add(key, value);
                }
            }
        }
    }

    public class SwaggerIgnoreFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (schema?.Properties == null || context.Type == null)
                return;

            var ignoredProperties = context.Type.GetProperties()
                .Where(prop => prop.GetCustomAttribute<SwaggerIgnoreAttribute>() != null);

            foreach (var ignoredProperty in ignoredProperties)
            {
                var jsonPropertyName = ignoredProperty.Name;

                var jsonName = char.ToLowerInvariant(jsonPropertyName[0]) + jsonPropertyName.Substring(1);

                schema.Properties.Remove(jsonName);
            }
        }
    }

    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Parameter)]
    public class SwaggerIgnoreAttribute : Attribute
    {
    }

}
