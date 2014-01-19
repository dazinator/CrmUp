using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine;
using DbUp.Engine.Output;
using Microsoft.Xrm.Client;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Deployment;
using Microsoft.Xrm.Sdk.Discovery;
using NUnit.Framework;
using Rhino.Mocks;

namespace CrmUp.Tests
{
    [TestFixture]
    public class CrmConnectionManagerTests : SpecificationFor<CrmConnectionManager>
    {
        MockRepository _mockRepos = new MockRepository();
        private IOrganizationService _mockOrgService = null;
        private IDeploymentService _mockDeploymentService = null;
        private IDiscoveryService _mockDiscoveryService = null;


        public override CrmConnectionManager Given()
        {
            // Arrange.
            // Set up our connection manager to use mock Crm services.
            _mockOrgService = MockRepository.GenerateMock<IOrganizationService, IDisposable>();
            _mockDeploymentService = MockRepository.GenerateMock<IDeploymentService, IDisposable>();
            _mockDiscoveryService = MockRepository.GenerateMock<IDiscoveryService, IDisposable>();

            var mockServiceProvider = new FakeCrmServiceProvider(_mockOrgService, _mockDeploymentService,
                                                                _mockDiscoveryService);

            var orgManager = new CrmOrganisationManager(mockServiceProvider);
            return new CrmConnectionManager(mockServiceProvider, orgManager);
        }

        public override void When()
        {
            using (Subject.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>())) { }
        }

        [Test]
        public void Should_Dispose_Connection()
        {
            // assert
            ((IDisposable)_mockOrgService).AssertWasCalled(a => a.Dispose(), options => options.Repeat.Once());
        }

    }


    [TestFixture]
    public class CrmConnectionManager_NoOrganisationTests : SpecificationFor<CrmConnectionManager>
    {
        MockRepository _mockRepos = new MockRepository();
        private IOrganizationService _mockOrgService = null;
        private IDeploymentService _mockDeploymentService = null;
        private IDiscoveryService _mockDiscoveryService = null;
        private ICrmOrganisationManager _mockOrganisationManager = null;

        public override CrmConnectionManager Given()
        {
            // Arrange.
            // Set up our connection manager to use mock Crm services.
            _mockOrgService = MockRepository.GenerateMock<IOrganizationService, IDisposable>();
            _mockDeploymentService = MockRepository.GenerateMock<IDeploymentService, IDisposable>();
            _mockDiscoveryService = MockRepository.GenerateMock<IDiscoveryService, IDisposable>();

            var mockServiceProvider = new FakeCrmServiceProvider(_mockOrgService, _mockDeploymentService,
                                                                _mockDiscoveryService);

            _mockOrganisationManager = MockRepository.GenerateMock<ICrmOrganisationManager, IDisposable>();

            //var orgManager = new CrmOrganisationManager(mockServiceProvider);
            Given_That_There_Is_No_Crm_Organisation();
            return new CrmConnectionManager(mockServiceProvider, _mockOrganisationManager);
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
            When_Executing_A_Rollup_Operation();
        }

        public void When_Asked_To_Ensure_Organisation_Is_Created()
        {
            // Act
            Subject.EnsureOrganisationExists = () =>
            {
                var createOrgParams = new CreateOrganisationParams();
                createOrgParams.Organisation = new Organization();
                createOrgParams.Organisation.UniqueName = "MyNewTestOrg";
                createOrgParams.SystemAdmingUser = "Domain\\User";
                return createOrgParams;
            };
        }

        public void When_Executing_A_Rollup_Operation()
        {
            // Act
            using (Subject.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>())) { }
        }

        [Test]
        public void Should_Create_The_Crm_Organisation()
        {
            // Assert
            OrganizationRequest org = null;
                        _mockOrganisationManager.AssertWasCalled(x => x.CreateOrganization(Arg<Organization>.Matches(y => y.UniqueName == "MyNewTestOrg"), Arg<string>.Is.Equal("Domain\\User"), Arg<IUpgradeLog>.Is.NotNull ));
            
        }

    }


}