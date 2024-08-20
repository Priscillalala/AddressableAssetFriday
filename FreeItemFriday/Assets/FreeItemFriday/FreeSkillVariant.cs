using RoR2;
using RoR2.ContentManagement;
using System;
using System.Collections.Generic;
using UnityEngine;
using HG;
using System.Linq;
using RoR2.Skills;

namespace FreeItemFriday
{
    [CreateAssetMenu(menuName = "FreeItemFriday/FreeSkillVariant")]
    public class FreeSkillVariant : ScriptableObject
    {
        public const string LABEL = "FreeSkillVariant";

        public static readonly List<FreeSkillVariant> allAssets = new List<FreeSkillVariant>();

        static FreeSkillVariant()
        {
            ContentManager.onContentPacksAssigned += ContentManager_onContentPacksAssigned;
        }

        private static void ContentManager_onContentPacksAssigned(ReadOnlyArray<ReadOnlyContentPack> contentPacks)
        {
            foreach (FreeSkillVariant freeSkillVariant in allAssets)
            {
                for (int i = contentPacks.Length - 1; i >= 0; i--)
                {
                    if (contentPacks[i].identifier == freeSkillVariant.targetContentPackIdentifier)
                    {
                        SkillFamily skillFamily = contentPacks[i].skillFamilies.Find(freeSkillVariant.targetSkillFamilyName);
                        if (skillFamily)
                        {
                            ArrayUtils.ArrayAppend(ref skillFamily.variants, freeSkillVariant.variant);
                        }
                        break;
                    }
                }
            }
        }

        [Header("Target")]
        public string targetContentPackIdentifier;
        public string targetSkillFamilyName;
        [Header("Variant")]
        public SkillFamily.Variant variant;
    }
}