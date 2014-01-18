using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;

// ReSharper disable CheckNamespace 
// Do not change the namespace as we want anyone impprting the dynamics Sdk to get these handy utility extension methods without having
// to set up additional Using / Imports statements.
namespace Microsoft.Xrm.Sdk
// ReSharper restore CheckNamespace
{
    public static class EntityUtils
    {

        /// <summary>Serialize an entity</summary>
        /// <param name="entity">Entity to serialize</param>
        /// <param name="formatting">Formatting, determines if indentation and line feeds are used in the file</param>
        public static string Serialize(this Entity entity, Formatting formatting)
        {

            using (var stringWriter = new StringWriter())
            {

                var serializer = new DataContractSerializer(typeof(Entity), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                var writer = new XmlTextWriter(stringWriter)
                {
                    Formatting = formatting
                };
                serializer.WriteObject(writer, entity);
                writer.Close();
                return stringWriter.ToString();
            }

        }

        /// <summary>
        /// Deserialises the xml into the Entity object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static Entity Deserialize(XmlReader reader)
        {
            var serializer = new DataContractSerializer(typeof(Entity), null, int.MaxValue, false, false, null, new KnownTypesResolver());
            var entity = (Entity)serializer.ReadObject(reader);
            return entity;
        }

    }
}
// ReSharper disable CheckNamespace 
// Do not change the namespace as we want anyone impprting the dynamics Sdk to get these handy utility extension methods without having
// to set up additional Using / Imports statements.
namespace Microsoft.Xrm.Sdk
// ReSharper restore CheckNamespace
{
    public static class EntityMetadataUtils
    {
        /// <summary>Serialize metadata</summary>
        /// <param name="metaData">Metadata to serialize</param>
        /// <param name="formatting">Formatting, determines if indentation and line feeds are used in the file</param>
        public static string SerializeMetaData(this EntityMetadata metaData, Formatting formatting)
        {

            using (var stringWriter = new StringWriter())
            {

                var serializer = new DataContractSerializer(typeof(EntityMetadata), null, int.MaxValue, false, false, null, new KnownTypesResolver());
                var writer = new XmlTextWriter(stringWriter)
                {
                    Formatting = formatting
                };
                serializer.WriteObject(writer, metaData);

                writer.Close();

                return stringWriter.ToString();
            }

        }

        /// <summary>
        /// Deserialises the xml into the EntityMetadata object.
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        public static EntityMetadata DeserializeMetaData(XmlReader reader)
        {
            var serializer = new DataContractSerializer(typeof(EntityMetadata), null, int.MaxValue, false, false, null, new KnownTypesResolver());
            var entity = (EntityMetadata)serializer.ReadObject(reader);
            return entity;
        }

    }
}