using System;
using System.Collections.Generic;
using System.ServiceModel;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using NUnit.Framework;
using Rhino.Mocks;

namespace CrmUp.Tests
{
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

    [TestFixture]
    public class CrmEntityJournal_NoOrganisationExists : SpecificationFor<CrmEntityJournal>
    {
        MockRepository _mockRepos = new MockRepository();
        private IOrganizationService _fakeConnection;
        private IConnectionManager _fakeConnectionManager = null;
        private ICrmOrganisationManager _mockOrganisationManager = null;
        private string[] _scripts = null;
    
        public override CrmEntityJournal Given()
        {
            // Arrange
            // fake connection to crm.
            _fakeConnection = _mockRepos.CreateMultiMock<IOrganizationService>(typeof(IOrganizationService), typeof(IDisposable));
            _fakeConnection = MockRepository.GenerateMock<IOrganizationService, IDisposable>();
            _fakeConnectionManager = new FakeCrmConnectionManager(_fakeConnection, null, null, null);
            // Log to console.
            var upgradeLog = new ConsoleUpgradeLog();
            _mockOrganisationManager = MockRepository.GenerateMock<ICrmOrganisationManager, IDisposable>();
            Given_That_There_Is_No_Crm_Organisation();

            // Start rollup operation..
            _fakeConnectionManager.OperationStarting(upgradeLog, new List<SqlScript>());
            // Return the test subject (Crm Journal)
            return new CrmEntityJournal(() => { return _fakeConnectionManager; }, () => { return upgradeLog; }, _mockOrganisationManager);
        }

        public void Given_That_There_Is_No_Crm_Organisation()
        {
            //// When our fake discovery service is asked for organisations, it will return a response with no results.
            //var request = new RetrieveOrganizationsRequest();
            //_mockDiscoveryService
            //    .Stub(c => c.Execute(request))
            //    .IgnoreArguments()
            //    .Constraints(global::Rhino.Mocks.Constraints.Is.TypeOf(typeof(RetrieveOrganizationsRequest)))
            //    .Return(new RetrieveOrganizationResponse());


            //var request = new Create();
            _mockOrganisationManager
                .Stub(c => c.GetOrganisations())
                .Return(new List<OrganizationDetail>());
        }

        public override void When()
        {
            // Act
            When_Asked_To_Ensure_Organisation_Is_Created();
            When_Getting_Scripts();
        }

        public void When_Asked_To_Ensure_Organisation_Is_Created()
        {
            // Act
            Subject.EnsureOrganisationExists = () =>
            {
                var createOrgParams = new CreateOrganisationArgs();
                createOrgParams.Organisation = new Organization();
                createOrgParams.Organisation.UniqueName = "MyNewTestOrg";
                createOrgParams.SystemAdmingUser = "Domain\\User";
                return createOrgParams;
            };
        }

        public void When_Getting_Scripts()
        {
            // Act
            _scripts = Subject.GetExecutedScripts();
        }

        [Test]
        public void Should_Create_The_Crm_Organisation()
        {
            // Assert
            OrganizationRequest org = null;
            _mockOrganisationManager.AssertWasCalled(x => x.CreateOrganization(Arg<Organization>.Matches(y => y.UniqueName == "MyNewTestOrg"), Arg<string>.Is.Equal("Domain\\User"), Arg<IUpgradeLog>.Is.NotNull));

        }


    }
}