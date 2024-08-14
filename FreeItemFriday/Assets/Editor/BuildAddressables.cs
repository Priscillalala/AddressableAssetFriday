#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using ThunderKit.Core.Pipelines;
using System.Threading.Tasks;
using UnityEditor.AddressableAssets;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using UnityEditor.AddressableAssets.Build.DataBuilders;

namespace FreeItemFriday.Editor
{
    [PipelineSupport(typeof(Pipeline))]
    public class BuildAddressables : FlowPipelineJob
    {
        public AddressableAssetSettings Addressables = AddressableAssetSettingsDefaultObject.Settings;

        [PathReferenceResolver]
        public string BuildPath;
        [PathReferenceResolver]
        public string LoadPath;

        protected override Task ExecuteInternal(Pipeline pipeline)
        {
            if (Addressables)
            {
                string 
                    resolvedBuildPath = BuildPath.Resolve(pipeline, this),
                    resolvedLoadPath = LoadPath.Resolve(pipeline, this),
                    bundlesBuildPath = resolvedBuildPath +"/[BuildTarget]",
                    bundlesLoadPath = resolvedLoadPath + "/[BuildTarget]",
                    preEvaluatedBuildPath = Addressables.profileSettings.EvaluateString(Addressables.activeProfileId, resolvedBuildPath),
                    preEvaluatedBundlesPath = Addressables.profileSettings.EvaluateString(Addressables.activeProfileId, bundlesBuildPath);
                if (Directory.Exists(preEvaluatedBuildPath))
                {
                    Directory.Delete(preEvaluatedBuildPath, true);
                }
                Addressables.BuildRemoteCatalog = true;
                Addressables.profileSettings.SetValue(Addressables.activeProfileId, "LocalBuildPath", bundlesBuildPath);
                Addressables.profileSettings.SetValue(Addressables.activeProfileId, "LocalLoadPath", bundlesLoadPath);
                Addressables.OverridePlayerVersion = "tk";
                Addressables.ActivePlayerDataBuilderIndex = Addressables.DataBuilders.FindIndex(s => s.GetType() == typeof(BuildScriptPackedMode));
                Debug.LogWarning("BUILD CATALOG!");
                AddressableAssetSettings.BuildPlayerContent();
                string catalogPath = Path.Combine(preEvaluatedBundlesPath, $"catalog_{Addressables.OverridePlayerVersion}.json");
                if (File.Exists(catalogPath))
                {
                    Debug.Log("catalog exists");
                    Debug.Log(catalogPath);
                    File.Move(catalogPath, Path.Combine(preEvaluatedBuildPath, "catalog.json"));
                    Debug.Log("moved catalog");
                }
                string hashPath = Path.Combine(preEvaluatedBundlesPath, $"catalog_{Addressables.OverridePlayerVersion}.hash");
                if (File.Exists(hashPath))
                {
                    Debug.Log("hash exists");
                    File.Delete(hashPath);
                    Debug.Log("killed hash");
                }
            }
            return Task.CompletedTask;
        }

        [MenuItem("FreeItemFriday/Build Addressables")]
        public static void RefreshLabels()
        {
            AddressableAssetSettings.BuildPlayerContent();
        }
    }
}
#endif