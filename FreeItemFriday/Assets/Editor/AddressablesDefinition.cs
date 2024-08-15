#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.AddressableAssets.Settings;
using ThunderKit.Core.Pipelines;
using UnityEditor.AddressableAssets;
using ThunderKit.Core.Attributes;
using ThunderKit.Core.Paths;
using UnityEditor.AddressableAssets.Build.DataBuilders;
using ThunderKit.Core.Manifests;
using System;
using RoR2.ContentManagement;
using System.Linq;
using HG;

namespace FreeItemFriday.Editor
{
    public class AddressablesDefinition : ManifestDatum
    {
        [Serializable]
        public struct AssetTypeLabel
        {
            [SerializableSystemType.RequiredBaseType(typeof(UnityEngine.Object))]
            public SerializableSystemType assetType;
            public string label;
        }

        [Serializable]
        public struct ComponentTypeLabel
        {
            [SerializableSystemType.RequiredBaseType(typeof(Component))]
            public SerializableSystemType componentType;
            public string label;
        }

        [PathReferenceResolver]
        public string RuntimeLoadPath;
        public AssetTypeLabel[] assetTypeLabels = AddressablesLabels.assetTypeLabels.Select(x => new AssetTypeLabel 
        { 
            assetType = (SerializableSystemType)x.Key,
            label = x.Value
        }).ToArray();
        public ComponentTypeLabel[] componentTypeLabels = AddressablesLabels.componentTypeLabels.Select(x => new ComponentTypeLabel
        {
            componentType = (SerializableSystemType)x.Key,
            label = x.Value
        }).ToArray();
    }
}
#endif