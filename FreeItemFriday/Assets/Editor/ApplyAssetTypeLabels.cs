using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using ThunderKit.Core.Pipelines;
using System.Threading.Tasks;
using RoR2.ContentManagement;
using ThunderKit.Core.Attributes;
using System;
using System.Reflection;
using UnityEditor.AddressableAssets;

namespace FreeItemFriday.Editor
{
    [PipelineSupport(typeof(Pipeline))]
    public class ApplyAssetTypeLabels : PipelineJob
    {
        public AddressableAssetSettings Addressables = AddressableAssetSettingsDefaultObject.Settings;
        [PathReferenceResolver]
        public string[] RootFolderPaths = new[] { "Assets" };
        public bool IncludeShaders = false;

        public override Task Execute(Pipeline pipeline)
        {
            FieldInfo labelsField = typeof(AddressableAssetEntry).GetField("m_Labels", BindingFlags.Instance | BindingFlags.NonPublic);
            Debug.Log(labelsField.Name);
            if (Addressables)
            {
                foreach (var assetTypeLabel in AddressablesLabels.assetTypeLabels)
                {
                    if (!IncludeShaders && assetTypeLabel.Key == typeof(Shader))
                    {
                        continue;
                    }
                    string[] assets = AssetDatabase.FindAssets($"t:{assetTypeLabel.Key.Name}", RootFolderPaths);
                    foreach (string guid in assets)
                    {
                        AddressableAssetEntry entry = Addressables.FindAssetEntry(guid, true);
                        if (entry != null)
                        {
                            if (entry.ParentEntry != null)
                            {
                                Addressables.MoveEntry(entry, entry.ParentEntry.parentGroup);//, entry.ReadOnly);
                                labelsField.SetValue(entry, new HashSet<string>(entry.labels));
                            }
                            if (!entry.SetLabel(assetTypeLabel.Value, true, true))
                            {
                                Debug.LogWarning($"failed to set label {assetTypeLabel.Value} on {AssetDatabase.GUIDToAssetPath(guid)}!");
                            }
                        }
                    }
                }
                /*string[] prefabs = AssetDatabase.FindAssets($"t:prefab", RootFolderPaths);
                foreach (string guid in prefabs)
                {
                    AddressableAssetEntry entry = Addressables.FindAssetEntry(guid, true);
                    if (entry != null)
                    {
                        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
                        if (prefab)
                        {
                            foreach (var componentTypeLabel in AddressablesLabels.componentTypeLabels)
                            {
                                if (prefab.GetComponent(componentTypeLabel.Key))
                                {
                                    if (!entry.SetLabel(componentTypeLabel.Value, true, true))
                                    {
                                        Debug.LogWarning($"failed to set label {componentTypeLabel.Value} on {AssetDatabase.GUIDToAssetPath(guid)}!");
                                    }
                                }
                            }
                        }
                    }
                }*/
            }
            return Task.CompletedTask;
        }
    }
}
