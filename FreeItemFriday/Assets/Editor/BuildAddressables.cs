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
        public AddressableAssetSettings Addressables => AddressableAssetSettingsDefaultObject.Settings;

        [PathReferenceResolver]
        public string BuildPath;
        [PathReferenceResolver]
        public string LoadPath;

        protected override Task ExecuteInternal(Pipeline pipeline)
        {
            if (Addressables)
            {
                string resolvedBuildPath = BuildPath.Resolve(pipeline, this);
                string resolvedLoadPath = LoadPath.Resolve(pipeline, this);
                if (Directory.Exists(resolvedBuildPath))
                {
                    Directory.Delete(resolvedBuildPath, true);
                }
                Addressables.BuildRemoteCatalog = true;
                Addressables.profileSettings.SetValue(Addressables.activeProfileId, Addressables.RemoteCatalogBuildPath.GetName(Addressables), resolvedBuildPath);
                Addressables.profileSettings.SetValue(Addressables.activeProfileId, Addressables.RemoteCatalogLoadPath.GetName(Addressables), resolvedLoadPath);
                Addressables.OverridePlayerVersion = pipeline.Manifest.Identity.Name;
                Addressables.ActivePlayerDataBuilderIndex = Addressables.DataBuilders.FindIndex(s => s.GetType() == typeof(BuildScriptPackedMode));
                AddressableAssetSettings.BuildPlayerContent();
                /*string catalogPath = Path.Combine(resolvedBuildPath, $"catalog_{Addressables.OverridePlayerVersion}.json");
                if (File.Exists(catalogPath))
                {
                    Debug.Log("catalog exists");
                    Debug.Log(catalogPath);
                    File.Move(catalogPath, Path.Combine(resolvedBuildPath, "catalog.json"));
                    Debug.Log("moved catalog");
                }*/
                string hashPath = Path.Combine(resolvedBuildPath, $"catalog_{Addressables.OverridePlayerVersion}.hash");
                if (File.Exists(hashPath))
                {
                    File.Delete(hashPath);
                }
            }
            return Task.CompletedTask;
        }
    }
}
#endif