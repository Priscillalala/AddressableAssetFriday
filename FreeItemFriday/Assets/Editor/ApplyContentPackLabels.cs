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
    public class ApplyContentPackLabels : PipelineJob
    {
        public AddressableAssetSettings Addressables = AddressableAssetSettingsDefaultObject.Settings;

        public override Task Execute(Pipeline pipeline)
        {
            if (Addressables)
            {
                foreach (AddressableAssetGroup group in Addressables.groups)
                {
                    var schema = group.GetSchema<ContentPackLabelSchema>();
                    if (schema)
                    {
                        schema.ApplyLabel();
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}
