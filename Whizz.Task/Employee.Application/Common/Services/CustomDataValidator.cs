using Employee.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Employee.Application.Common.Services
{
    public class CustomDataValidator : ICustomDataValidator
    {
        private readonly IApplicationDbContext _context;

        public CustomDataValidator(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<(bool IsValid, string? ErrorMessage)> ValidateAsync(
            string? customDataJson,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(customDataJson))
            {
                customDataJson = "{}";
            }

            JsonDocument doc;
            try
            {
                doc = JsonDocument.Parse(customDataJson);
            }
            catch (JsonException ex)
            {
                return (false, $"CustomData is not valid JSON: {ex.Message}");
            }

            if (doc.RootElement.ValueKind != JsonValueKind.Object)
            {
                return (false, "CustomData must be a valid JSON object.");
            }

            var root = doc.RootElement;

            var definitions = await _context.CustomFieldDefinitions
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            if (!definitions.Any())
            {
                return (true, null);
            }

            foreach (var def in definitions)
            {
                bool hasProperty = root.TryGetProperty(def.FieldName, out var prop);

                if (def.IsRequired && (!hasProperty || prop.ValueKind == JsonValueKind.Null || prop.ValueKind == JsonValueKind.Undefined))
                {
                    return (false, $"Required custom field '{def.FieldName}' is missing.");
                }

                if (hasProperty && prop.ValueKind != JsonValueKind.Null)
                {
                    var expectedType = def.DataType.Trim().ToLowerInvariant();

                    switch (expectedType)
                    {
                        case "string":
                        case "text":
                            if (prop.ValueKind != JsonValueKind.String)
                                return (false, $"Custom field '{def.FieldName}' must be of type string.");
                            break;

                        case "number":
                        case "int":
                        case "integer":
                        case "decimal":
                            if (prop.ValueKind != JsonValueKind.Number)
                                return (false, $"Custom field '{def.FieldName}' must be a numeric value.");
                            break;

                        case "boolean":
                        case "bool":
                            if (prop.ValueKind != JsonValueKind.True && prop.ValueKind != JsonValueKind.False)
                                return (false, $"Custom field '{def.FieldName}' must be a boolean (true/false).");
                            break;

                        case "date":
                        case "datetime":
                            if (prop.ValueKind != JsonValueKind.String || !DateTime.TryParse(prop.GetString(), out _))
                                return (false, $"Custom field '{def.FieldName}' must be a valid ISO-8601 date (e.g. YYYY-MM-DD).");
                            break;
                    }
                }
            }

            return (true, null);
        }
    }
}
