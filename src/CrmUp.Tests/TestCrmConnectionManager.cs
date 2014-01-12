using CrmUp;
using DbUp.Engine.Output;
using Microsoft.Xrm.Sdk;

public class TestCrmConnectionManager : CrmConnectionManager
{
    private IOrganizationService _connection = null;
    public TestCrmConnectionManager(IOrganizationService testConnection, string connectionStringOrKey, bool isConnectionStringKey = false)
        : base(connectionStringOrKey, isConnectionStringKey)
    {
        _connection = testConnection;
    }

    public override IOrganizationService CreateConnection(IUpgradeLog upgradeLog)
    {
        return _connection;
    }
}