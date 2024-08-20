using RoR2;
using RoR2.Achievements;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace FreeItemFriday.Achievements
{
    // Match achievement identifiers from 1.6.1
    [TryRegisterAchievement("FSS_RailgunnerEliteSniper", "Skills.Railgunner.FreeItemFriday.PassiveBouncingBullets", null)]
    public class RailgunnerEliteSniperAchievement : BaseEndingAchievement
    {
        public override BodyIndex LookUpRequiredBodyIndex() => BodyCatalog.FindBodyIndex("RailgunnerBody");

        public override bool ShouldGrant(RunReport runReport)
        {
            if (runReport == null || !runReport.gameEnding || !runReport.gameEnding.isWin || runReport.ruleBook.FindDifficulty() < DifficultyIndex.Eclipse1)
            {
                return false;
            }
            RunReport.PlayerInfo playerInfo = runReport.FindPlayerInfo(localUser);
            return playerInfo != null && playerInfo.equipment != null && playerInfo.equipment.Any(x => x == RoR2Content.Equipment.QuestVolatileBattery.equipmentIndex);
        }
    }
}