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
using ThunderKit.Core.Manifests;

namespace FreeItemFriday.Editor
{
    public class AddressablesDefinition : ManifestDatum
    {
        [PathReferenceResolver]
        public string RuntimeLoadPath;
    }
}
#endif