using System;
using System.Collections.Generic;
using System.Data;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Sdk;

namespace CrmUp
{
    public class CrmConnectionManager : IConnectionManager
    {

        private IOrganizationService _UpgradeConnection;
        private string _ConnectionStringKey;
        private bool _IsConnectionStringKey;
        private bool errorOccured = false;
        public CrmConnectionManager(string connectionStringOrKey, bool isConnectionStringKey = false)
        {
            _ConnectionStringKey = connectionStringOrKey;
            _IsConnectionStringKey = isConnectionStringKey;
        }

        public IDisposable OperationStarting(IUpgradeLog upgradeLog, List<SqlScript> executedScripts)
        {
            _UpgradeConnection = CreateConnection(upgradeLog);

            return new DelegateDisposable(() =>
                {
                    var disposable = _UpgradeConnection as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                        _UpgradeConnection = null;
                    }
                });
        }

        public virtual IOrganizationService CreateConnection(IUpgradeLog upgradeLog)
        {
            var factory = new OrganizationProxyFactory(_ConnectionStringKey, _IsConnectionStringKey);
            return factory.CreateOrganizationServiceProxy();
        }

        public virtual void ExecuteWithManagedConnection(Action<Func<IOrganizationService>> action)
        {
            if (errorOccured)
                throw new InvalidOperationException("Error occured on previous script execution");
            try
            {
                action(() => _UpgradeConnection);
            }
            catch (Exception)
            {
                errorOccured = true;
                throw;
            }
        }
        
        public void ExecuteCommandsWithManagedConnection(Action<Func<IDbCommand>> action)
        {
            throw new NotImplementedException();
        }

        public T ExecuteCommandsWithManagedConnection<T>(Func<Func<IDbCommand>, T> actionWithResult)
        {
            throw new NotImplementedException();
        }

        public virtual IEnumerable<string> SplitScriptIntoCommands(string scriptContents)
        {
            // Nothing to do..
            // only ever one *command*
            return new string[] { scriptContents };
        }

        public TransactionMode TransactionMode { get; set; }
        public bool IsScriptOutputLogged { get; set; }

        class DelegateDisposable : IDisposable
        {
            private readonly Action dispose;

            public DelegateDisposable(Action dispose)
            {
                this.dispose = dispose;
            }

            public void Dispose()
            {
                dispose();
            }
        }

    }
}