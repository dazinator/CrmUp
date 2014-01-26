using System;
using System.Collections.Generic;
using System.ServiceModel;
using CrmUp.Dynamics;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NUnit.Framework;
using Rhino.Mocks;

namespace CrmUp.Tests
{
     [Category("Journal")]
    [TestFixture]
    public class CrmEntityJournal_NoJournalEntityMetadataTests : SpecificationFor<IJournal>
    {
        MockRepository _mockRepos = new MockRepository();
        private IOrganizationService _fakeConnection;
        private IConnectionManager _fakeConnectionManager = null;
        private ICrmOrganisationManager _mockOrganisationManager = null;

        public override IJournal Given()
        {
            // Arrange
            // fake connection to crm.
            _fakeConnection = _mockRepos.CreateMultiMock<IOrganizationService>(typeof(IOrganizationService), typeof(IDisposable));
            _fakeConnection = MockRepository.GenerateMock<IOrganizationService, IDisposable>();
            _fakeConnectionManager = new FakeCrmConnectionManager(_fakeConnection, null, null, null);
            // Log to console.
            var upgradeLog = new ConsoleUpgradeLog();
            _mockOrganisationManager = MockRepository.GenerateMock<ICrmOrganisationManager, IDisposable>();

            // Start rollup operation..
            _fakeConnectionManager.OperationStarting(upgradeLog, new List<SqlScript>());
            // Return the test subject (Crm Journal)
            return new CrmEntityJournal(() => { return _fakeConnectionManager; }, () => { return upgradeLog; }, _mockOrganisationManager);
        }

        public override void When()
        {
            When_Crm_Does_Not_Have_Journal_Entity_Metadata();
        }

        public void When_Crm_Does_Not_Have_Journal_Entity_Metadata()
        {
            // When request is made for journal entity - no such entity is found found!
            var orgFault = new Microsoft.Xrm.Sdk.OrganizationServiceFault();
            orgFault.Message = "Could not find entity";
            var fault = new System.ServiceModel.FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault>(orgFault, new FaultReason(orgFault.Message));

            var retrieveJournalEntityRequest = new RetrieveEntityRequest()
                {
                    EntityFilters = EntityFilters.Entity,
                    LogicalName = CrmEntityJournal.JournalEntityName
                };
            //  Arg<System.Linq.Expressions.Expression<Func<Entities.Stage, bool>>>.Is.Equal(StartParam)
            // var b = Arg<OrganizationRequest>.Is.TypeOf(typeof(RetrieveEntityRequest));

            // retrieveJournalEntityRequest

            _fakeConnection
                .Stub(c => c.Execute(retrieveJournalEntityRequest))
                .IgnoreArguments().Constraints(global::Rhino.Mocks.Constraints.Is.TypeOf(typeof(RetrieveEntityRequest)))
                .Throw(fault);

        }
        
        [Then]
        public void Should_Return_Zero_Scripts()
        {
            // Verify the IOrganizationService was called with a request to create the journal entity.
            var scripts = Subject.GetExecutedScripts();
            Assert.That(scripts.Length, Is.EqualTo(0));
        }

        [Then]
        public void Should_Create_Journal_Entity_Metadata_When_A_Script_Is_Stored()
        {
            var newScript = new SqlScript("mytest", null);
            Subject.StoreExecutedScript(newScript);
            // Verify the IOrganizationService was called with a request to create the journal entity.
            OrganizationRequest org = null;
            _fakeConnection.AssertWasCalled(x => x.Execute(Arg<CreateEntityRequest>.Matches(y => y.Entity.LogicalName == CrmEntityJournal.JournalEntityName)));
        }



    }
}