using System;
using CrmUp.Dynamics;
using DbUp.Engine;
using DbUp.Engine.Output;

namespace CrmUp
{
    public class CrmCodeMigrationScript : SqlScript
    {

        private CrmCodeMigration _Migration = null;

        public CrmCodeMigrationScript(string name, string contents)
            : base(name, contents)
        {
        }

        public CrmCodeMigrationScript(CrmCodeMigration migration)
            : base(migration.GetType().FullName, string.Empty)
        {
            _Migration = migration;
        }

        public void Apply(ICrmServiceProvider serviceProvider, IUpgradeLog log)
        {
            _Migration.Up(serviceProvider, log);
        }

    }
}