using System;

namespace CrmUp
{
    public interface ISupportOrgCreation
    {
        Func<CreateOrganisationArgs> EnsureOrganisationExists { get; set; }
    }
}