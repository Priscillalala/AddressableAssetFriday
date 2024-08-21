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
using EntityStates.Toolbot;
using UnityEngine.UI;
using BepInEx.Configuration;

namespace FreeItemFriday.SkillContent
{
    [AddContentPackProvider(ContentGroup.SKILLS, NAME)]
    public class RebootContent : BaseContentPackProvider
    {
        const string NAME = "Reboot";

        public static class Skills
        {
            public static SkillDef ToolbotReboot;
        }

        public static class EntityStates
        {
            [TargetTypeName(typeof(Reboot))]
            public static EntityStateConfiguration Reboot;
        }

        public static ConfigEntry<float> duration;

        public override string identifier => "FreeItemFriday.SkillContent.Reboot";

        public override IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            contentPack.identifier = identifier;
            AddressablesLoadHelper loadHelper = AddressablesLoadHelper.CreateUsingDefaultResourceLocator("ContentPack:" + identifier);
            loadHelper.AddContentPackLoadOperation(contentPack);
            loadHelper.AddLoadOperation<FreeSkillVariant>(FreeSkillVariant.LABEL, FreeSkillVariant.allAssets.Add);
            loadHelper.AddGenericOperation(delegate
            {
                ContentLoadHelper.PopulateTypeFields(typeof(Skills), contentPack.skillDefs);
                ContentLoadHelper.PopulateTypeFields(typeof(EntityStates), contentPack.entityStateConfigurations);
            }, 0.05f);
            loadHelper.AddGenericOperation(delegate
            {
                var duration = config.Option(NAME, "Duration", EntityStates.Reboot, nameof(Reboot.baseDuration));
                LanguageSystem.SetArgs(Skills.ToolbotReboot.skillDescriptionToken, duration);
            }, 0.05f);
            loadHelper.AddGenericOperation(CreateLegacyPrefabsAsync);
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }
        }

        public IEnumerator CreateLegacyPrefabsAsync()
        {
            var RailgunnerOfflineUI = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/RailgunnerOfflineUI.prefab");
            var texRebootUIGear = Addressables.LoadAssetAsync<Sprite>("FreeItemFriday/SkillContent/Reboot/texRebootUIGear.png");
            var Chest1Starburst = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Chest1/Chest1Starburst.prefab");
            var groupOp = Addressables.ResourceManager.CreateGenericGroupOperation(new List<AsyncOperationHandle>
            {
                RailgunnerOfflineUI,
                texRebootUIGear,
                Chest1Starburst
            }, true);
            while (!groupOp.IsDone)
            {
                yield return null;
            }

            GameObject RebootOverlay = Prefab.Clone(RailgunnerOfflineUI.Result, "RebootOverlay");
            Transform barContainer = RebootOverlay.transform.Find("BarContainer");
            barContainer.localPosition = Vector3.zero;
            barContainer.localEulerAngles = Vector3.zero;
            barContainer.transform.Find("Inner").localPosition = Vector3.zero;
            Image backdropImage = barContainer.transform.Find("Inner/FillBarDimensions/Fillbar Backdrop").GetComponent<Image>();
            backdropImage.transform.localScale = Vector3.one * 1.1f;
            backdropImage.color = new Color32(0, 0, 0, 200);
            Image fillbarImage = barContainer.transform.Find("Inner/FillBarDimensions/FillBar").GetComponent<Image>();
            fillbarImage.sprite = texRebootUIGear.Result;
            fillbarImage.color = Color.white;
            UnityEngine.Object.DestroyImmediate(barContainer.transform.Find("SoftGlow").gameObject);
            UnityEngine.Object.DestroyImmediate(barContainer.transform.Find("Inner/SpinnySquare").gameObject);

            GameObject VentEffect = Prefab.Clone(Chest1Starburst.Result, "ToolbotVentEffect");
            UnityEngine.Object.DestroyImmediate(VentEffect.transform.Find("Dust").gameObject);
            UnityEngine.Object.DestroyImmediate(VentEffect.transform.Find("BurstLight").gameObject);
            UnityEngine.Object.DestroyImmediate(VentEffect.transform.Find("Beams").gameObject);
            EffectComponent effectComponent = VentEffect.AddComponent<EffectComponent>();
            effectComponent.soundName = "Play_env_geyser_launch";
            VFXAttributes vFXAttributes = VentEffect.AddComponent<VFXAttributes>();
            vFXAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vFXAttributes.vfxPriority = VFXAttributes.VFXPriority.Medium;
            contentPack.effectDefs.Add(new[] { new EffectDef(VentEffect) });

            EntityStates.Reboot.SetValue(nameof(Reboot.hudOverlayPrefab), RebootOverlay);
            EntityStates.Reboot.SetValue(nameof(Reboot.cleanseBodyEffectPrefab), VentEffect);
        }

        public override IEnumerator GenerateContentPackAsync(GetContentPackAsyncArgs args)
        {
            ContentPack.Copy(contentPack, args.output);
            yield break;
        }

        public override IEnumerator FinalizeAsync(FinalizeAsyncArgs args)
        {
            yield break;
        }
    }
}