using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

namespace CrmUp
{
    /// <summary>
    /// Single responsbility: To provide a fluent API for constructing Crm Entity Metadata.
    /// </summary>
    public class EntityMetadataBuilder
    {

        public EntityMetadata Entity { get; set; }

        public EntityAttributeBuilder AttributeBuilder { get; set; }

        public EntityMetadataBuilder(string entityName)
        {

            //Initialise Meatdata
            Entity = new EntityMetadata
                {
                    LogicalName = entityName.ToLower(),
                    SchemaName = entityName,
                    IsActivity = false,
                    IsActivityParty = false,
                    OwnershipType =  OwnershipTypes.UserOwned
                };

            AttributeBuilder = new EntityAttributeBuilder(this);

        }

        public EntityMetadata Build()
        {
            return Entity;
        }

        public EntityAttributeBuilder WithAttributes()
        {
            return AttributeBuilder;
        }

        public EntityMetadataBuilder IsActivity()
        {
            this.Entity.IsActivity = true;
            return this;
        }

        public EntityMetadataBuilder IsActivityParty()
        {
            this.Entity.IsActivityParty = true;
            return this;
        }

        public EntityMetadataBuilder OwnershipType(OwnershipTypes ownershipType)
        {
            this.Entity.OwnershipType = ownershipType;
            return this;
        }

        public EntityMetadataBuilder Description(string description, int lcid = 1033)
        {
            this.Entity.Description = new Label(description, lcid);
            return this;
        }

        public EntityMetadataBuilder DisplayCollectionName(string displayCollectionName, int lcid = 1033)
        {
            this.Entity.DisplayCollectionName = new Label(displayCollectionName, lcid);
            return this;
        }

        public EntityMetadataBuilder DisplayName(string displayName, int lcid = 1033)
        {
            this.Entity.DisplayName = new Label(displayName, lcid);
            return this;
        }

    }
}