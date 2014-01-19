using System;
using System.Collections.Generic;
using System.Linq;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;

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
            try
            {
                var log = _LogFactory();
                log.WriteInformation("Fetching list of already executed scripts.");
                var scripts = new List<string>();

                var exists = DoesJournalEntityExist();
                if (!exists)
                {
                    log.WriteInformation(string.Format("The CrmUp journal entity could not be found in Crm. The Crm organisation is assumed to be at version 0."));
                    return new string[0];
                }

                var conn = _ConnectionManagerFactory();
                var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn, "ConnectionManager");
                crmConnManager.ExecuteWithManagedConnection((a) =>
                {
                    var atts = new ColumnSet(new string[] { "crmup_scriptname", "crmup_appliedon" });
                    var querySampleSolution = new QueryExpression
                    {
                        EntityName = JournalEntityName,
                        ColumnSet = atts
                    };
                    var results = a().RetrieveMultiple(querySampleSolution);
                    scripts.AddRange(results.Entities.Select(r => (string)r["crmup_scriptname"]));
                });

                return scripts.ToArray();
            }
            catch (Exception ex)
            {
                _LogFactory().WriteError("Exception has occured checking journal");
                _LogFactory().WriteError(ex.ToString());
                throw;
            }
        }

        public void StoreExecutedScript(SqlScript script)
        {
            var log = _LogFactory();
            EnsureJournalEnityExists();
            var conn = _ConnectionManagerFactory();
            var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn, "ConnectionManager");
            crmConnManager.ExecuteWithManagedConnection((a) =>
                {

                    var ent = new Entity();
                    ent.LogicalName = JournalEntityName;
                    ent.Attributes.Add("crmup_scriptname", script.Name);
                    ent.Attributes.Add("crmup_appliedon", DateTime.UtcNow);
                    var result = a().Create(ent);
                });
        }

        public void EnsureJournalEnityExists()
        {
            // Check Crm metadata to see if entity exists.
            _LogFactory().WriteInformation("Ensuring Crm has Journal Entity..");
            bool exists = DoesJournalEntityExist();
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
                    var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn,
                                                                                                  "ConnectionManager");
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

        public bool DoesJournalEntityExist()
        {
            var retrieveJournalEntityRequest = new RetrieveEntityRequest
                {
                    EntityFilters = EntityFilters.Entity,
                    LogicalName = JournalEntityName
                };

            var conn = _ConnectionManagerFactory();
            var crmConnManager = Guard.EnsureIs<CrmConnectionManager, IConnectionManager>(conn, "ConnectionManager");

            var result = crmConnManager.ExecuteWithManagedConnection<RetrieveEntityResponse>((a) =>
                 {
                     try
                     {
                         var response = (RetrieveEntityResponse)a().Execute(retrieveJournalEntityRequest);
                         return response;
                     }
                     catch (System.ServiceModel.FaultException<Microsoft.Xrm.Sdk.OrganizationServiceFault> fault)
                     {
                         if (fault.Message == "Could not find entity")
                         {
                             return null;
                         }
                         throw;
                     }
                 });

            return result != null;

        }
    }


}