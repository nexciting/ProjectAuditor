using System;
using System.Linq;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEditor;

namespace Unity.ProjectAuditor.Editor.AssetsAnalyzers
{
    public class ResourcesAnalyzer : IAssetsAnalyzer
    {
        private static readonly ProblemDescriptor s_Descriptor = new ProblemDescriptor
            (
            302000,
            "Resources folder asset",
            Area.BuildSize,
            "The Resources folder is a common source of many problems in Unity projects. Improper use of the Resources folder can bloat the size of a project’s build, lead to uncontrollable excessive memory utilization, and significantly increase application startup times.",
            "Use AssetBundles when possible"
            );

        public void Initialize(IAuditor auditor)
        {
            auditor.RegisterDescriptor(s_Descriptor);
        }

        public void Analyze(string[] playerAssetPaths, Action<ProjectIssue> onIssueFound)
        {
            var resourceAssetPaths = playerAssetPaths.Where(path => path.IndexOf("/resources/", StringComparison.OrdinalIgnoreCase) >= 0);

            foreach (var path in resourceAssetPaths)
            {
                var location = new Location(path, LocationType.Asset);
                onIssueFound(new ProjectIssue
                    (
                        s_Descriptor,
                        location.Path,
                        IssueCategory.Assets,
                        location
                    )
                );
            }
        }
    }
}
