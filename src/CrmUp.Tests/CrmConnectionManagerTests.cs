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
    [Category("Connection Manager")]
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

            return new CrmConnectionManager(mockServiceProvider);
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


}