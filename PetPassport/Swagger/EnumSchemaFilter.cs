using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace PetPassport.Swagger
{
    /// <summary>
    /// Показывает enum в Swagger как "0 (Indefinite), 1 (Upcoming)..." вместо просто цифр.
    /// Не меняет поведение API — числа по-прежнему используются в запросах/ответах.
    /// </summary>
    public class EnumSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (!context.Type.IsEnum)
                return;

            schema.Description = string.Join(", ",
                Enum.GetValues(context.Type)
                    .Cast<object>()
                    .Select(v => $"{(int)v} = {v}"));
        }
    }
}
