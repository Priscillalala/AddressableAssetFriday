using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;

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

        public UnityEngine.Object keyAsset;
        public ItemDisplayRuleGroup[] itemDisplayRuleGroups;
    }
}