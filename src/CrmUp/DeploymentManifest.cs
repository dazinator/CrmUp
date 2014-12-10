using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace CrmUp
{
    /// <summary>
    /// Describes all the assets that should be deployed by CrmUp.
    /// </summary>
    [Serializable]
    public class DeploymentManifest
    {
        public DeploymentManifest()
        {
            Assemblies = new List<DeploymentAssembly>();
            Steps = new List<DeploymentStep>();
        }

        public List<DeploymentAssembly> Assemblies { get; set; }
        public List<DeploymentStep> Steps { get; set; }

        public static DeploymentManifest FromFile(string filePath)
        {
            var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(DeploymentManifest));
            using (var reader = new StreamReader(filePath))
            {
                var manifest = (DeploymentManifest)serialiser.Deserialize(reader);
                return manifest;
            }
        }

        /// <summary>
        /// Saves the deployment manifest to a file.
        /// </summary>
        /// <param name="fileName"></param>
        public void Save(string fileName)
        {
      
            var xml = ToXml(this, Formatting.Indented);
            using (var writer = new System.IO.StreamWriter(fileName))
            {
                writer.Write(xml);
                writer.Close();
            }
        }

        /// <summary>
        /// Serialises to xml.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="objectToSerialise"></param>
        /// <param name="formatting"></param>
        /// <returns></returns>
        private static string ToXml<T>(T objectToSerialise, Formatting formatting)
        {

            using (var stringWriter = new StringWriter())
            {
                // var serializer = new DataContractSerializer(typeof(T), null, int.MaxValue, false, false, null);

                var serializer = new System.Xml.Serialization.XmlSerializer(typeof(T));
                var writer = new XmlTextWriter(stringWriter)
                {
                    Formatting = formatting
                };
                serializer.Serialize(writer, objectToSerialise);
                writer.Close();
                return stringWriter.ToString();
            }

        }

        public static DeploymentManifest SaveToFile(string filePath)
        {
            var serialiser = new System.Xml.Serialization.XmlSerializer(typeof(DeploymentManifest));
            using (var reader = new StreamReader(filePath))
            {
                var manifest = (DeploymentManifest)serialiser.Deserialize(reader);
                return manifest;
            }
        }

    }

    [Serializable]
    public class DeploymentAssembly
    {
        public string Name { get; set; }
    }

    [Serializable]
    public class DeploymentStep
    {
        public string StepName { get; set; }
        public string SolutionFileName { get; set; }
        public string CodeMigrationScriptName { get; set; }

        public string GetScriptName()
        {
            if (string.IsNullOrWhiteSpace(CodeMigrationScriptName))
            {
                return System.IO.Path.GetFileNameWithoutExtension(SolutionFileName);
            }
            return CodeMigrationScriptName;
        }


    }

}

