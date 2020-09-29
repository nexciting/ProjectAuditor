using System;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEditor;

namespace Unity.ProjectAuditor.Editor.AssetsAnalyzers
{
    public class ImportersAnalyzer : IAssetsAnalyzer
    {
        private static readonly ProblemDescriptor s_ModelImporterRWDescriptor = new ProblemDescriptor
            (
            302001,
            "Model Importer Read/Write flag is enabled",
            Area.Memory,
            "When a model is Read/Write enabled, Unity keeps two copies of the model in memory at runtime.",
            "The Read/Write flag should be disabled whenever that level of access is not needed."
            );

        private static readonly ProblemDescriptor s_TextureImporterRWDescriptor = new ProblemDescriptor
            (
            302002,
            "Texture Importer Read/Write flag is enabled",
            Area.Memory,
            "When a texture is Read/Write enabled, Unity keeps two copies of the texture in memory at runtime.",
            "The Read/Write flag should be disabled whenever that level of access is not needed."
            );

        public void Initialize(IAuditor auditor)
        {
            auditor.RegisterDescriptor(s_TextureImporterRWDescriptor);
        }

        public void Analyze(string[] playerAssetPaths, Action<ProjectIssue> onIssueFound)
        {
            foreach (var filename in playerAssetPaths)
            {
                var textureImporter = AssetImporter.GetAtPath(filename) as TextureImporter;
                if (textureImporter != null)
                {
                    if (textureImporter.isReadable)
                    {
                        var location = new Location(filename, LocationType.Asset);
                        onIssueFound(new ProjectIssue
                            (
                                s_TextureImporterRWDescriptor,
                                location.Path,
                                IssueCategory.Assets,
                                location
                            )
                        );
                    }
                    continue;
                }
                var modelImporter = AssetImporter.GetAtPath(filename) as ModelImporter;
                if (modelImporter != null)
                {
                    if (modelImporter.isReadable)
                    {
                        var location = new Location(filename, LocationType.Asset);
                        onIssueFound(new ProjectIssue
                            (
                                s_ModelImporterRWDescriptor,
                                location.Path,
                                IssueCategory.Assets,
                                location
                            )
                        );
                    }
                }
            }
        }
    }
}
