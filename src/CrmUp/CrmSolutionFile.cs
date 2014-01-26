using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using DbUp.Engine;

namespace CrmUp
{
    /// <summary>
    /// Single responsbility: Extends the DbUp concept of a "SqlScript" to be a Dynamics Crm Solution File.
    /// </summary>
    public class CrmSolutionFile : SqlScript
    {

        public CrmSolutionFile(string name, Byte[] fileBytes)
            : base(name, Convert.ToBase64String(fileBytes))
        {
            FileBytes = fileBytes;
        }

        /// <summary>
        /// Returns a new Crm Solution file from the given stream.
        /// </summary>
        /// <param name="assembly"></param>
        /// <param name="manifestResourceName"></param>
        /// <returns></returns>
        public static CrmSolutionFile FromEmbeddedResource(Assembly assembly, string manifestResourceName)
        {
            if (string.IsNullOrEmpty(manifestResourceName))
            {
                throw new ArgumentException("manifestResourceName");
            }

            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }

            // Split the resource manifest name into is components, removing path information relating to where it is in assembly. We only want the file name.
            var scriptPathArray = manifestResourceName.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            int segmentCount = scriptPathArray.Count();
            if (segmentCount < 2)
            {
                throw new Exception("Embedded solution files should be named ending '.zip'");
            }
            var fileName = string.Format("{0}.{1}", scriptPathArray[segmentCount - 2], scriptPathArray[segmentCount - 1]);
            var scriptName = System.IO.Path.GetFileNameWithoutExtension(fileName);
            using (var resource = assembly.GetManifestResourceStream(manifestResourceName))
            {
                if (resource == null)
                {
                    throw new ArgumentException("No embedded resource found with the name " + manifestResourceName + " inside assembly: " + assembly.FullName);
                }

                using (var resourceStreamReader = new StreamReader(resource, Encoding.Default, true))
                {
                    return new CrmSolutionFile(scriptName, GetStreamBytes(resourceStreamReader.BaseStream));
                }
            }
        }

        /// <summary>
        /// Returns the Bytes from a stream, as a Byte Array.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        protected static byte[] GetStreamBytes(Stream stream)
        {
            if (stream.CanSeek && stream.Position > 0)
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
            if (stream.Position > 0)
            {
                throw new NotSupportedException("The stream containing the solution file must be at postion 0.");
            }

            var readBuffer = new byte[1024];
            var outputBytes = new List<byte>();

            //int offset = 0;

            while (true)
            {
                int bytesRead = stream.Read(readBuffer, 0, readBuffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                if (bytesRead == readBuffer.Length)
                {
                    outputBytes.AddRange(readBuffer);
                }
                else
                {
                    var tempBuf = new byte[bytesRead];
                    Array.Copy(readBuffer, tempBuf, bytesRead);
                    outputBytes.AddRange(tempBuf);
                    break;
                }
                // offset += bytesRead;
            }
            return outputBytes.ToArray();
        }

        public Byte[] FileBytes { get; set; }

    }
}