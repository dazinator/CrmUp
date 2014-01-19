using System;
using System.Collections.Generic;
using System.Data;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;

namespace CrmUp
{

    public class CreateOrganisationParams
    {

        public Organization Organisation { get; set; }
        public string SystemAdmingUser { get; set; }
    }

    public class CrmConnectionManager : IConnectionManager
    {
        private ICrmServiceProvider _CrmServiceProvider = null;
        private IOrganizationService _organizationService = null;
        private ICrmOrganisationManager _orgManager = null;
        private Func<CreateOrganisationParams> _ensureOrganisationExists = null;

        private bool errorOccured = false;

        public CrmConnectionManager(ICrmServiceProvider crmServiceProvider, ICrmOrganisationManager orgManager)
        {
            _CrmServiceProvider = crmServiceProvider;
            _orgManager = orgManager;
            // _ensureOrganisationExists = ensureOrganisationExists;
        }

        public Func<CreateOrganisationParams> EnsureOrganisationExists
        {
            get { return _ensureOrganisationExists; }
            set { _ensureOrganisationExists = value; }
        }

        public IDisposable OperationStarting(IUpgradeLog upgradeLog, List<SqlScript> executedScripts)
        {
            try
            {
                // Before connecting, do we need to ensure organisation exists?
                // Check organisation exists
                if (_ensureOrganisationExists != null)
                {
                    var orgRequest = _ensureOrganisationExists();
                    upgradeLog.WriteInformation("Checking whether '{0}' organization exists..", orgRequest.Organisation.UniqueName);
                    var orgs = _orgManager.GetOrganisations();
                    OrganizationDetail orgFound = null;
                    foreach (var organizationDetail in orgs)
                    {
                        if (organizationDetail.UniqueName.ToLower() == orgRequest.Organisation.UniqueName.ToLower())
                        {
                            orgFound = organizationDetail;
                            upgradeLog.WriteInformation("  - Yes {0} exists!", orgFound.UniqueName);
                            // Use discovery information to establish connection?
                            //_organizationService = _CrmServiceProvider.GetOrganisationService(orgFound, );
                            break;
                        }
                    }

                    if (orgFound == null)
                    {
                        upgradeLog.WriteInformation("Creating organization: {0}", orgRequest.Organisation.UniqueName);
                        _orgManager.CreateOrganization(orgRequest.Organisation, orgRequest.SystemAdmingUser, upgradeLog);
                        upgradeLog.WriteInformation("Organisation Created!");
                    }
                }

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