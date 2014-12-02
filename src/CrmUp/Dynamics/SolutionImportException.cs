using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmUp.Dynamics
{
    /// <summary>
    /// Single responsibility: To represent an exception for a failed solution file import.
    /// </summary>
    [Serializable()]
    public class SolutionImportException : Exception
    {
        public const string FailedToImportSolutionMessage = "Solution Import Failed.";

        public string JobLog { get; set; }

        public SolutionImportException(string joblog, string message, Exception innerException)
            : base(message, innerException)
        {
            JobLog = joblog;
        }
        public override string Message
        {
            get
            {
                var builder = new System.Text.StringBuilder();
                builder.AppendLine(FailedToImportSolutionMessage);
                if (!string.IsNullOrEmpty(base.Message))
                {
                    builder.Append(base.Message);
                }
                builder.AppendLine("CRM Job Log: ");
                if (!string.IsNullOrEmpty(JobLog))
                {
                    builder.Append(JobLog);                 
                }               
                return builder.ToString();
            }
        }


    }
}
