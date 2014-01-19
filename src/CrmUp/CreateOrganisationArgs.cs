using Microsoft.Xrm.Sdk.Deployment;

namespace CrmUp
{
    public class CreateOrganisationArgs
    {

        public Organization Organisation { get; set; }
        public string SystemAdmingUser { get; set; }
    }
}