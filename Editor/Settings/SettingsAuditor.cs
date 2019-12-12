using System;
using System.Collections.Generic;
using System.Linq;
using Unity.ProjectAuditor.Editor.Utils;
using UnityEditor.Macros;

namespace Unity.ProjectAuditor.Editor
{
    public class SettingsAuditor : IAuditor
    {
        private System.Reflection.Assembly[] m_Assemblies;
        
        private List<SettingsAnalyzer> m_SettingAnalyzers = new List<SettingsAnalyzer>();
        private List<ProblemDescriptor> m_ProblemDescriptors;
        private List<KeyValuePair<string, string>> m_ProjectSettingsMapping = new List<KeyValuePair<string, string>>();           
        
        public SettingsAuditor()
        {
            m_Assemblies = AppDomain.CurrentDomain.GetAssemblies();
            m_ProjectSettingsMapping.Add(new KeyValuePair<string, string>("UnityEditor.Physics", "Physics"));
            m_ProjectSettingsMapping.Add(new KeyValuePair<string, string>("UnityEditor.Physics2D", "Physics2D"));
            m_ProjectSettingsMapping.Add(new KeyValuePair<string, string>("UnityEngine.Time", "Time"));
            m_ProjectSettingsMapping.Add(new KeyValuePair<string, string>("UnityEngine.QualitySettings", "QualitySettings"));
            m_ProjectSettingsMapping.Add(new KeyValuePair<string, string>("UnityEditor.PlayerSettings", "Player"));
            m_ProjectSettingsMapping.Add(new KeyValuePair<string, string>("UnityEditor.Rendering.EditorGraphicsSettings", "Graphics"));
        }

        public string GetUIName()
        {
            return "Settings";
        }

        public IEnumerable<ProblemDescriptor> GetDescriptors()
        {
            return m_ProblemDescriptors;
        }
        
        public void LoadDatabase(string path)
        {
             m_ProblemDescriptors = ProblemDescriptorHelper.LoadProblemDescriptors( path, "ProjectSettings");
             
             var assemblies = AppDomain.CurrentDomain.GetAssemblies();
             foreach (var assembly in assemblies)
             {
                 foreach (var type in GetAnalyzerTypes(assembly))
                 {
                     m_SettingAnalyzers.Add(Activator.CreateInstance(type)  as SettingsAnalyzer);
                 }
             }
        }

        public IEnumerable<Type> GetAnalyzerTypes(System.Reflection.Assembly assembly) {
            foreach(Type type in assembly.GetTypes()) {
                if (type.GetCustomAttributes(typeof(SettingAnalyzerAttribute), true).Length > 0) {
                    yield return type;
                }
            }
        }

        public void RegisterDescriptor(ProblemDescriptor descriptor)
        {
            m_ProblemDescriptors.Add(descriptor);
        }

        public void Audit(ProjectReport projectReport, IProgressBar progressBar = null)
        {
            if (progressBar != null)
                progressBar.Initialize("Analyzing Settings", "Analyzing project settings", m_ProblemDescriptors.Count);

            foreach (var descriptor in m_ProblemDescriptors)
            {
                if (progressBar != null)
                    progressBar.AdvanceProgressBar();

                SearchAndEval(descriptor, projectReport);
            }
            if (progressBar != null)
                progressBar.ClearProgressBar();
        }

        private void AddIssue(ProblemDescriptor descriptor, string description,  string editorWindowName, ProjectReport projectReport)
        {
            projectReport.AddIssue(new ProjectIssue
            {
                description = description,
                category = IssueCategory.ProjectSettings,
                descriptor = descriptor,
                location = new Location {path = editorWindowName}
            });
            
        }
        
        private void SearchAndEval(ProblemDescriptor descriptor, ProjectReport projectReport)
        {
            if (string.IsNullOrEmpty(descriptor.customevaluator))
            {
                // do we actually need to look in all assemblies? Maybe we can find a way to only evaluate on the right assembly
                foreach (var assembly in m_Assemblies)
                {
                    try
                    {    
                        var value = MethodEvaluator.Eval(assembly.Location,
                            descriptor.type, "get_" + descriptor.method, new System.Type[0]{}, new object[0]{});

                        if (value.ToString() == descriptor.value)
                        {
                            var projectSettingsWindowName = "Project/" + m_ProjectSettingsMapping.Where(p => p.Key.Equals(descriptor.type)).First().Value;
                            AddIssue(descriptor, string.Format("{0}: {1}", descriptor.description, value), projectSettingsWindowName, projectReport);
                        
                            // stop iterating assemblies
                            break;
                        }
                    }
                    catch (Exception)
                    {
                        // this is safe to ignore
                    }
                }
            }
            else
            {
                foreach (var analyzer in m_SettingAnalyzers)
                {
                    var projectIssue = analyzer.Analyze(descriptor);
                    if (projectIssue != null)
                    {
                        projectReport.AddIssue(projectIssue);
                    }                        
                }
            }
        }
    }
}