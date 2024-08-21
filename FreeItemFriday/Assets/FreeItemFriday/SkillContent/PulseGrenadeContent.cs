using System;
using HG;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Skills;
using System.Collections;
using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;
using EntityStates.Railgunner.Weapon;
using UnityEngine.UI;
using BepInEx.Configuration;
using RoR2.UI;
using MonoMod.Cil;
using Mono.Cecil.Cil;
using TMPro;
using RoR2.Projectile;
using R2API;

namespace FreeItemFriday.SkillContent
{
    [AddContentPackProvider(ContentGroup.SKILLS, NAME)]
    public class PulseGrenadeContent : BaseContentPackProvider
    {
        const string NAME = "Pulse Grenade";

        public static class Skills
        {
            public static SkillDef RailgunnerElectricGrenade;
        }

        public static class EntityStates
        {
            [TargetTypeName(typeof(FireElectricGrenade))]
            public static EntityStateConfiguration FireElectricGrenade;
        }

        public static class NetworkSoundEvents
        {
            public static NetworkSoundEventDef ElectricGrenadeExpired;
        }

        public static class DamageTypes
        {
            public static DamageAPI.ModdedDamageType Shock2s;
        }

        public override string identifier => "FreeItemFriday.SkillContent.PulseGrenade";

        public override IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            DamageTypes.Shock2s = DamageAPI.ReserveDamageType();
            AddressablesLoadHelper loadHelper = AddressablesLoadHelper.CreateUsingDefaultResourceLocator("ContentPack:" + identifier);
            loadHelper.AddContentPackLoadOperation(contentPack);
            loadHelper.AddLoadOperation<FreeSkillVariant>(FreeSkillVariant.LABEL, FreeSkillVariant.allAssets.Add);
            loadHelper.AddGenericOperation(delegate
            {
                ContentLoadHelper.PopulateTypeFields(typeof(Skills), contentPack.skillDefs);
                ContentLoadHelper.PopulateTypeFields(typeof(EntityStates), contentPack.entityStateConfigurations);
                ContentLoadHelper.PopulateTypeFields(typeof(NetworkSoundEventDef), contentPack.networkSoundEventDefs, fieldName => "nse" + fieldName);
            }, 0.05f);
            loadHelper.AddGenericOperation(delegate
            {
                LanguageSystem.SetArgs(Skills.RailgunnerElectricGrenade.skillDescriptionToken,
                    EntityStates.FireElectricGrenade.BindPercent(config, NAME, "Damage Coefficient", nameof(FireElectricGrenade.damageCoefficient)));
                EntityStates.FireElectricGrenade.BindFloat(config, NAME, "Duration", nameof(FireElectricGrenade.baseDuration));
                EntityStates.FireElectricGrenade.BindFloat(config, NAME, "Firing Delay", nameof(FireElectricGrenade.baseDelayBeforeFiringProjectile));
            }, 0.05f);
            loadHelper.AddGenericOperation(SetOfflineIconAsync);
            loadHelper.AddGenericOperation(ModifyRailgunnerCrosshairAsync);
            loadHelper.AddGenericOperation(CreateGrenadeProjectileAsync);
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }
        }

        public IEnumerator SetOfflineIconAsync()
        {
            var RailgunnerBodyFirePistol = Addressables.LoadAssetAsync<RailgunSkillDef>("RoR2/DLC1/Railgunner/RailgunnerBodyFirePistol.asset");
            while (!RailgunnerBodyFirePistol.IsDone)
            {
                yield return null;
            }
            ((RailgunSkillDef)Skills.RailgunnerElectricGrenade).offlineIcon = RailgunnerBodyFirePistol.Result.offlineIcon;
        }

        public IEnumerator ModifyRailgunnerCrosshairAsync()
        {
            var RailgunnerCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerCrosshair.prefab");
            var RailgunnerCryochargeCrosshair = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerCryochargeCrosshair.prefab");
            var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(new List<AsyncOperationHandle>
            {
                RailgunnerCrosshair,
                RailgunnerCryochargeCrosshair,
            }, true);
            while (!groupOp.IsDone)
            {
                yield return null;
            }
            SetupRailgunnerCrosshair.prefab = Prefab.Clone(RailgunnerCrosshair.Result.transform.Find("Flavor, Ready").gameObject, "Flavor, Grenade Ready");
            Color color = new Color32(198, 169, 217, 255);
            foreach (Image image in SetupRailgunnerCrosshair.prefab.GetComponentsInChildren<Image>())
            {
                image.color = color;
            }
            foreach (TextMeshProUGUI text in SetupRailgunnerCrosshair.prefab.GetComponentsInChildren<TextMeshProUGUI>())
            {
                text.color = color;
            }
            UnityEngine.Object.Instantiate(RailgunnerCryochargeCrosshair.Result.transform.Find("CenterDot").gameObject, SetupRailgunnerCrosshair.prefab.transform);
            RailgunnerCrosshair.Result.AddComponent<SetupRailgunnerCrosshair>();
        }

        public IEnumerator CreateGrenadeProjectileAsync()
        {
            var EngiGrenadeProjectile = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiGrenadeProjectile.prefab");
            var CaptainTazerSupplyDropNova = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Captain/CaptainTazerSupplyDropNova.prefab");
            var nseCommandoGrenadeBounce = Addressables.LoadAssetAsync<NetworkSoundEventDef>("RoR2/Base/Commando/nseCommandoGrenadeBounce.asset");
            var EngiGrenadeGhost = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Engi/EngiGrenadeGhost.prefab");
            var texRailgunnerElectricGrenade = Addressables.LoadAssetAsync<Texture>("FreeItemFriday/SkillContent/PulseGrenade/texRailgunnerElectricGrenade.png");
            var texRailgunnerElectricGrenadeAlt = Addressables.LoadAssetAsync<Texture>("FreeItemFriday/SkillContent/PulseGrenade/texRailgunnerElectricGrenadeAlt.png");
            var skinRailGunnerAlt = Addressables.LoadAssetAsync<SkinDef>("RoR2/DLC1/Railgunner/skinRailGunnerAlt.asset");
            var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(new List<AsyncOperationHandle>
            {
                EngiGrenadeProjectile,
                CaptainTazerSupplyDropNova,
                nseCommandoGrenadeBounce,
                EngiGrenadeGhost,
                texRailgunnerElectricGrenade,
                texRailgunnerElectricGrenadeAlt,
                skinRailGunnerAlt
            }, true);
            while (!groupOp.IsDone)
            {
                yield return null;
            }
            var GrenadeProjectile = Prefab.Clone(EngiGrenadeProjectile.Result, "RailgunnerElectricGrenadeProjectile");
            if (GrenadeProjectile.TryGetComponent(out ProjectileDamage projectileDamage))
            {
                projectileDamage.damageType = DamageType.Shock5s;
            }

            var GrenadeExplosionEffect = Prefab.Clone(CaptainTazerSupplyDropNova.Result, "RailgunnerElectricGrenadeExplosion");
            GrenadeExplosionEffect.GetComponent<EffectComponent>().soundName = "Play_roboBall_attack1_explode";
            GrenadeExplosionEffect.GetComponent<VFXAttributes>().vfxPriority = VFXAttributes.VFXPriority.Always;
            contentPack.effectDefs.Add(new[] { new EffectDef(GrenadeExplosionEffect) });

            if (GrenadeProjectile.TryGetComponent(out ProjectileImpactExplosion projectileImpactExplosion))
            {
                projectileImpactExplosion.blastRadius = 5f;
                projectileImpactExplosion.lifetimeAfterImpact = 0.15f;
                projectileImpactExplosion.lifetimeExpiredSound = NetworkSoundEvents.ElectricGrenadeExpired;
                projectileImpactExplosion.offsetForLifetimeExpiredSound = 0.05f;
                projectileImpactExplosion.impactEffect = GrenadeExplosionEffect;
            }
            if (GrenadeProjectile.TryGetComponent(out RigidbodySoundOnImpact rigidbodySoundOnImpact))
            {
                rigidbodySoundOnImpact.impactSoundString = string.Empty;
                rigidbodySoundOnImpact.networkedSoundEvent = nseCommandoGrenadeBounce.Result;
            }
            if (GrenadeProjectile.TryGetComponent(out SphereCollider sphereCollider))
            {
                sphereCollider.radius = 0.8f;
            }
            if (GrenadeProjectile.TryGetComponent(out ProjectileSimple projectileSimple))
            {
                projectileSimple.desiredForwardSpeed = 60f;
            }
            if (GrenadeProjectile.TryGetComponent(out ApplyTorqueOnStart applyTorqueOnStart))
            {
                applyTorqueOnStart.localTorque = Vector3.one * 100f;
            }
            GrenadeProjectile.AddComponent<DamageAPI.ModdedDamageTypeHolderComponent>().Add(DamageTypes.Shock2s);
            UnityEngine.Object.DestroyImmediate(GrenadeProjectile.GetComponent<AntiGravityForce>());

            var GrenadeGhost = Prefab.Clone(EngiGrenadeGhost.Result, "RailgunnerElectricGrenadeGhost");
            if (GrenadeGhost.transform.TryFind("mdlEngiGrenade", out Transform mdlEngiGrenade))
            {
                mdlEngiGrenade.localScale = new Vector3(0.3f, 0.3f, 0.5f);
                if (mdlEngiGrenade.TryGetComponent(out MeshRenderer meshRenderer))
                {
                    Material matRailgunnerElectricGrenade = new Material(meshRenderer.sharedMaterial);
                    matRailgunnerElectricGrenade.SetColor("_EmColor", new Color32(55, 188, 255, 255));
                    matRailgunnerElectricGrenade.SetFloat("_EmPower", 2f);
                    matRailgunnerElectricGrenade.SetTexture("_MainTex", texRailgunnerElectricGrenade.Result);
                    meshRenderer.sharedMaterial = matRailgunnerElectricGrenade;
                }
            }

            GrenadeProjectile.GetComponent<ProjectileController>().ghostPrefab = GrenadeGhost;
            contentPack.projectilePrefabs.Add(new[] { GrenadeProjectile });
            EntityStates.FireElectricGrenade.SetValue(nameof(FireElectricGrenade.projectilePrefab), GrenadeProjectile);

            GameObject grenadeGhostReskin = Prefab.Clone(GrenadeGhost, "RailgunnerElectricGrenadeGhostReskin");
            if (grenadeGhostReskin.transform.TryFind("mdlEngiGrenade", out Transform mdlEngiGrenadeReskin) && mdlEngiGrenadeReskin.TryGetComponent(out MeshRenderer meshRendererReskin))
            {
                Material matRailgunnerElectricGrenadeAlt = new Material(meshRendererReskin.sharedMaterial);
                matRailgunnerElectricGrenadeAlt.SetTexture("_MainTex", texRailgunnerElectricGrenadeAlt.Result);
                meshRendererReskin.sharedMaterial = matRailgunnerElectricGrenadeAlt;
            }

            ArrayUtils.ArrayAppend(ref skinRailGunnerAlt.Result.projectileGhostReplacements, new SkinDef.ProjectileGhostReplacement
            {
                projectilePrefab = GrenadeProjectile,
                projectileGhostReplacementPrefab = grenadeGhostReskin
            });
        }

        public override IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public override IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            IL.RoR2.SetStateOnHurt.OnTakeDamageServer += SetStateOnHurt_OnTakeDamageServer;
            yield break;
        }

        private static void SetStateOnHurt_OnTakeDamageServer(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(MoveType.Before, x => x.MatchCallOrCallvirt<SetStateOnHurt>(nameof(SetStateOnHurt.SetShock))))
            {
                c.Emit(OpCodes.Ldarg, 1);
                c.EmitDelegate<Func<float, DamageReport, float>>((duration, damageReport) =>
                {
                    if (damageReport.damageInfo.HasModdedDamageType(DamageTypes.Shock2s))
                    {
                        return 2f * damageReport.damageInfo.procCoefficient;
                    }
                    return duration;
                });
            }
            else FreeItemFridayPlugin.Logger.LogError($"{nameof(PulseGrenadeContent)}.{nameof(SetStateOnHurt_OnTakeDamageServer)} IL hook failed!");
        }

        public class SetupRailgunnerCrosshair : MonoBehaviour
        {
            public static GameObject prefab;

            public void Awake()
            {
                GameObject instance = Instantiate(prefab, transform);
                if (TryGetComponent(out CrosshairController crosshairController))
                {
                    ArrayUtils.ArrayAppend(ref crosshairController.skillStockSpriteDisplays, new CrosshairController.SkillStockSpriteDisplay
                    {
                        requiredSkillDef = Skills.RailgunnerElectricGrenade,
                        skillSlot = SkillSlot.Primary,
                        target = instance
                    });
                }
                Destroy(this);
            }
        }
    }
}