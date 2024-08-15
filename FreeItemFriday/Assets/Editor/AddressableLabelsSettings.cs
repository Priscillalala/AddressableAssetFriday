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
using ThunderKit.Core.Data;
using RoR2.ContentManagement;
using System;
using System.Reflection;

namespace FreeItemFriday.Editor
{
    public class AddressableLabelsSettings
    {
        public bool includeShaders = false;

        private static FieldInfo labelsField;
        private static FieldInfo subAssetsField;

        //[InitializeOnLoadMethod]
        public static void OnLoad()
        {
            AddressableAssetSettings.OnModificationGlobal -= OnModificationGlobal;
            AddressableAssetSettings.OnModificationGlobal += OnModificationGlobal;

            labelsField = typeof(AddressableAssetEntry).GetField("m_Labels", BindingFlags.Instance | BindingFlags.NonPublic);
            subAssetsField = typeof(AddressableAssetEntry).GetField("SubAssets", BindingFlags.Instance | BindingFlags.NonPublic);
        }

        private static void OnModificationGlobal(AddressableAssetSettings addressables, AddressableAssetSettings.ModificationEvent modificationEvent, object data)
        {
            //var settings = GetOrCreateSettings<AddressableLabelsSettings>();
            if (data is ICollection collection)
            {
                foreach (object item in collection)
                {
                    OnModificationGlobal(addressables, modificationEvent, item);
                }
                return;
            }
            if (data is AddressableAssetEntry entry)// && entry.TargetAsset && modificationEvent != AddressableAssetSettings.ModificationEvent.EntryRemoved)
            {
                Debug.Log($"{modificationEvent}: {entry.AssetPath}");
                if (AssetDatabase.IsValidFolder(entry.AssetPath))
                {
                    List<AddressableAssetEntry> subAssets = (List<AddressableAssetEntry>)subAssetsField.GetValue(entry);
                    if (subAssets != null)
                    {
                        Debug.Log($"has sub assets: {subAssets.Count}");
                        foreach (var subAssetEntry in subAssets)
                        {
                            Debug.Log($"sub assets: {subAssetEntry.AssetPath}");
                            Debug.Log($"sub asset has label?: {subAssetEntry.labels.Contains("NotAFolder")}");
                            var labels = new HashSet<string>(subAssetEntry.labels);
                            labels.Add("NotAFolder");
                            labelsField.SetValue(subAssetEntry, labels);
                            //subAssetEntry.SetLabel("NotAFolder", true, true, false);
                        }
                    }
                }
                /*if (entry.IsSubAsset)
                {
                    labelsField.SetValue(entry, new HashSet<string>(entry.labels));
                }
                entry.SetLabel("NotAFolder", true, true, false);*/
                /*Type assetType = entry.TargetAsset.GetType();
                if (!settings.includeShaders && assetType == typeof(Shader))
                {
                    return;
                }
                if (AddressablesLabels.assetTypeLabels.TryGetValue(assetType, out string label))
                {
                    if (entry.IsSubAsset)
                    {
                        labelsField.SetValue(entry, new HashSet<string>(entry.labels));
                    }
                    entry.SetLabel(label, true, true, false);
                }*/
            }
        }
    }
}
#endif