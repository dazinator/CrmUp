using System;
using Microsoft.Xrm.Client;

namespace CrmUp
{
    /// <summary>
    /// Single responsibility: To represent an exception connecting to a Crm service.
    /// </summary>
    [Serializable()]
    public class FailedToConnectToCrmException : Exception
    {
        public const string FailedToConnectErrorMessage = "Failed to connect to a required CRM service.";

        public CrmConnection Connection { get; set; }

        public FailedToConnectToCrmException(CrmConnection connection, Exception innerException)
            : base(FailedToConnectErrorMessage, innerException)
        {

        }
        public override string Message
        {
            get
            {
                var builder = new System.Text.StringBuilder();
                builder.AppendLine(FailedToConnectErrorMessage);
                if (Connection != null)
                {
                    builder.AppendFormat("Crm Connection String was: {0}", Connection.ServiceUri.ToString());
                    builder.AppendLine();
                }
                builder.Append(base.Message);
                return builder.ToString();

            }
        }


    }
}