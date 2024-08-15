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
using UnityEditor.AddressableAssets.Build.DataBuilders;
using UnityEditor.AddressableAssets.Settings.GroupSchemas;
using System.Linq;
using UnityEditor.AddressableAssets.Build;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.AddressableAssets.Initialization;
using UnityEngine.ResourceManagement.Util;

namespace FreeItemFriday.Editor
{
    [CreateAssetMenu(fileName = "BuildScriptPacked.asset", menuName = "Addressables/Content Builders/Build RoR2 Content")]
    public class BuildScriptRoR2 : BuildScriptPackedMode
    {
        private static readonly FieldInfo labelsField = typeof(AddressableAssetEntry).GetField("m_Labels", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly FieldInfo subAssetsField = typeof(AddressableAssetEntry).GetField("SubAssets", BindingFlags.Instance | BindingFlags.NonPublic);

        private AddressablesDefinition.AssetTypeLabel[] assetTypeLabels;
        private AddressablesDefinition.ComponentTypeLabel[] componentTypeLabels;

        public override string Name => "Build RoR2 Content";

        public void SetAssetTypeLabels(AddressablesDefinition.AssetTypeLabel[] assetTypeLabels, AddressablesDefinition.ComponentTypeLabel[] componentTypeLabels)
        {
            this.assetTypeLabels = assetTypeLabels;
            this.componentTypeLabels = componentTypeLabels;
        }

        protected override string ProcessGroupSchema(AddressableAssetGroupSchema schema, AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
        {
            if (schema is ContentPackLabelSchema contentPackLabelSchema)
            {
                contentPackLabelSchema.ApplyLabel();
            }
            return base.ProcessGroupSchema(schema, assetGroup, aaContext);
        }

        protected override string ProcessBundledAssetSchema(BundledAssetGroupSchema schema, AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
        {
            string result = base.ProcessBundledAssetSchema(schema, assetGroup, aaContext);
            foreach (AddressableAssetEntry entry in assetGroup.entries)
            {
                if (AssetDatabase.IsValidFolder(entry.AssetPath))
                {
                    List<AddressableAssetEntry> subAssets = (List<AddressableAssetEntry>)subAssetsField.GetValue(entry);
                    if (subAssets != null)
                    {
                        foreach (var subEntry in subAssets)
                        {
                            ProcessBundledAsset(subEntry, assetGroup, aaContext);
                        }
                    }
                }
                else
                {
                    ProcessBundledAsset(entry, assetGroup, aaContext);
                }
            }
            return result;
        }

        public void ProcessBundledAsset(AddressableAssetEntry entry, AddressableAssetGroup assetGroup, AddressableAssetsBuildContext aaContext)
        {
            Type assetType = AssetDatabase.GetMainAssetTypeAtPath(entry.AssetPath);
            if (assetType == typeof(GameObject))
            {
                GameObject gameObject = (GameObject)entry.TargetAsset;
                foreach (var componentTypeLabel in componentTypeLabels)
                {
                    if (gameObject.TryGetComponent((Type)componentTypeLabel.componentType, out _))
                    {
                        LabelAsset(entry, componentTypeLabel.label);
                    }
                }
                return;
            }
            foreach (var assetTypeLabel in assetTypeLabels)
            {
                if (((Type)assetTypeLabel.assetType).IsAssignableFrom(assetType))
                {
                    LabelAsset(entry, assetTypeLabel.label);
                    continue;
                }
            }
        }

        private void LabelAsset(AddressableAssetEntry entry, string label)
        {
            Debug.Log($"{label}: {entry.AssetPath}");
            if (entry.ParentEntry != null && entry.labels == entry.ParentEntry.labels)
            {
                labelsField.SetValue(entry, new HashSet<string>(entry.labels));
            }
            entry.SetLabel(label, true, true, false);
        }
    }
}
#endif