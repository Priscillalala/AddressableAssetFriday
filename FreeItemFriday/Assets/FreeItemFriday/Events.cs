using System;
using UnityEngine;
using RoR2;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2.Projectile;

namespace FreeItemFriday
{
    public static class Events
    {
        public static class GlobalEventManager
        {
            public delegate void HitEnemyAcceptedDelegate(DamageInfo damageInfo, GameObject victim, uint? dotMaxStacksFromAttacker);

            private static event HitEnemyAcceptedDelegate _onHitEnemyAcceptedServer;
            private static bool set_onHitEnemyAcceptedServer;

            /// <summary>
            /// Occurs during <see cref="RoR2.GlobalEventManager.OnHitEnemy(DamageInfo, GameObject)"/>, after the initial checks have been passed.
            /// </summary>
            public static event HitEnemyAcceptedDelegate onHitEnemyAcceptedServer 
            {
                add
                {
                    if (!set_onHitEnemyAcceptedServer)
                    {
                        IL.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
                        set_onHitEnemyAcceptedServer = true;
                    }
                    _onHitEnemyAcceptedServer += value;
                }
                remove
                {
                    _onHitEnemyAcceptedServer -= value;
                }
            }

            private static void GlobalEventManager_OnHitEnemy(ILContext il)
            {
                ILCursor c = new ILCursor(il);

                int locMaxStacksFromAttackerIndex = -1;
                if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdloca(out locMaxStacksFromAttackerIndex),
                    x => x.MatchLdloc(out _),
                    x => x.MatchLdfld<ProjectileDamage>(nameof(ProjectileDamage.dotMaxStacksFromAttacker))
                    ) && c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.attacker)),
                    x => x.MatchCallOrCallvirt<GameObject>(nameof(GameObject.GetComponent))
                    ))
                {
                    c.MoveAfterLabels();
                    c.Emit(OpCodes.Ldarg, 1);
                    c.Emit(OpCodes.Ldarg, 2);
                    c.Emit(OpCodes.Ldloc, locMaxStacksFromAttackerIndex);
                    c.EmitDelegate<Action<DamageInfo, GameObject, uint?>>((damageInfo, victim, maxStackFromAttacks) =>
                    {
                        _onHitEnemyAcceptedServer?.Invoke(damageInfo, victim, maxStackFromAttacks);
                    });
                }
                else Debug.LogError($"{nameof(Events)}: {nameof(GlobalEventManager_OnHitEnemy)} IL hook failed!");
            }
        }

        public static class HealthComponent
        {
            public delegate void ModifyDamageDelegate(RoR2.HealthComponent victim, DamageInfo damageInfo, ref float damage);

            private static event ModifyDamageDelegate _onModifyDamageServer;
            private static bool set_onModifyDamageServer;

            /// <summary>
            /// Occurs during <see cref="RoR2.HealthComponent.TakeDamage(DamageInfo)"/> when damage modifiers (e.g., Focus Crystal, Armor-Piercing Rounds) are being applied.
            /// </summary>
            public static event ModifyDamageDelegate onModifyDamageServer
            {
                add
                {
                    if (!set_onModifyDamageServer)
                    {
                        IL.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
                        set_onModifyDamageServer = true;
                    }
                    _onModifyDamageServer += value;
                }
                remove
                {
                    _onModifyDamageServer -= value;
                }
            }

            private static void HealthComponent_TakeDamage(ILContext il)
            {
                ILCursor c = new ILCursor(il);

                int locDamageIndex = -1;
                if (c.TryGotoNext(MoveType.After,
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damage)),
                    x => x.MatchStloc(out locDamageIndex)
                    ) && c.TryGotoNext(MoveType.Before,
                    x => x.MatchLdarg(1),
                    x => x.MatchLdfld<DamageInfo>(nameof(DamageInfo.damageType)),
                    x => x.MatchLdcI4((int)DamageType.WeakPointHit),
                    x => x.MatchAnd(),
                    x => x.MatchBrfalse(out _),
                    x => x.MatchLdloc(locDamageIndex)
                    ))
                {
                    c.MoveAfterLabels();
                    c.Emit(OpCodes.Ldarg, 0);
                    c.Emit(OpCodes.Ldarg, 1);
                    c.Emit(OpCodes.Ldloc, locDamageIndex);
                    c.EmitDelegate<Func<RoR2.HealthComponent, DamageInfo, float, float>>((victim, damageInfo, damage) =>
                    {
                        _onModifyDamageServer?.Invoke(victim, damageInfo, ref damage);
                        return damage;
                    });
                    c.Emit(OpCodes.Stloc, locDamageIndex);
                }
                else Debug.LogError($"{nameof(Events)}: {nameof(HealthComponent_TakeDamage)} IL hook failed!");
            }
        }
    }
}