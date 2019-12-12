using System.Collections.Generic;
using System.Linq;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEditor;
using UnityEngine;

namespace Unity.ProjectAuditor.Editor
{
    [SettingAnalyzerAttribute]
    public class QualitySettingsAnalyzer : SettingsAnalyzer
    {
        public override string GetEditorWindowName()
        {
            return "Project/Quality";
        }

        public QualitySettingsAnalyzer()
        {
            m_EvalFuncs.Add("QualityUsingDefaultSettings", delegate
            {
                return (QualitySettings.names.Length == 6 &&
                        QualitySettings.names[0] == "Very Low" &&
                        QualitySettings.names[1] == "Low" &&
                        QualitySettings.names[2] == "Medium" &&
                        QualitySettings.names[3] == "High" &&
                        QualitySettings.names[4] == "Very High" &&
                        QualitySettings.names[5] == "Ultra");
            });
            
            m_EvalFuncs.Add("QualityUsingLowQualityTextures", delegate
            {
                bool usingLowTextureQuality = false;
                int initialQualityLevel = QualitySettings.GetQualityLevel();

                for (int i = 0; i < QualitySettings.names.Length; ++i)
                {
                    QualitySettings.SetQualityLevel(i);

                    if (QualitySettings.masterTextureLimit > 0)
                    {
                        usingLowTextureQuality = true;
                        break;
                    }
                }

                QualitySettings.SetQualityLevel(initialQualityLevel);
                return usingLowTextureQuality;
            });

            m_EvalFuncs.Add("QualityDefaultAsyncUploadTimeSlice", delegate
            {
                bool usingDefaultAsyncUploadTimeslice = false;
                int initialQualityLevel = QualitySettings.GetQualityLevel();

                for (int i = 0; i < QualitySettings.names.Length; ++i)
                {
                    QualitySettings.SetQualityLevel(i);

                    if (QualitySettings.asyncUploadTimeSlice == 2)
                    {
                        usingDefaultAsyncUploadTimeslice = true;
                        break;
                    }
                }

                QualitySettings.SetQualityLevel(initialQualityLevel);
                return usingDefaultAsyncUploadTimeslice;
            });
            
            m_EvalFuncs.Add("QualityDefaultAsyncUploadBufferSize", delegate
            {
                bool usingDefaultAsyncUploadBufferSize = false;
                int initialQualityLevel = QualitySettings.GetQualityLevel();
        
                for (int i = 0; i < QualitySettings.names.Length; ++i)
                {
                    QualitySettings.SetQualityLevel(i);

                    if (QualitySettings.asyncUploadBufferSize == 4 || QualitySettings.asyncUploadBufferSize == 16)
                    {
                        usingDefaultAsyncUploadBufferSize = true;
                        break;
                    }
                }
        
                QualitySettings.SetQualityLevel(initialQualityLevel);
                return usingDefaultAsyncUploadBufferSize;
            });
        }        
    }
}