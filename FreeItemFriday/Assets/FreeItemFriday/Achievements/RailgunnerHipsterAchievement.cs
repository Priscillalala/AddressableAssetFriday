using RoR2;
using RoR2.Achievements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace FreeItemFriday.Achievements
{
    // Match achievement identifiers from 1.6.1
    [TryRegisterAchievement("FSS_RailgunnerHipster", "Skills.Railgunner.FreeItemFriday.ElectricGrenade", null)]
    public class RailgunnerHipsterAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex() => BodyCatalog.FindBodyIndex("RailgunnerBody");

        public override void OnInstall()
        {
            base.OnInstall();
            listeningToggle = new ToggleAction(SetListeningTrue, SetListeningFalse);
            Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Teleporters/LunarTeleporter Variant.prefab").WaitForCompletion().AddComponent<IsLunarTeleporter>();
        }

        public override void OnUninstall()
        {
            listeningToggle.SetActive(false);
            base.OnUninstall();
        }

        public class IsLunarTeleporter : MonoBehaviour { }

        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            listeningToggle.SetActive(true);
        }

        public override void OnBodyRequirementBroken()
        {
            listeningToggle.SetActive(false);
            base.OnBodyRequirementBroken();
        }

        private void SetListeningTrue()
        {
            TeleporterInteraction.onTeleporterBeginChargingGlobal += TeleporterInteraction_onTeleporterBeginChargingGlobal;
            TeleporterInteraction.onTeleporterChargedGlobal += TeleporterInteraction_onTeleporterChargedGlobal;
            On.EntityStates.Railgunner.Scope.BaseActive.OnEnter += BaseActive_OnEnter;
        }

        private void SetListeningFalse()
        {
            TeleporterInteraction.onTeleporterBeginChargingGlobal -= TeleporterInteraction_onTeleporterBeginChargingGlobal;
            TeleporterInteraction.onTeleporterChargedGlobal -= TeleporterInteraction_onTeleporterChargedGlobal;
            On.EntityStates.Railgunner.Scope.BaseActive.OnEnter -= BaseActive_OnEnter;
        }

        private void TeleporterInteraction_onTeleporterBeginChargingGlobal(TeleporterInteraction obj)
        {
            hasScoped = false;
        }

        private void TeleporterInteraction_onTeleporterChargedGlobal(TeleporterInteraction obj)
        {
            if (localUser.cachedBody?.healthComponent && localUser.cachedBody.healthComponent.alive && !hasScoped && obj.gameObject.GetComponent<IsLunarTeleporter>())
            {
                Grant();
            }
        }

        private void BaseActive_OnEnter(On.EntityStates.Railgunner.Scope.BaseActive.orig_OnEnter orig, EntityStates.Railgunner.Scope.BaseActive self)
        {
            orig(self);
            if (self.gameObject && self.gameObject == localUser.cachedBodyObject)
            {
                hasScoped = true;
            }
        }

        private ToggleAction listeningToggle;
        private bool hasScoped;
    }
}