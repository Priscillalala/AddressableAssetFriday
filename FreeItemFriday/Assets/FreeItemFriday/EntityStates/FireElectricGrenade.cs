using RoR2.ConVar;
using System.Linq;
using RoR2.PostProcessing;
using UnityEngine.Networking;
using RoR2.HudOverlay;
using RoR2.UI;
using UnityEngine;
using System.Collections.Generic;
using RoR2;
using UnityEngine.AddressableAssets;

namespace EntityStates.Railgunner.Weapon
{
    public class FireElectricGrenade : GenericProjectileBaseState, IBaseWeaponState
    {
        public FireElectricGrenade() : base()
        {
            effectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/MuzzleflashSmokeRing.prefab").WaitForCompletion();
        }

        public override void PlayAnimation(float duration)
        {
            base.PlayAnimation(duration);
            Util.PlayAttackSpeedSound("Play_MULT_R_variant_end", gameObject, attackSpeedStat);
            PlayAnimation("Gesture, Override", "FirePistol", "FirePistol.playbackRate", delayBeforeFiringProjectile);
            //PlayAnimation("Gesture, Override", "WindupSuper", "Super.playbackRate", delayBeforeFiringProjectile);
        }

        public override void DoFireEffects()
        {
            base.DoFireEffects();
            if (isAuthority)
            {
                characterMotor?.ApplyForce(-1000f * GetAimRay().direction, false, false);
            }
            PlayAnimation("Gesture, Override", "FireSniper", "FireSniper.playbackRate", duration - fixedAge);
        }

        public override InterruptPriority GetMinimumInterruptPriority() => InterruptPriority.Skill;

        public bool CanScope() => true;
    }
}
