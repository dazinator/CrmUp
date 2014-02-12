using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine.Output;

namespace CrmUp.Tests.TestScripts.Unmanaged
{
    public class TestMigration : CrmCodeMigration
    {

        public bool Ran { get; set; }

        public override void Up(Dynamics.ICrmServiceProvider serviceProvider, IUpgradeLog log)
        {
            var service = serviceProvider.GetOrganisationService();
            using (service as IDisposable)
            {
                Ran = true;
            }
        }
    }
}
