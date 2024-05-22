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
            Debug.Log("transform:");
            string[] assets = AssetDatabase.FindAssets($"t:Light", new[] { "Assets/FreeItemFriday" });
            foreach (string guid in assets)
            {
                Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            }
            Debug.Log("prefab:");
            assets = AssetDatabase.FindAssets($"t:prefab", new[] { "Assets/FreeItemFriday" });
            foreach (string guid in assets)
            {
                Debug.Log(AssetDatabase.GUIDToAssetPath(guid));
            }
        }
    }
}
