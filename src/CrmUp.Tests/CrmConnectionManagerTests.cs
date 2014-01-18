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
using NUnit.Framework;
using Rhino.Mocks;

namespace CrmUp.Tests
{
    [TestFixture]
    public class CrmConnectionManagerTests : SpecificationFor<CrmConnectionManager>
    {
        MockRepository _mockRepos = new MockRepository();
        private IOrganizationService _mockConnection = null;

        public override CrmConnectionManager Given()
        {
            _mockConnection = _mockRepos.CreateMultiMock<IOrganizationService>(typeof(IOrganizationService), typeof(IDisposable));
            _mockConnection = MockRepository.GenerateMock<IOrganizationService, IDisposable>();
            return new FakeCrmConnectionManager(_mockConnection, null, null, null);
        }

        public override void When()
        {
            using (Subject.OperationStarting(new ConsoleUpgradeLog(), new List<SqlScript>())) { }
        }

        [Test]
        public void Should_Dispose_Connection()
        {
            // assert
            ((IDisposable)_mockConnection).AssertWasCalled(a => a.Dispose(), options => options.Repeat.Once());
        }

    }


}