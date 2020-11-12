using System;
using System.Linq;
using Mono.Cecil;
using UnityEngine;

namespace Unity.ProjectAuditor.Editor.CodeAnalysis
{
    static class MonoBehaviourAnalysis
    {
        static readonly int k_ClassNameHashCode = "UnityEngine.MonoBehaviour".GetHashCode();

        static readonly string[] k_MagicMethodNames =
        {"Awake", "Start", "OnEnable", "OnDisable", "Update", "LateUpdate", "OnEnable", "FixedUpdate"};

        static readonly string[] k_UpdateMethodNames = {"Update", "LateUpdate", "FixedUpdate"};

        public static bool IsMonoBehaviour(TypeReference typeReference)
        {
            try
            {
                var typeDefinition = typeReference.Resolve();

                if (typeDefinition.FullName.GetHashCode() == k_ClassNameHashCode &&
                    typeDefinition.Module.Name.Equals("UnityEngine.CoreModule.dll"))
                    return true;

                if (typeDefinition.BaseType != null)
                    return IsMonoBehaviour(typeDefinition.BaseType);
            }
            catch (AssemblyResolutionException e)
            {
                Debug.LogWarning(e);
            }

            return false;
        }

        public static bool IsMonoBehaviourMagicMethod(MethodDefinition methodDefinition)
        {
            return k_MagicMethodNames.Contains(methodDefinition.Name);
        }

        public static bool IsMonoBehaviourUpdateMethod(MethodDefinition methodDefinition)
        {
            return k_UpdateMethodNames.Contains(methodDefinition.Name);
        }
    }
}
