﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using CrmUp.Dynamics;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using DbUp.Support.SqlServer;
using Microsoft.Xrm.Sdk;
using NUnit.Framework;
using Rhino.Mocks;

namespace CrmUp.Tests
{
    [Category("Journal")]
    [TestFixture]
    public class CrmEntityJournalTests
    {

        MockRepository _mockRepos = new MockRepository();
        private IOrganizationService _fakeConnection;
        private IConnectionManager _fakeConnectionManager = null;
        private ICrmOrganisationManager _mockOrganisationManager = null;

        [Then]
        public void Should_Create_Journal_Record_When_A_Script_Is_Stored()
        {

            // Arrange
            _fakeConnection = _mockRepos.CreateMultiMock<IOrganizationService>(typeof(IOrganizationService), typeof(IDisposable));
            _fakeConnection = MockRepository.GenerateMock<IOrganizationService, IDisposable>();
            _fakeConnectionManager = new FakeCrmConnectionManager(_fakeConnection, null, null, null);
            var upgradeLog = new ConsoleUpgradeLog();
            _mockOrganisationManager = MockRepository.GenerateMock<ICrmOrganisationManager, IDisposable>();

            _fakeConnectionManager.OperationStarting(upgradeLog, new List<SqlScript>());
            var subject = new CrmEntityJournal(() => { return _fakeConnectionManager; }, () => { return upgradeLog; }, _mockOrganisationManager);

            // Act
            var newScript = new SqlScript("mytest", null);
            subject.StoreExecutedScript(newScript);

            //Assert
            // Verify the IOrganizationService was called to create the journal record and it has correct script name.
            OrganizationRequest org = null;
            _fakeConnection.AssertWasCalled(x => x.Create(Arg<Entity>.Matches(y => y.LogicalName == CrmEntityJournal.JournalEntityName
                && (string)y.Attributes["crmup_scriptname"] == "mytest")));
        }

    }
}
