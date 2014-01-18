using System;
using System.Linq;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;

namespace CrmUp
{
    public class CrmEntityJournal : IJournal
    {

        public const string JournalEntityName = "crmup_journal";


        // private readonly string schemaTableName;
        private readonly Func<IConnectionManager> _ConnectionManagerFactory;
        private readonly Func<IUpgradeLog> _LogFactory;

        public CrmEntityJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger)
        {
            this._ConnectionManagerFactory = connectionManager;
            _LogFactory = logger;
        }

        public string[] GetExecutedScripts()
        {
            throw new NotImplementedException();
        }

        public void StoreExecutedScript(SqlScript script)
        {
            throw new NotImplementedException();
        }

        public void EnsureJournalEnityExists()
        {
            // Check Crm metadata to see if entity exists.
            bool exists = false;
            if (!exists)
            {

                _LogFactory().WriteInformation("Creating journal entity in Crm..");

                var journalEntityBuilder = EntityConstruction.ConstructEntity(JournalEntityName)
                                                             .DisplayName("Crm Up Journal Entry")
                                                             .Description(
                                                                 "Holds journal entrues regarding upgrades made by CrmUp.")
                                                             .DisplayCollectionName("CrmUp Journal")
                                                             .WithAttributes()
                                                             .StringAttribute("crmup_scriptname", "Script Name",
                                                                              "The name of the script that was applied by CrmUp.",
                                                                              AttributeRequiredLevel.ApplicationRequired,
                                                                              255, StringFormat.Text)
                                                             .DateTimeAttribute("crmup_appliedon", "Applied On",
                                                                                "The date the script was applied.",
                                                                                AttributeRequiredLevel
                                                                                    .ApplicationRequired,
                                                                                DateTimeFormat.DateAndTime,
                                                                                ImeMode.Disabled);


                // This is the metadata for the all of the atributes we defined:
                var attributesMetadata = journalEntityBuilder.Attributes;
                // This is the metadata for the entity we defined:
                var entityMetadata = journalEntityBuilder.MetaDataBuilder.Entity;

                // Create the entity and then add in aatributes.
                var createrequest = new CreateEntityRequest();
                createrequest.Entity = entityMetadata;
                createrequest.PrimaryAttribute = attributesMetadata.First() as StringAttributeMetadata;

                try
                {
                    var conn = _ConnectionManagerFactory();
                    var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn, "ConnectionManager");
                    crmConnManager.ExecuteWithManagedConnection((a) =>
                        {
                            a().Execute(createrequest);

                            _LogFactory().WriteInformation("Journal entity created, adding attributes..");
                            // Now add in attributes as Crm doesnt let us do this in one go.
                            foreach (var attributeMetadata in attributesMetadata)
                            {
                                // Create the request.
                                var createAttributeRequest = new CreateAttributeRequest
                                    {
                                        EntityName = entityMetadata.LogicalName,
                                        Attribute = attributeMetadata
                                    };

                                // Execute the request.
                                a().Execute(createAttributeRequest);
                            }
                            _LogFactory().WriteInformation("Journal entity attributes added..");
                        });
                }
                catch (Exception ex)
                {
                    _LogFactory().WriteError("Exception has occured creating journal entity");
                    _LogFactory().WriteError(ex.ToString());
                    throw;
                }
            }
        }
    }
}