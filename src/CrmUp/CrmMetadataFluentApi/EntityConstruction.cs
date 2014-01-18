namespace CrmUp
{
    public static class EntityConstruction
    {
        public static EntityMetadataBuilder ConstructEntity(string entityName)
        {
            return new EntityMetadataBuilder(entityName);
        }
    }
}