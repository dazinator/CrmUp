using CrmUp.Dynamics;
using DbUp.Engine.Output;

namespace CrmUp
{
    public abstract class CrmCodeMigration : ICrmCodeMigration
    {

        public abstract string ScriptName { get; }
        public abstract void Up(ICrmServiceProvider serviceProvider, IUpgradeLog log);
    }
}