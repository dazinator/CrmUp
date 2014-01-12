using System;

namespace CrmUp
{
    public class OrganisationDoesNotExistException : Exception
    {
        public string OrganisationName { get; set; }
        public OrganisationDoesNotExistException(string orgName)
            : base("An organisation named " + orgName + " does not exist on the Crm instance.")
        {
            OrganisationName = orgName;
        }
    }
}