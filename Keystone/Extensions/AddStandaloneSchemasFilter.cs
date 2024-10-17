using KeystoneLibrary.Models;
using KeystoneLibrary.Models.Api;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Keystone.Extensions
{
    public class AddStandaloneSchemasFilter : IDocumentFilter
    {
        public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
        {
            // Register each of your special classes
            var schema1 = context.SchemaGenerator.GenerateSchema(typeof(HolidayViewModel), context.SchemaRepository);
            var schema2 = context.SchemaGenerator.GenerateSchema(typeof(SectionSlotWithDetailApiViewModel), context.SchemaRepository);

            //context.SchemaRepository.Schemas.Add("ArrayOfHolidayViewModel", schema1);
            //context.SchemaRepository.Schemas.Add("ArrayOfSectionSlotWithDetailApiViewModel", schema2);

            // Repeat for other classes as needed
        }

    }
    public class EnsureTypesAreIncludedFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Check if the schema is for your dynamic object field
            if (context.Type == typeof(object))
            {
                // Add references to your special classes
                schema.OneOf = new List<OpenApiSchema>
                {
                    new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = nameof(HolidayViewModel) } } },
                    new OpenApiSchema { Type = "array", Items = new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = nameof(SectionSlotWithDetailApiViewModel) } }  }
                    //new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ArrayOfHolidayViewModel" } },
                    //new OpenApiSchema { Reference = new OpenApiReference { Type = ReferenceType.Schema, Id = "ArrayOfSectionSlotWithDetailApiViewModel" } }
                };
            }
        }
    }
}
