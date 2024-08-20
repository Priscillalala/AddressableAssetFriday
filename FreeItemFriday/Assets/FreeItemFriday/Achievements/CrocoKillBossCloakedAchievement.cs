using RoR2;
using RoR2.Achievements;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

namespace FreeItemFriday.Achievements
{
    // Match achievement identifiers from 1.6.1
    [TryRegisterAchievement("FSS_CrocoKillBossCloaked", "Skills.Croco.FreeItemFriday.PassiveToxin", "BeatArena", typeof(ServerAchievement))]
    public class CrocoKillBossCloakedAchievement : BaseAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex() => BodyCatalog.FindBodyIndex("CrocoBody");

        public override void OnBodyRequirementMet()
        {
            base.OnBodyRequirementMet();
            SetServerTracked(true);
        }

        public override void OnBodyRequirementBroken()
        {
            SetServerTracked(false);
            base.OnBodyRequirementBroken();
        }

        public class ServerAchievement : BaseServerAchievement
        {
            public override void OnInstall()
            {
                base.OnInstall();
                GlobalEventManager.onCharacterDeathGlobal += GlobalEventManager_onCharacterDeathGlobal;
            }

            public override void OnUninstall()
            {
                GlobalEventManager.onCharacterDeathGlobal -= GlobalEventManager_onCharacterDeathGlobal;
                base.OnUninstall();
            }

            private void GlobalEventManager_onCharacterDeathGlobal(DamageReport damageReport)
            {
                if (damageReport.victimIsChampion && networkUser.master == damageReport.attackerMaster)
                {
                    CharacterBody currentBody = GetCurrentBody();
                    if (currentBody.GetVisibilityLevel(damageReport.victimTeamIndex) <= VisibilityLevel.Cloaked)
                    {
                        Grant();
                    }
                }
            }
        }
    }
}