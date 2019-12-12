using UnityEngine;

namespace Unity.ProjectAuditor.Editor
{
    [SettingAnalyzerAttribute]
    public class Physics2DSettingsAnalyzer: SettingsAnalyzer
    {
        public override string GetEditorWindowName()
        {
            return "Project/Physics2D";
        }

        public Physics2DSettingsAnalyzer()
        {
            m_EvalFuncs.Add("Physics2DLayerCollisionMatrix", delegate
            {
                const int NUM_LAYERS = 32;
                for (int i = 0; i < NUM_LAYERS; ++i)
                {
                    for (int j = 0; j < i; ++j)
                    {
                        if (Physics2D.GetIgnoreLayerCollision(i, j))
                        {
                            return false;
                        }
                    } 
                }
                return true;
            });
        }
    }
}