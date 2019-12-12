using System.Collections.Generic;
using System.Linq;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Unity.ProjectAuditor.Editor
{
    [SettingAnalyzerAttribute]
    public class GraphicsSettingsAnalyzer : SettingsAnalyzer
    {
        // Edit this to reflect the target platforms for your project
        // TODO - Provide an interface for this, or something
        private readonly BuildTargetGroup[] _buildTargets = {BuildTargetGroup.iOS, BuildTargetGroup.Android, BuildTargetGroup.Standalone};
        private readonly GraphicsTier[] _graphicsTiers = {GraphicsTier.Tier1, GraphicsTier.Tier2, GraphicsTier.Tier3};

        public override string GetEditorWindowName()
        {
            return "Project/Graphics";
        }

        public GraphicsSettingsAnalyzer()
        {
            m_EvalFuncs.Add("GraphicsMixedStandardShaderQuality", delegate
            {
                for (int btIdx = 0; btIdx < _buildTargets.Length; ++btIdx)
                {
                    ShaderQuality ssq = EditorGraphicsSettings.GetTierSettings(_buildTargets[btIdx], _graphicsTiers[0]).standardShaderQuality;
                    for (int tierIdx = 0; tierIdx < _graphicsTiers.Length; ++tierIdx)
                    {
                        TierSettings tierSettings =
                            EditorGraphicsSettings.GetTierSettings(_buildTargets[btIdx], _graphicsTiers[tierIdx]);

                        if (tierSettings.standardShaderQuality != ssq)
                            return true;
                    }
                }
                return false;
            });
            
            m_EvalFuncs.Add("GraphicsUsingForwardRendering", delegate
            {
                for (int btIdx = 0; btIdx < _buildTargets.Length; ++btIdx)
                {
                    for (int tierIdx = 0; tierIdx < _graphicsTiers.Length; ++tierIdx)
                    {
                        TierSettings tierSettings =
                            EditorGraphicsSettings.GetTierSettings(_buildTargets[btIdx], _graphicsTiers[tierIdx]);

                        if (tierSettings.renderingPath == RenderingPath.Forward)
                            return true;
                    }
                }

                return false;
            });
            
            m_EvalFuncs.Add("GraphicsUsingDeferredRendering", delegate
            {
                for (int btIdx = 0; btIdx < _buildTargets.Length; ++btIdx)
                {
                    for (int tierIdx = 0; tierIdx < _graphicsTiers.Length; ++tierIdx)
                    {
                        TierSettings tierSettings =
                            EditorGraphicsSettings.GetTierSettings(_buildTargets[btIdx], _graphicsTiers[tierIdx]);

                        if (tierSettings.renderingPath == RenderingPath.DeferredShading)
                            return true;
                    }
                }

                return false;
            });
        }        
    }
}