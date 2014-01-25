using Microsoft.Xrm.Sdk.Deployment;

namespace CrmUp
{
    /// <summary>
    /// Single responsbility: To provide the arguments necessary for ensuring creation of an Organisation in Crm.
    /// </summary>
    public class CreateOrganisationArgs
    {

        public Organization Organisation { get; set; }
        public string SystemAdmingUser { get; set; }
    }
}