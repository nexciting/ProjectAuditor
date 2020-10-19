using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;
#if UNITY_2018_2_OR_NEWER
using UnityEditor.Build.Player;

#endif

namespace Unity.ProjectAuditor.Editor.Utils
{
    internal class AssemblyCompilationHelper : IDisposable
    {
        private string m_OutputFolder = String.Empty;
        private bool m_Success = true;

        private List<AssemblyInfo> m_AssemblyInfos = new List<AssemblyInfo>();
        private int m_PendingAssemblies = 0;

        private Action<string> m_OnAssemblyCompilationStarted;

        public Action<string, CompilerMessage[]> AssemblyCompilationFinished;
        public Action<AssemblyCompilationHelper, IEnumerable<AssemblyInfo>> CompilationFinished;

        public void Dispose()
        {
#if UNITY_2018_2_OR_NEWER
            if (m_OnAssemblyCompilationStarted != null)
                CompilationPipeline.assemblyCompilationStarted -= m_OnAssemblyCompilationStarted;

            //CompilationPipeline.assemblyCompilationFinished -= OnAssemblyCompilationFinished;
#endif
            if (!string.IsNullOrEmpty(m_OutputFolder) && Directory.Exists(m_OutputFolder))
            {
                Directory.Delete(m_OutputFolder, true);
            }
            m_OutputFolder = string.Empty;
        }

        public void Compile(IProgressBar progressBar = null)
        {
            bool async = false;
#if UNITY_2018_1_OR_NEWER
            var assembliesTmp = CompilationPipeline.GetAssemblies(AssembliesType.Player);
#else
            var assemblies = CompilationPipeline.GetAssemblies();
#endif

            //HACK
            var assemblies = assembliesTmp.Where(ass =>
                !ass.name.Contains("Entities") && !ass.name.Contains("Transforms") &&
                !ass.name.Contains("Collections") && !ass.name.Contains("TextMeshPro") &&
                !ass.name.Contains("RenderPipelines")).ToArray();

#if UNITY_2018_2_OR_NEWER
            async = true;
            if (progressBar != null)
            {
                var numAssemblies = assemblies.Length;
                progressBar.Initialize("Assembly Compilation", "Compiling project scripts",
                    numAssemblies);
                m_OnAssemblyCompilationStarted = (s) =>
                {
                    progressBar.AdvanceProgressBar(Path.GetFileName(s));
                };
                CompilationPipeline.assemblyCompilationStarted += m_OnAssemblyCompilationStarted;
            }
            //CompilationPipeline.assemblyCompilationFinished += OnAssemblyCompilationFinished;

            m_OutputFolder = FileUtil.GetUniqueTempPathInProject();

            if (!Directory.Exists(m_OutputFolder))
                Directory.CreateDirectory(m_OutputFolder);

            m_PendingAssemblies = assemblies.Length;
            foreach (var assembly in assemblies)
            {
                var assemblyPath = Path.Combine(m_OutputFolder, Path.GetFileName(assembly.outputPath));
                var assemblyBuilder = new AssemblyBuilder(assemblyPath, assembly.sourceFiles);

                assemblyBuilder.buildFinished += OnAssemblyCompilationFinished;
                assemblyBuilder.compilerOptions = assembly.compilerOptions;
                assemblyBuilder.additionalReferences = assembly.allReferences;
                assemblyBuilder.additionalDefines = assembly.defines;

                assemblyBuilder.compilerOptions.RoslynAnalyzerDllPaths = new[]
                {

                };
                bool result = assemblyBuilder.Build();
                Debug.Log(result);
            }

            if (progressBar != null)
                progressBar.ClearProgressBar();

            if (!m_Success && !async)
            {
                Dispose();
                throw new AssemblyCompilationException();
            }

            var compiledAssemblyPaths = assemblies.Select(assembly => Path.Combine(m_OutputFolder, Path.GetFileName(assembly.outputPath)));
#else
            // fallback to CompilationPipeline assemblies
            var compiledAssemblyPaths = CompilationPipeline.GetAssemblies()
                .Where(a => a.flags != AssemblyFlags.EditorAssembly).Select(assembly => assembly.outputPath);
#endif

            m_AssemblyInfos = new List<AssemblyInfo>();
            foreach (var compiledAssemblyPath in compiledAssemblyPaths)
            {
                var assemblyInfo = AssemblyHelper.GetAssemblyInfoFromAssemblyPath(compiledAssemblyPath);
                var assembly = assemblies.First(a => a.name.Equals(assemblyInfo.name));
                var sourcePaths = assembly.sourceFiles.Select(file => file.Remove(0, assemblyInfo.relativePath.Length + 1));

                assemblyInfo.sourcePaths = sourcePaths.ToArray();
                m_AssemblyInfos.Add(assemblyInfo);
            }

            if (!async)
            {
                CompilationFinished(this, m_AssemblyInfos);
            }
        }

        public IEnumerable<string> GetCompiledAssemblyDirectories()
        {
#if UNITY_2018_2_OR_NEWER
            yield return m_OutputFolder;
#else
            foreach (var dir in CompilationPipeline.GetAssemblies()
                     .Where(a => a.flags != AssemblyFlags.EditorAssembly).Select(assembly => Path.GetDirectoryName(assembly.outputPath)).Distinct())
            {
                yield return dir;
            }
#endif
        }

        private void OnAssemblyCompilationFinished(string outputAssemblyPath, CompilerMessage[] messages)
        {
            --m_PendingAssemblies;
            AssemblyCompilationFinished(outputAssemblyPath, messages);
            var errors = messages.Where(message => message.type == CompilerMessageType.Error).ToArray();

            if (m_PendingAssemblies == 0)
                CompilationFinished(this, m_AssemblyInfos);
            m_Success = m_Success && messages.Count(message => message.type == CompilerMessageType.Error) == 0;
        }
    }
}
