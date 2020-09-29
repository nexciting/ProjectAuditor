using System;

namespace Unity.ProjectAuditor.Editor.AssetsAnalyzers
{
    public interface IAssetsAnalyzer
    {
        void Initialize(IAuditor auditor);

        void Analyze(string[] playerAssetPaths, Action<ProjectIssue> onIssueFound);
    }
}
