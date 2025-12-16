using System;
using Autodesk.Revit.DB;
using Autodesk.Revit.DB.ExtensibleStorage;

namespace BIMKraft.Services
{
    /// <summary>
    /// Service for storing and retrieving line color data using Extensible Storage
    /// </summary>
    public static class LineColorStorageService
    {
        private static readonly Guid SchemaGuid = new Guid("A7B8C9D0-E1F2-4A5B-9C8D-7E6F5A4B3C2D");
        private const string SchemaName = "BIMKraftLineColorData";
        private const string ColorFieldName = "AssignedColor";

        /// <summary>
        /// Gets or creates the schema for storing line color data
        /// </summary>
        private static Schema GetOrCreateSchema()
        {
            Schema schema = Schema.Lookup(SchemaGuid);

            if (schema == null)
            {
                SchemaBuilder schemaBuilder = new SchemaBuilder(SchemaGuid);
                schemaBuilder.SetSchemaName(SchemaName);
                schemaBuilder.SetReadAccessLevel(AccessLevel.Public);
                schemaBuilder.SetWriteAccessLevel(AccessLevel.Public);
                schemaBuilder.SetDocumentation("Stores the assigned color for line elements in the Line Length Calculator tool");

                // Add field for color (stored as hex string)
                FieldBuilder colorField = schemaBuilder.AddSimpleField(ColorFieldName, typeof(string));
                colorField.SetDocumentation("The assigned color in hex format (e.g., #FF5733)");

                schema = schemaBuilder.Finish();
            }

            return schema;
        }

        /// <summary>
        /// Stores a color for a line element
        /// </summary>
        public static void StoreColor(Element element, string colorHex)
        {
            if (element == null || string.IsNullOrEmpty(colorHex))
                return;

            Schema schema = GetOrCreateSchema();
            Entity entity = new Entity(schema);
            entity.Set(ColorFieldName, colorHex);

            element.SetEntity(entity);
        }

        /// <summary>
        /// Retrieves the stored color for a line element
        /// </summary>
        /// <returns>The color hex string, or null if no color is stored</returns>
        public static string GetStoredColor(Element element)
        {
            if (element == null)
                return null;

            Schema schema = Schema.Lookup(SchemaGuid);
            if (schema == null)
                return null;

            Entity entity = element.GetEntity(schema);
            if (entity == null || !entity.IsValid())
                return null;

            try
            {
                return entity.Get<string>(ColorFieldName);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Checks if an element has a stored color
        /// </summary>
        public static bool HasStoredColor(Element element)
        {
            return !string.IsNullOrEmpty(GetStoredColor(element));
        }

        /// <summary>
        /// Removes the stored color from an element
        /// </summary>
        public static void RemoveStoredColor(Element element)
        {
            if (element == null)
                return;

            Schema schema = Schema.Lookup(SchemaGuid);
            if (schema == null)
                return;

            element.DeleteEntity(schema);
        }
    }
}
