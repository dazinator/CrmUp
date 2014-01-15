using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbUp.Engine;
using DbUp.Engine.Output;
using DbUp.Engine.Transactions;

namespace CrmUp
{
    public class CrmSolutionJournal : IJournal
    {
       // private readonly string _SchemaTableName;
        private readonly Func<IConnectionManager> _ConnectionManager;
        private readonly Func<IUpgradeLog> _Log;

        public CrmSolutionJournal(Func<IConnectionManager> connectionManager, Func<IUpgradeLog> logger)
        {
            //this.schemaTableName = SqlObjectParser.QuoteSqlObjectName(table);
            // this.schemaTableName = !string.IsNullOrEmpty(schema) ? SqlObjectParser.QuoteSqlObjectName(schema) + "." + SqlObjectParser.QuoteSqlObjectName(table) : SqlObjectParser.QuoteSqlObjectName(table);
            this._ConnectionManager = connectionManager;
            this._Log = logger;
        }

        public string[] GetExecutedScripts()
        {
            this._Log().WriteInformation("Fetching list of already executed scripts.", new object[0]);
            //if (!this.DoesTableExist())
            //{
            //    this.log().WriteInformation(string.Format("The {0} table could not be found. The database is assumed to be at version 0.", (object)this.schemaTableName), new object[0]);
            //    return new string[0];
            //}
            // else
            //{
            var scripts = new List<string>();
            throw new NotImplementedException();
            //this.connectionManager().ExecuteCommandsWithManagedConnection((Action<Func<IDbCommand>>)(dbCommandFactory =>
            //{
            //    using (IDbCommand resource_1 = dbCommandFactory())
            //    {
            //        resource_1.CommandText = this.GetExecutedScriptsSql(this.schemaTableName);
            //        resource_1.CommandType = CommandType.Text;
            //        using (IDataReader resource_0 = resource_1.ExecuteReader())
            //        {
            //            while (resource_0.Read())
            //                scripts.Add((string)resource_0[0]);
            //        }
            //    }
            //}));
            return scripts.ToArray();
            //  }
        }

        protected virtual string GetExecutedScriptsSql(string table)
        {
            throw new NotImplementedException();
            // return string.Format("select [ScriptName] from {0} order by [ScriptName]", (object)table);
        }

        public void StoreExecutedScript(SqlScript script)
        {
            throw new NotImplementedException();
            //this.connectionManager().ExecuteCommandsWithManagedConnection((Action<Func<IDbCommand>>)(dbCommandFactory =>
            //{
            //    using (IDbCommand resource_0 = dbCommandFactory())
            //    {
            //        resource_0.CommandText = string.Format("insert into {0} (ScriptName, Applied) values (@scriptName, @applied)", (object)this.schemaTableName);
            //        IDbDataParameter local_1 = resource_0.CreateParameter();
            //        local_1.ParameterName = "scriptName";
            //        local_1.Value = (object)script.Name;
            //        resource_0.Parameters.Add((object)local_1);
            //        IDbDataParameter local_2 = resource_0.CreateParameter();
            //        local_2.ParameterName = "applied";
            //        local_2.Value = (object)DateTime.Now;
            //        resource_0.Parameters.Add((object)local_2);
            //        resource_0.CommandType = CommandType.Text;
            //        resource_0.ExecuteNonQuery();
            //    }
            //}));
        }


    }
}
