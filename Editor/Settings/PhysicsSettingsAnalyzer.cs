using UnityEngine;

namespace Unity.ProjectAuditor.Editor
{
    [SettingAnalyzerAttribute]
    public class PhysicsSettingsAnalyzer: SettingsAnalyzer
    {
        public override string GetEditorWindowName()
        {
            return "Project/Physics";
        }

        public PhysicsSettingsAnalyzer()
        {
            m_EvalFuncs.Add("Physics2DLayerCollisionMatrix", delegate
            {
                const int NUM_LAYERS = 32;
                for (int i = 0; i < NUM_LAYERS; ++i)
                {
                    for (int j = 0; j < i; ++j)
                    {
                        if (Physics.GetIgnoreLayerCollision(i, j))
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