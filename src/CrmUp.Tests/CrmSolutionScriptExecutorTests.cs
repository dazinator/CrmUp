using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support.SqlServer;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using Rhino.Mocks;

namespace CrmUp.Tests
{

    public class CrmSolutionScriptExecutorTests
    {
        [TestFixture]
        public class CrmSolutionScriptExecutorSpecificationOne : SpecificationFor<CrmSolutionScriptExecutor>
        {
            MockRepository _mockRepos = new MockRepository();
            private SqlScript[] scriptsToExecute;
            private IScriptProvider _scriptProvider;
            private IOrganizationService _fakeConnection;
            private IConnectionManager _fakeConnectionManager = null;

            public override CrmSolutionScriptExecutor Given()
            {
                // Arrange
                // fake connection to crm and get scripts to be executed.
                _fakeConnection = _mockRepos.CreateMultiMock<IOrganizationService>(typeof(IOrganizationService), typeof(IDisposable));
                _fakeConnection = MockRepository.GenerateMock<IOrganizationService, IDisposable>();
                _fakeConnectionManager = new FakeCrmConnectionManager(_fakeConnection, null, null,null);
                var upgradeLog = new ConsoleUpgradeLog();
                _fakeConnectionManager.OperationStarting(upgradeLog, new List<SqlScript>());
                return new CrmSolutionScriptExecutor(() => { return _fakeConnectionManager; },
                    () => { return upgradeLog; },
                    () => { return false; }, null);
            }

            public override void When()
            {
                // Act
                var assembly = Assembly.GetExecutingAssembly();
                _scriptProvider = new EmbeddedCrmSolutionScriptProvider(assembly, s => true);
                scriptsToExecute = _scriptProvider.GetScripts(_fakeConnectionManager).ToArray();
            }

            [Then]
            public void Should_Execute_Multiple_Solution_Imports()
            {
                var scriptCount = scriptsToExecute.Count();
                OrganizationRequest req = null;
                // The Crm Org service should have scriptCount * Requests made - 1 for each solution to apply.
                foreach (var sqlScript in scriptsToExecute)
                {
                    Subject.Execute(sqlScript);
                }
                _fakeConnection.AssertWasCalled(o => o.Execute(Arg<OrganizationRequest>.Is.Anything), options => options.Repeat.Times(scriptCount));
            }
            
            [Then]
            public void Should_Execute_A_Single_Solution_Import()
            {
                const int scriptCount = 1;
                OrganizationRequest req = null;
                Subject.Execute(scriptsToExecute.First());
                _fakeConnection.AssertWasCalled(o => o.Execute(Arg<OrganizationRequest>.Is.Anything), options => options.Repeat.Times(scriptCount));
            }


            //[Then]
            //public void Uses_Variable_Subtitute_Preprocessor_When_Applying_Solutions()
            //{
            //    Assert.Fail();
            //}

            //[Then]
            //public void Does_Not_Use_Variable_Subtitute_Preprocessor_When_Setting_False()
            //{
            //    Assert.Fail();
            //}

            //[Test]
            //public void Logs_Output_When_Configured_To()
            //{
            //    var scriptCount = scriptsToExecute.Count();
            //    OrganizationRequest req = null;
            //    Subject.Execute(scriptsToExecute.First());
            //    _fakeConnection.AssertWasCalled(o => o.Execute(Arg<OrganizationRequest>.Is.Anything), options => options.Repeat.Times(scriptCount));
            //}
        }

    }


}
