using CrmUp.Dynamics;
using DbUp.Engine.Output;

namespace CrmUp
{
    public interface ICrmCodeMigration
    {
        void Up(ICrmServiceProvider serviceProvider, IUpgradeLog log);
    }
}