using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.ProjectAuditor.Editor.AssetsAnalyzers;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEditor;

namespace Unity.ProjectAuditor.Editor.Auditors
{
    public class AssetsAuditor : IAuditor
    {
        private readonly List<IAssetsAnalyzer> m_AssetsAnalyzers = new List<IAssetsAnalyzer>();
        private readonly List<ProblemDescriptor> m_ProblemDescriptors = new List<ProblemDescriptor>();

        public IEnumerable<ProblemDescriptor> GetDescriptors()
        {
            return m_ProblemDescriptors;
        }

        public void Initialize(ProjectAuditorConfig config)
        {
        }

        public void Reload(string path)
        {
            foreach (var type in AssemblyHelper.GetAllTypesInheritedFromInterface<IAssetsAnalyzer>())
                AddAnalyzer(Activator.CreateInstance(type) as IAssetsAnalyzer);
        }

        public void RegisterDescriptor(ProblemDescriptor descriptor)
        {
            m_ProblemDescriptors.Add(descriptor);
        }

        public void Audit(Action<ProjectIssue> onIssueFound, Action onComplete, IProgressBar progressBar = null)
        {
            var allAssetPaths = AssetDatabase.GetAllAssetPaths();
            var playerAssetPaths = allAssetPaths.Where(path => path.IndexOf("/editor/", StringComparison.OrdinalIgnoreCase) == -1).Where(path => (File.GetAttributes(path) & FileAttributes.Directory) != FileAttributes.Directory).ToArray();
            foreach (var analyzer in m_AssetsAnalyzers)
            {
                analyzer.Analyze(playerAssetPaths, onIssueFound);
            }

            onComplete();
        }

        private void AddAnalyzer(IAssetsAnalyzer analyzer)
        {
            analyzer.Initialize(this);
            m_AssetsAnalyzers.Add(analyzer);
        }
    }
}
