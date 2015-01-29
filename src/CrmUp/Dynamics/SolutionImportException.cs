using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
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

        //public string JobLog { get; set; }

        public SolutionImportException(string message, Exception innerException)
            : base(message, innerException)
        {
           // JobLog = joblog;
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

                if (this.InnerException != null)
                {
                    WriteInnerException(builder, this.InnerException);
                };
                return builder.ToString();
            }
        }

        private void WriteFault(StringBuilder builder, Microsoft.Xrm.Sdk.OrganizationServiceFault faultEx)
        {
            builder.AppendLine("Microsoft.Xrm.Sdk.OrganizationServiceFault ");

            builder.AppendFormat("Timestamp: {0}", faultEx.Timestamp);
            builder.Append(Environment.NewLine);

            builder.AppendFormat("Error Code: {0}", faultEx.ErrorCode);
            builder.Append(Environment.NewLine);           
          
            builder.AppendFormat("Message: {0}", faultEx.Message);
            builder.Append(Environment.NewLine);

            builder.AppendFormat("Trace: {0}", faultEx.TraceText);
            builder.Append(Environment.NewLine);           

            builder.AppendFormat("Inner Fault: {0}", null == faultEx.InnerFault ? "No Inner Fault" : "Has Inner Fault");
            builder.Append(Environment.NewLine);

            if (faultEx.InnerFault != null)
            {
                WriteFault(builder, faultEx.InnerFault);
            }

        }

        private void WriteInnerException(StringBuilder builder, Exception ex)
        {
            builder.AppendLine("Inner Exception Details: ");
            if (ex is FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>)
            {
                var faultEx = ex as FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>;
                WriteFault(builder, faultEx.Detail);
                return;
            }

            if (ex is System.TimeoutException)
            {
                var timeoutEx = ex as System.TimeoutException;
                WriteTimeoutException(builder, timeoutEx);
                return;
            }

            WriteException(builder, ex);
        }

        private void WriteException(StringBuilder builder, Exception ex)
        {
            builder.AppendLine(ex.GetType().Name);

            builder.AppendFormat("Message: {0}", ex.Message);
            builder.Append(Environment.NewLine);

            builder.AppendFormat("Stack Trace: {0}", ex.StackTrace);
            builder.Append(Environment.NewLine);

            builder.AppendFormat("Inner Exception: {0}", null == ex.InnerException.Message ? "No Inner Exception" : ex.InnerException.Message);
            builder.Append(Environment.NewLine);

            if (ex.InnerException != null)
            {
                WriteInnerException(builder, ex.InnerException);
            }
            return;
        }

        private void WriteTimeoutException(StringBuilder builder, TimeoutException timeoutEx)
        {
            builder.AppendLine("System.TimeoutException.");
            
            builder.AppendFormat("Message: {0}", timeoutEx.Message);
            builder.Append(Environment.NewLine);

            builder.AppendFormat("Stack Trace: {0}", timeoutEx.StackTrace);
            builder.Append(Environment.NewLine);

            builder.AppendFormat("Inner Exception: {0}", null == timeoutEx.InnerException.Message ? "No Inner Exception." : timeoutEx.InnerException.Message);
            builder.Append(Environment.NewLine);

            if (timeoutEx.InnerException != null)
            {
                WriteInnerException(builder, timeoutEx.InnerException);
            }

        }


    }
}
