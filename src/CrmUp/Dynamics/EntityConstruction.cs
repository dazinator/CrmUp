namespace CrmUp.Dynamics
{
    /// <summary>
    /// Single responsbility: To provide an entrance point into the fluent API for constructing / building metadata for an entity.
    /// </summary>
    public static class EntityConstruction
    {
        public static EntityMetadataBuilder ConstructEntity(string entityName)
        {
            return new EntityMetadataBuilder(entityName);
        }
    }
}