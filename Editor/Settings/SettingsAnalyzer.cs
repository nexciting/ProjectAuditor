using System.Collections.Generic;
using System.Linq;
using Unity.ProjectAuditor.Editor.Utils;

namespace Unity.ProjectAuditor.Editor
{
    public abstract class SettingsAnalyzer
    {
        protected delegate bool Eval();

        protected Dictionary<string, Eval>  m_EvalFuncs = new Dictionary<string, Eval>();
        public abstract string GetEditorWindowName();

        public ProjectIssue Analyze(ProblemDescriptor descriptor)
        {
            var func = m_EvalFuncs.Where(e => e.Key.Equals(descriptor.customevaluator)).Select(e => e.Value).FirstOrDefault();
            if (func != null && func())
            {
                var projectIssue = new ProjectIssue
                {
                    description = descriptor.description,
                    category = IssueCategory.ProjectSettings,
                    descriptor = descriptor
                };
                projectIssue.location = new Location { path = GetEditorWindowName()};
                return projectIssue;
            }

            return null;
        }
    }
}