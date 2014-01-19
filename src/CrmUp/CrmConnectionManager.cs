using System;
using System.Collections.Generic;
using System.Data;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmUp
{
    public class CrmConnectionManager : IConnectionManager
    {
        private ICrmServiceProvider _CrmServiceProvider = null;
        private IOrganizationService _organizationService = null;
       
      

        private bool errorOccured = false;

        public CrmConnectionManager(ICrmServiceProvider crmServiceProvider)
        {
            _CrmServiceProvider = crmServiceProvider;
        }
    

        public IDisposable OperationStarting(IUpgradeLog upgradeLog, List<SqlScript> executedScripts)
        {
            try
            {
               // Initialise connection to organisation.
                _organizationService = _CrmServiceProvider.GetOrganisationService();
                return new DelegateDisposable(() =>
                {
                    var disposable = _organizationService as IDisposable;
                    if (disposable != null)
                    {
                        disposable.Dispose();
                        _organizationService = null;
                    }
                });
            }
            catch (Exception)
            {
                errorOccured = true;
                throw;
            }

        }

        public virtual void ExecuteWithManagedConnection(Action<Func<IOrganizationService>> action)
        {
            if (errorOccured)
                throw new InvalidOperationException("Error occured on previous script execution");
            try
            {
                action(() => _organizationService);
            }
            catch (Exception)
            {
                errorOccured = true;
                throw;
            }
        }

        public T ExecuteWithManagedConnection<T>(Func<Func<IOrganizationService>, T> actionWithResult)
        {
            if (errorOccured)
                throw new InvalidOperationException("Error occured on previous script execution");
            try
            {
                return actionWithResult(() => _organizationService);
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

        public ICrmServiceProvider CrmServiceProvider
        {
            get { return _CrmServiceProvider; }
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