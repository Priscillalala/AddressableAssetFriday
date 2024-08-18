using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using UnityEngine;

namespace FreeItemFriday.ItemContent
{
    [AddContentPackProvider(ContentGroup.ITEMS, NAME)]
	public class ThereminContent : BaseContentPackProvider
	{
        const string NAME = "Theremin";

        public static class Items
        {
            public static ItemDef Theremin;
        }

        public static ConfigEntry<Percent> attackSpeedBonus;
        public static ConfigEntry<Percent> attackSpeedBonusPerStack;

        public override string identifier => $"FreeItemFriday.ItemContent.Theremin";

        public override IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            AddressablesLoadHelper loadHelper = CreateLoadHelper();
            loadHelper.AddContentPackLoadOperation(contentPack);
            loadHelper.AddLoadOperation<KeyAssetRuleSet>(KeyAssetRuleSet.LABEL, KeyAssetRuleSet.allAssets.AddRange);
            loadHelper.AddGenericOperation(delegate
            {
                ContentLoadHelper.PopulateTypeFields(typeof(Items), contentPack.itemDefs);
            }, 0.05f);
            loadHelper.AddGenericOperation(delegate
            {
                attackSpeedBonus = config.Option(NAME, "Attack Speed Bonus", (Percent)0.45f);
                attackSpeedBonusPerStack = config.Option(NAME, "Attack Speed Bonus Per Stack", (Percent)0.35f);
                LanguageSystem.SetArgs(Items.Theremin.descriptionToken, attackSpeedBonus, attackSpeedBonusPerStack);
            }, 0.05f);
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }
        }

        public override IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public override IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            On.RoR2.MusicController.UpdateTeleporterParameters += MusicController_UpdateTeleporterParameters;
            yield break;
        }

        private static void MusicController_UpdateTeleporterParameters(On.RoR2.MusicController.orig_UpdateTeleporterParameters orig, MusicController self, TeleporterInteraction teleporter, Transform cameraTransform, CharacterBody targetBody)
        {
            orig(self, teleporter, cameraTransform, targetBody);
            self.rtpcTeleporterProximityValue.value = Util.Remap(self.rtpcTeleporterProximityValue.value, 0f, 10000f, 5000f, 10000f);
            if (targetBody && targetBody.HasItem(Items.Theremin) && targetBody.TryGetComponent(out ThereminBehaviour behaviour))
            {
                self.rtpcTeleporterProximityValue.value -= 5000f * behaviour.currentBonusCoefficient;
                float directionValue = self.rtpcTeleporterDirectionValue.value;
                self.rtpcTeleporterDirectionValue.value -= (directionValue <= 180f ? directionValue : directionValue - 360f) * behaviour.currentBonusCoefficient;
            }
        }

        public class ThereminBehaviour : BaseItemBodyBehavior
        {
            [ItemDefAssociation(useOnServer = true, useOnClient = true)]
            public static ItemDef GetItemDef() => Items.Theremin;

            public float currentBonus;
            public float currentBonusCoefficient;
            public int lastPercentBonus;

            public void FixedUpdate()
            {
                if (TeleporterUtil.TryLocateTeleporter(out Vector3 position))
                {
                    Vector3 distance = body.corePosition - position;
                    currentBonusCoefficient = 1000f / (1000f + distance.sqrMagnitude);
                    currentBonus = currentBonusCoefficient * (attackSpeedBonus.Value + attackSpeedBonusPerStack.Value * (stack - 1));
                }
                else
                {
                    currentBonusCoefficient = 0;
                    currentBonus = 0;
                }
                int percentBonus = (int)(currentBonus * 100f);
                if (percentBonus != lastPercentBonus)
                {
                    lastPercentBonus = percentBonus;
                    body.MarkAllStatsDirty();
                }
            }

            public void OnEnable()
            {
                RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            }

            public void OnDisable()
            {
                RecalculateStatsAPI.GetStatCoefficients -= RecalculateStatsAPI_GetStatCoefficients;
            }

            private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
            {
                if (sender && sender == body)
                {
                    args.attackSpeedMultAdd += currentBonus;
                }
            }
        }
    }
}