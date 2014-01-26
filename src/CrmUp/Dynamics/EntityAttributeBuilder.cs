using System.Collections.Generic;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace CrmUp.Dynamics
{
    /// <summary>
    /// Single responsbility: To provide a fluent API for constructing attribute metadata for an entity.
    /// </summary>
    public class EntityAttributeBuilder
    {
        public EntityMetadataBuilder MetaDataBuilder { get; set; }

        public List<AttributeMetadata> Attributes { get; set; }

        public EntityAttributeBuilder(EntityMetadataBuilder metadataBuilder)
        {
            MetaDataBuilder = metadataBuilder;
            Attributes = new List<AttributeMetadata>();
        }


        public EntityAttributeBuilder StringAttribute(string schemaName,  string displayName, string description,
                                                               AttributeRequiredLevel requiredLevel,
                                                               int maxLength, StringFormat format)
        {
            // Define the primary attribute for the entity
            var newAtt = new StringAttributeMetadata
            {
                SchemaName = schemaName,
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                MaxLength = maxLength,
                Format = format,
                DisplayName = new Label(displayName, 1033),
                Description = new Label(description, 1033)
            };
            this.Attributes.Add(newAtt);
            return this;
        }

        public EntityAttributeBuilder BooleanAttribute(string schemaName,
                                                                 AttributeRequiredLevel requiredLevel,
                                                                 int maxLength, StringFormat format,
                                                                 string displayName, string description)
        {
            int languageCode = 1033;
            // Create a boolean attribute
            var boolAttribute = new BooleanAttributeMetadata
            {
                // Set base properties
                SchemaName = schemaName,
                DisplayName = new Label(displayName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, languageCode),
                // Set extended properties
                OptionSet = new BooleanOptionSetMetadata(
                    new OptionMetadata(new Label("True", languageCode), 1),
                    new OptionMetadata(new Label("False", languageCode), 0)
                    )
            };
            this.Attributes.Add(boolAttribute);
            return this;
        }

        public EntityAttributeBuilder DateTimeAttribute(string schemaName, string displayName, string description,
                                                                 AttributeRequiredLevel requiredLevel,
                                                                 DateTimeFormat format, ImeMode imeMode)
        {
            int languageCode = 1033;
            // Create a date time attribute
            var dtAttribute = new DateTimeAttributeMetadata
            {
                // Set base properties
                SchemaName = schemaName,
                DisplayName = new Label(displayName, languageCode),
                RequiredLevel = new AttributeRequiredLevelManagedProperty(requiredLevel),
                Description = new Label(description, languageCode),
                // Set extended properties
                Format = format,
                ImeMode = imeMode
            };
            this.Attributes.Add(dtAttribute);
            return this;
        }

    }
}