using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using RoR2.ContentManagement;
using System.Linq;

namespace FreeItemFriday.Editor
{
    public class LabelWriter
    {
        [MenuItem("AddressablesTest/Refresh Labels")]
        public static void RefreshLabels()
        {
            Debug.Log("Refreshing Labels..");
            var settings = AssetDatabase.LoadAssetAtPath<AddressableAssetSettings>("Assets/AddressableAssetsData/AddressableAssetSettings.asset");
            var entries = settings.groups.SelectMany(x => x.entries);
            foreach (AddressableAssetEntry entry in entries)
            {
                Debug.Log($"Entry: {entry.AssetPath}, target: {entry.TargetAsset?.name}, type:{entry.TargetAsset?.GetType().Name} ");
            }
            //AssetDatabase.IsValidFolder()

            foreach (var assetTypeLabel in AddressablesLabels.assetTypeLabels)
            {
                if (assetTypeLabel.Key == typeof(Shader))
                {
                    continue;
                }
                string[] assets = AssetDatabase.FindAssets($"t:{assetTypeLabel.Key.Name}", new[] { "Assets/FreeItemFriday" });
                foreach (string guid in assets)
                {
                    AddressableAssetEntry entry = settings.FindAssetEntry(guid);
                    if (entry != null)
                    {
                        string path = Path.GetDirectoryName(AssetDatabase.GUIDToAssetPath(guid)).Substring(7);
                        Debug.Log(path);
                        string contentPackLabel = $"ContentPack:{path.Replace('\\', '.')}";
                        //entry.SetLabel(contentPackLabel, true, true);
                        if (!entry.SetLabel(assetTypeLabel.Value, true, true))
                        {
                            Debug.LogWarning($"failed to set label {assetTypeLabel.Value} on {path}!");
                        }
                    }
                }
            }
        }
    }
}
