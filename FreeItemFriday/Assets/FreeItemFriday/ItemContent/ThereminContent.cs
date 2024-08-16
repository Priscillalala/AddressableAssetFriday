using BepInEx.Configuration;
using RiskOfOptions;
using RiskOfOptions.Options;
using RiskOfOptions.OptionConfigs;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using UnityEngine;

namespace FreeItemFriday.ItemContent
{
	public class ThereminContent : IContentPackProvider
	{
        public static class Items
        {
            public static ItemDef Theremin;
        }

        private readonly ContentPack contentPack = new ContentPack();

        public static ConfigEntry<float> attackSpeedBonus;
        public static ConfigEntry<float> attackSpeedBonusPerStack;

        public string identifier => "FreeItemFriday.ItemContent.Theremin";

        public IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            AddressablesLoadHelper loadHelper = AddressablesLoadHelper.CreateUsingDefaultResourceLocator("ContentPack:" + identifier);
            loadHelper.AddContentPackLoadOperation(contentPack);
            loadHelper.AddGenericOperation(delegate
            {
                ContentLoadHelper.PopulateTypeFields(typeof(Items), contentPack.itemDefs);
            }, 0.05f);
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }
        }

        public IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            ConfigFile config = FreeItemFridayPlugin.Instance.Config;
            attackSpeedBonus = config.Bind("Theremin", "Attack Speed Bonus", 0.45f);
            attackSpeedBonus.SettingChanged += (sender, eventArgs) =>
            {
                Debug.Log($"attack speed bonus changed: {attackSpeedBonus.Value}");
            };
            attackSpeedBonusPerStack = config.Bind("Theremin", "Attack Speed Bonus Per Stack", 0.35f);
            attackSpeedBonusPerStack.SettingChanged += (sender, eventArgs) =>
            {
                Debug.Log($"attack speed bonus PER STACK changed: {attackSpeedBonusPerStack.Value}");
            };
            LanguageSystem.SetArgs(Items.Theremin.descriptionToken, attackSpeedBonus, attackSpeedBonusPerStack);
            ModSettingsManager.AddOption(new FloatFieldOption(attackSpeedBonus, new FloatFieldConfig { FormatString = "{0:%}" }));
            ModSettingsManager.AddOption(new FloatFieldOption(attackSpeedBonusPerStack, new FloatFieldConfig { FormatString = "{0:%}" }));
            //On.RoR2.MusicController.UpdateTeleporterParameters += MusicController_UpdateTeleporterParameters;
            yield break;
        }

        /*private static void MusicController_UpdateTeleporterParameters(On.RoR2.MusicController.orig_UpdateTeleporterParameters orig, MusicController self, TeleporterInteraction teleporter, Transform cameraTransform, CharacterBody targetBody)
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
                    currentBonus = currentBonusCoefficient * Ivyl.StackScaling(attackSpeedBonus, attackSpeedBonusPerStack, stack);
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
        }*/
    }
}