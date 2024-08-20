using RoR2;
using RoR2BepInExPack.VanillaFixes;
using System;

namespace FreeItemFriday
{
    public class TryRegisterAchievementAttribute : RegisterAchievementAttribute
    {
		public TryRegisterAchievementAttribute(string identifier, string unlockableRewardIdentifier, string prerequisiteAchievementIdentifier, Type serverTrackerType = null)
            : base (identifier, unlockableRewardIdentifier, prerequisiteAchievementIdentifier, serverTrackerType) { }

        static TryRegisterAchievementAttribute()
        {
            SaferAchievementManager.OnRegisterAchievementAttributeFound += SaferAchievementManager_OnRegisterAchievementAttributeFound;
        }

        private static RegisterAchievementAttribute SaferAchievementManager_OnRegisterAchievementAttributeFound(Type target, RegisterAchievementAttribute attribute)
        {
            if (attribute is TryRegisterAchievementAttribute && !UnlockableCatalog.GetUnlockableDef(attribute.unlockableRewardIdentifier))
            {
                return null;
            }
            return attribute;
        }
    }
}