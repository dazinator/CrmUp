using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using DbUp.Engine;

namespace CrmUp
{
    public class CrmSolutionFile : SqlScript
    {
        public CrmSolutionFile(string name, Byte[] fileBytes)
            : base(name, Convert.ToBase64String(fileBytes))
        {
        }

        /// <summary>
        /// Returns a new Crm Solution file from the given stream.
        /// </summary>
        /// <param name="scriptName">The name of this script.</param>
        /// <param name="stream">The stream containing solution file data.</param>
        /// <returns></returns>
        public new static CrmSolutionFile FromStream(string scriptName, Stream stream)
        {
            using (var resourceStreamReader = new StreamReader(stream, Encoding.Default, true))
            {
                string c = resourceStreamReader.ReadToEnd();
                return new CrmSolutionFile(scriptName, GetStreamBytes(stream));
            }
        }

        /// <summary>
        /// Returns the Bytes from a stream, as a Byte Array.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        public static byte[] GetStreamBytes(Stream stream)
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
    }
}