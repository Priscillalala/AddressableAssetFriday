using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using HG;
using System.Linq;

namespace FreeItemFriday
{
    [CreateAssetMenu(menuName = "FreeItemFriday/KeyAssetRuleSet")]
    public class KeyAssetRuleSet : ScriptableObject
    {
        [Serializable]
        public struct ItemDisplayRuleGroup
        {
            public string targetRuleSetName;
            public ItemDisplayRule[] rules;
        }

        public const string LABEL = "KeyAssetRuleSet";

        public static readonly NamedAssetCollection<KeyAssetRuleSet> allAssets = new NamedAssetCollection<KeyAssetRuleSet>(ContentPack.getScriptableObjectName);

        static KeyAssetRuleSet()
        {
            ContentManager.onContentPacksAssigned += ContentManager_onContentPacksAssigned;
        }

        private static void ContentManager_onContentPacksAssigned(ReadOnlyArray<ReadOnlyContentPack> contentPacks)
        {
            Dictionary<string, ItemDisplayRuleSet> itemDisplayRuleSets = ItemDisplayRuleSet.instancesList.ToDictionary(x => x.name);
            foreach (KeyAssetRuleSet keyAssetRuleSet in allAssets)
            {
                foreach (ItemDisplayRuleGroup group in keyAssetRuleSet.itemDisplayRuleGroups)
                {
                    if (itemDisplayRuleSets.TryGetValue(group.targetRuleSetName, out ItemDisplayRuleSet idrs))
                    {
                        ArrayUtils.ArrayAppend(ref idrs.keyAssetRuleGroups, new ItemDisplayRuleSet.KeyAssetRuleGroup
                        {
                            keyAsset = keyAssetRuleSet.keyAsset,
                            displayRuleGroup = { rules = group.rules }
                        });
                    }
                }
            }
        }

        public UnityEngine.Object keyAsset;
        public ItemDisplayRuleGroup[] itemDisplayRuleGroups;
    }
}