using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrmUp
{
    /// <summary>
    /// Describes all the assets that should be deployed by CrmUp.
    /// </summary>
    [Serializable]
    public class DeploymentManifest
    {

        public DeploymentManifest()
        {
            Assemblies = new List<DeploymentAssembly>();
            Steps = new List<DeploymentStep>();
        }

        public List<DeploymentAssembly> Assemblies { get; set; }
        public List<DeploymentStep> Steps { get; set; }

    }

    [Serializable]
    public class DeploymentAssembly
    {
        public string Name { get; set; }
    }

    [Serializable]
    public class DeploymentStep
    {
        public string StepName { get; set; }
        public string SolutionFileName { get; set; }
        public string CodeMigrationScriptName { get; set; }

        public string GetScriptName()
        {
            if (string.IsNullOrWhiteSpace(CodeMigrationScriptName))
            {
                return System.IO.Path.GetFileNameWithoutExtension(SolutionFileName);
            }
            return CodeMigrationScriptName;
        }
    }

}

