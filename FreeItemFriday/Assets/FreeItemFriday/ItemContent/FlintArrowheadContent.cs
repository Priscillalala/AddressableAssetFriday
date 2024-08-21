using BepInEx.Configuration;
using HG;
using R2API;
using R2API.ScriptableObjects;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FreeItemFriday.ItemContent
{
    [AddContentPackProvider(ContentGroup.ITEMS, NAME)]
    public class FlintArrowheadContent : BaseContentPackProvider
    {
        const string NAME = "Flint Arrowhead";

        public static class Items
        {
            public static ItemDef Arrowhead;
        }

        public static class DamageColors
        {
            public static SerializableDamageColor StrongerBurn;
        }

        public static ConfigEntry<float> damage;
        public static ConfigEntry<float> damagePerStack;

        //public static DamageColorIndex StrongerBurn { get; private set; } = ColorsAPI.RegisterDamageColor(new Color32(244, 113, 80, 255));
        public static GameObject ImpactArrowhead { get; private set; }
        public static GameObject ImpactArrowheadStronger { get; private set; }

        public override string identifier => "FreeItemFriday.ItemContent.FlintArrowhead";

        public override IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            AddressablesLoadHelper loadHelper = CreateLoadHelper();
            loadHelper.AddContentPackLoadOperation(contentPack);
            loadHelper.AddLoadOperation<KeyAssetRuleSet>(KeyAssetRuleSet.LABEL, KeyAssetRuleSet.allAssets.Add);
            loadHelper.AddDamageColorsLoadOperation();
            loadHelper.AddUpgradeStubbedShadersOperation();
            loadHelper.AddGenericOperation(delegate
            {
                ContentLoadHelper.PopulateTypeFields(typeof(Items), contentPack.itemDefs);
                ContentLoadHelper.PopulateTypeFields(typeof(DamageColors), SerializableColorCatalog.damageColors, fieldName => "dc" + fieldName);
            }, 0.05f);
            loadHelper.AddGenericOperation(delegate
            {
                damage = config.Option(NAME, "Flat Damage", 3f);
                damagePerStack = config.Option(NAME, "Flat Damage Per Stack", 3f);
                LanguageSystem.SetArgs(Items.Arrowhead.descriptionToken, damage, damagePerStack);
            }, 0.05f);
            loadHelper.AddGenericOperation(CreateImpactEffectsAsync);
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }
        }

        public IEnumerator CreateImpactEffectsAsync()
        {
            var OmniExplosionVFXQuick = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/OmniExplosionVFXQuick.prefab");
            var matOmniHitspark3Gasoline = Addressables.LoadAssetAsync<Material>("RoR2/Base/IgniteOnKill/matOmniHitspark3Gasoline.mat");
            var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(new List<AsyncOperationHandle>
            {
                OmniExplosionVFXQuick,
                matOmniHitspark3Gasoline
            }, true);
            while (!groupOp.IsDone)
            {
                yield return null;
            }

            ImpactArrowhead = Prefab.Clone(OmniExplosionVFXQuick.Result, "ImpactArrowhead");
            if (ImpactArrowhead.TryGetComponent(out EffectComponent effectComponent))
            {
                effectComponent.soundName = "Play_item_proc_strengthenBurn";
            }
            if (ImpactArrowhead.TryGetComponent(out VFXAttributes vFXAttributes))
            {
                vFXAttributes.vfxPriority = VFXAttributes.VFXPriority.Low;
            }
            if (ImpactArrowhead.TryGetComponent(out OmniEffect omniEffect))
            {
                for (int i = omniEffect.omniEffectGroups.Length - 1; i >= 0; i--)
                {
                    switch (omniEffect.omniEffectGroups[i].name)
                    {
                        case "Scaled Smoke":
                        case "Smoke Ring":
                        case "Area Indicator Ring":
                        case "Unscaled Smoke":
                        case "Flames":
                            ArrayUtils.ArrayRemoveAtAndResize(ref omniEffect.omniEffectGroups, i);
                            break;
                    }
                }
            }

            ImpactArrowheadStronger = Prefab.Clone(ImpactArrowhead, "ImpactArrowHeadStronger");
            ImpactArrowheadStronger.transform.GetChild(0).GetComponent<Renderer>().sharedMaterial = matOmniHitspark3Gasoline.Result;

            contentPack.effectDefs.Add(new[] { new EffectDef(ImpactArrowhead), new EffectDef(ImpactArrowheadStronger) });
        }

        public override IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public override IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            On.RoR2.DotController.InitDotCatalog += DotController_InitDotCatalog;
            Events.GlobalEventManager.onHitEnemyAcceptedServer += GlobalEventManager_onHitEnemyAcceptedServer;
            yield break;
        }

        private static void DotController_InitDotCatalog(On.RoR2.DotController.orig_InitDotCatalog orig)
        {
            orig();
            DotController.dotDefs[(int)DotController.DotIndex.StrongerBurn].damageColorIndex = DamageColors.StrongerBurn.DamageColorIndex;
        }

        private static void GlobalEventManager_onHitEnemyAcceptedServer(DamageInfo damageInfo, GameObject victim, uint? dotMaxStacksFromAttacker)
        {
            if (damageInfo.attacker && damageInfo.attacker.TryGetComponent(out CharacterBody attackerBody) && attackerBody.HasItem(Items.Arrowhead, out int stack) && Util.CheckRoll(100f * damageInfo.procCoefficient, attackerBody.master))
            {
                InflictDotInfo inflictDotInfo = new InflictDotInfo
                {
                    attackerObject = damageInfo.attacker,
                    dotIndex = DotController.DotIndex.Burn,
                    victimObject = victim,
                    totalDamage = damage.Value + damagePerStack.Value * (stack - 1)
                };
                StrengthenBurnUtils.CheckDotForUpgrade(attackerBody.inventory, ref inflictDotInfo);
                DotController.DotDef dotDef = DotController.GetDotDef(inflictDotInfo.dotIndex);
                if (dotDef != null)
                {
                    DamageInfo burnDamageInfo = new DamageInfo();
                    burnDamageInfo.attacker = inflictDotInfo.attackerObject;
                    burnDamageInfo.crit = false;
                    burnDamageInfo.damage = (float)inflictDotInfo.totalDamage;
                    burnDamageInfo.force = Vector3.zero;
                    burnDamageInfo.inflictor = inflictDotInfo.attackerObject;
                    burnDamageInfo.position = damageInfo.position;
                    burnDamageInfo.procCoefficient = 0f;
                    burnDamageInfo.damageColorIndex = dotDef.damageColorIndex;
                    burnDamageInfo.damageType = DamageType.DoT | DamageType.Silent;
                    burnDamageInfo.dotIndex = inflictDotInfo.dotIndex;
                    if (inflictDotInfo.victimObject && inflictDotInfo.victimObject.TryGetComponent(out CharacterBody victimBody) && victimBody.healthComponent)
                    {
                        victimBody.healthComponent.TakeDamage(burnDamageInfo);
                        EffectManager.SpawnEffect(inflictDotInfo.dotIndex == DotController.DotIndex.Burn ? ImpactArrowhead : ImpactArrowheadStronger, new EffectData
                        {
                            origin = damageInfo.position,
                            rotation = Util.QuaternionSafeLookRotation(-damageInfo.force),
                            scale = inflictDotInfo.dotIndex == DotController.DotIndex.Burn ? 1.5f : 2.5f
                        }, true);
                    }
                }
            }
        }
    }
}