using System;

namespace CrmUp
{
    public class FailedToConnectToCrmException : Exception
    {
        public const string FailedToConnectErrorMessage = "Failed to connect to CRM";

        public FailedToConnectToCrmException(Exception innerException)
            : base(FailedToConnectErrorMessage, innerException)
        {

        }
    }
}