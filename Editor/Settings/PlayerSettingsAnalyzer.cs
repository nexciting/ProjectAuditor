using System.Linq;
using UnityEditor;
using UnityEngine.Rendering;

namespace Unity.ProjectAuditor.Editor
{
    [SettingAnalyzerAttribute]
    public class PlayerSettingsAnalyzer:  SettingsAnalyzer
    {
        public override string GetEditorWindowName()
        {
            return "Project/Player";
        }

        public PlayerSettingsAnalyzer()
        {
            m_EvalFuncs.Add("PlayerSettingsGraphicsAPIs_iOS_OpenGLES", () =>
            {            
                var graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.iOS);
               
                var hasMetal = graphicsAPIs.Contains(GraphicsDeviceType.Metal); 
                
                return !hasMetal;
            });
                
            m_EvalFuncs.Add("PlayerSettingsGraphicsAPIs_iOS_OpenGLESAndMetal", () =>
            {            
                var graphicsAPIs = PlayerSettings.GetGraphicsAPIs(BuildTarget.iOS);
    
                var hasOpenGLES = graphicsAPIs.Contains(GraphicsDeviceType.OpenGLES2) ||
                                  graphicsAPIs.Contains(GraphicsDeviceType.OpenGLES3);  
                
                return graphicsAPIs.Contains(GraphicsDeviceType.Metal) && hasOpenGLES;
            });
            
            m_EvalFuncs.Add("PlayerSettingsArchitecture_Android", () =>
            {
                return (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARMv7) != 0 &&
                       (PlayerSettings.Android.targetArchitectures & AndroidArchitecture.ARM64) != 0;
            });
        }
    }
}