using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine.Output;

namespace CrmUp.Tests.TestScripts.Unmanaged
{
    public class _ImportReferenceData : CrmCodeMigration
    {
        public override void Up(Dynamics.ICrmServiceProvider serviceProvider, IUpgradeLog log)
        {
            var service = serviceProvider.GetOrganisationService();
            using (service as IDisposable)
            {
               throw new NotImplementedException("This migration script has not yet been implemented..");

            }
        }
    }
}
