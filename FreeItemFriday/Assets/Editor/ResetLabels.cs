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

namespace FreeItemFriday.Editor
{
    [PipelineSupport(typeof(Pipeline))]
    public class ResetLabels : PipelineJob
    {
        public AddressableAssetSettings Addressables => AddressableAssetSettingsDefaultObject.Settings;

        public override Task Execute(Pipeline pipeline)
        {
            if (Addressables)
            {
                List<string> labels = Addressables.GetLabels();
                for (int i = labels.Count - 1; i >= 0; i--)
                {
                    Addressables.RemoveLabel(labels[i]);
                }
            }
            return Task.CompletedTask;
        }
    }
}
#endif