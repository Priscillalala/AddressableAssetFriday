using BepInEx.Configuration;
using R2API;
using RoR2;
using RoR2.ContentManagement;
using RoR2.Items;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace FreeItemFriday.EquipmentContent
{
    [AddContentPackProvider(ContentGroup.EQUIPMENT, NAME)]
    public class GodlessEyeContent : BaseContentPackProvider
    {
        const string NAME = "Godless Eye";

        public static class Equipment
        {
            public static EquipmentDef DeathEye;
            public static EquipmentDef DeathEyeConsumed;
        }

        public static GameObject DelayedDeathHandler { get; private set; }

        public static ConfigEntry<float> range;
        public static ConfigEntry<float> duration;
        public static ConfigEntry<int> maxConsecutiveEnemies;

        public override string identifier => "FreeItemFriday.EquipmentContent.GodlessEye";

        public override IEnumerator LoadStaticContentAsync(LoadStaticContentAsyncArgs args)
        {
            AddressablesLoadHelper loadHelper = CreateLoadHelper();
            loadHelper.AddContentPackLoadOperation(contentPack);
            loadHelper.AddLoadOperation<KeyAssetRuleSet>(KeyAssetRuleSet.LABEL, KeyAssetRuleSet.allAssets.AddRange);
            loadHelper.AddUpgradeStubbedShadersOperation();
            loadHelper.AddGenericOperation(delegate
            {
                ContentLoadHelper.PopulateTypeFields(typeof(Equipment), contentPack.equipmentDefs);
            }, 0.05f);
            loadHelper.AddGenericOperation(delegate
            {
                range = config.Option(NAME, "Range", 200f);
                duration = config.Option(NAME, "Duration", 2f);
                maxConsecutiveEnemies = config.Option(NAME, "Maximum Consecutive Enemies", 10);
                LanguageSystem.SetArgs(Equipment.DeathEye.descriptionToken, range);
            }, 0.05f);
            loadHelper.AddGenericOperation(LoadModelMaterials);
            loadHelper.AddGenericOperation(CreateDelayedDeathHandler);
            while (loadHelper.coroutine.MoveNext())
            {
                args.ReportProgress(loadHelper.progress.value);
                yield return loadHelper.coroutine.Current;
            }
        }

        public IEnumerator LoadModelMaterials()
        {
            var matMSObeliskLightning = Addressables.LoadAssetAsync<Material>("RoR2/Base/mysteryspace/matMSObeliskLightning.mat");
            var matMSObeliskHeart = Addressables.LoadAssetAsync<Material>("RoR2/Base/mysteryspace/matMSObeliskHeart.mat");
            var matMSStarsLink = Addressables.LoadAssetAsync<Material>("RoR2/Base/mysteryspace/matMSStarsLink.mat");
            var matJellyfishLightning = Addressables.LoadAssetAsync<Material>("RoR2/Base/Common/VFX/matJellyfishLightning.mat");
            var groupOP = Addressables.ResourceManager.CreateGenericGroupOperation(new List<AsyncOperationHandle>
            {
                matMSObeliskLightning,
                matMSObeliskHeart,
                matMSStarsLink,
                matJellyfishLightning
            }, true);
            while (!groupOP.IsDone)
            {
                yield return null;
            }
            GameObject pickupModelPrefab = Equipment.DeathEye.pickupModelPrefab;

            MeshRenderer modelRenderer = pickupModelPrefab.transform.Find("mdlDeathEye").GetComponent<MeshRenderer>();
            Material[] sharedMaterials = modelRenderer.sharedMaterials;
            sharedMaterials[1] = matMSObeliskLightning.Result;
            modelRenderer.sharedMaterials = sharedMaterials;

            pickupModelPrefab.transform.Find("EyeBallFX/Weird Sphere").GetComponent<ParticleSystemRenderer>().sharedMaterial = matMSObeliskHeart.Result;
            pickupModelPrefab.transform.Find("EyeBallFX/LongLifeNoiseTrails, Bright").GetComponent<ParticleSystemRenderer>().trailMaterial = matMSStarsLink.Result;
            pickupModelPrefab.transform.Find("EyeBallFX/Lightning").GetComponent<ParticleSystemRenderer>().sharedMaterial = matJellyfishLightning.Result;
        }

        public IEnumerator CreateDelayedDeathHandler()
        {
            var MSObelisk = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/mysteryspace/MSObelisk.prefab");
            while (!MSObelisk.IsDone)
            {
                yield return null;
            }
            DelayedDeathHandler = Prefab.Clone(MSObelisk.Result.transform.Find("Stage1FX").gameObject, "DelayedDeathHandler");
            DelayedDeathHandler.SetActive(true);
            DelayedDeathHandler.AddComponent<NetworkIdentity>();
            DelayedDeathHandler.AddComponent<DelayedDeathEye>();
            DelayedDeathHandler.AddComponent<DestroyOnTimer>().duration = duration.Value;
            UnityEngine.Object.DestroyImmediate(DelayedDeathHandler.transform.Find("LongLifeNoiseTrails, Bright").gameObject);
            UnityEngine.Object.DestroyImmediate(DelayedDeathHandler.transform.Find("PersistentLight").gameObject);

            contentPack.networkedObjectPrefabs.Add(new[] { DelayedDeathHandler });
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

        public static bool FireDeathEye(EquipmentSlot equipmentSlot)
        {
            if (!equipmentSlot.healthComponent || !equipmentSlot.healthComponent.alive)
            {
                return false;
            }
            Vector3 position = equipmentSlot.characterBody?.corePosition ?? equipmentSlot.transform.position;
            DelayedDeathEye delayedDeathEye = UnityEngine.Object.Instantiate(DelayedDeathHandler, position, Quaternion.identity).GetComponent<DelayedDeathEye>();

            TeamMask teamMask = TeamMask.allButNeutral;
            if (equipmentSlot.teamComponent)
            {
                teamMask.RemoveTeam(equipmentSlot.teamComponent.teamIndex);
            }
            delayedDeathEye.cleanupTeams = teamMask;

            List<DelayedDeathEye.DeathGroup> deathGroups = new List<DelayedDeathEye.DeathGroup>();
            int consecutiveEnemies = 0;
            BodyIndex currentBodyIndex = BodyIndex.None;
            List<CharacterBody> currentVictims = new List<CharacterBody>();
            foreach (CharacterBody body in CharacterBody.readOnlyInstancesList)
            {
                if (teamMask.HasTeam(body.teamComponent.teamIndex) && (body.corePosition - position).sqrMagnitude <= range.Value * range.Value)
                {
                    if (body.bodyIndex != currentBodyIndex || consecutiveEnemies >= maxConsecutiveEnemies.Value)
                    {
                        currentBodyIndex = body.bodyIndex;
                        consecutiveEnemies = 0;
                        if (currentVictims.Count > 0)
                        {
                            deathGroups.Add(new DelayedDeathEye.DeathGroup
                            {
                                victimBodies = new List<CharacterBody>(currentVictims),
                            });
                        }
                        currentVictims.Clear();
                    }
                    currentVictims.Add(body);
                    consecutiveEnemies++;
                }
            }
            if (currentVictims.Count > 0)
            {
                deathGroups.Add(new DelayedDeathEye.DeathGroup
                {
                    victimBodies = new List<CharacterBody>(currentVictims),
                });
            }
            currentVictims.Clear();
            deathGroups.Add(new DelayedDeathEye.DeathGroup
            {
                victimBodies = new List<CharacterBody>() { equipmentSlot.characterBody }
            });
            if (deathGroups.Count > 0)
            {
                float durationBetweenDeaths = duration.Value / deathGroups.Count;
                for (int i = 0; i < deathGroups.Count; i++)
                {
                    DelayedDeathEye.DeathGroup group = deathGroups[i];
                    group.time = Run.FixedTimeStamp.now + (durationBetweenDeaths * i);
                    delayedDeathEye.EnqueueDeath(group);
                }
            }
            NetworkServer.Spawn(delayedDeathEye.gameObject);

            if (equipmentSlot.characterBody?.inventory)
            {
                CharacterMasterNotificationQueue.SendTransformNotification(equipmentSlot.characterBody.master, equipmentSlot.characterBody.inventory.currentEquipmentIndex, Equipment.DeathEyeConsumed.equipmentIndex, CharacterMasterNotificationQueue.TransformationType.Default);
                equipmentSlot.characterBody.inventory.SetEquipmentIndex(Equipment.DeathEyeConsumed.equipmentIndex);
            }
            return true;
        }

        public class DelayedDeathEye : MonoBehaviour
        {
            public struct DeathGroup
            {
                public Run.FixedTimeStamp time;
                public List<CharacterBody> victimBodies;
            }

            public Queue<DeathGroup> deathQueue = new Queue<DeathGroup>();
            public TeamMask cleanupTeams = TeamMask.none;
            private bool hasRunCleanup;
            private GameObject destroyEffectPrefab;

            public void EnqueueDeath(DeathGroup death)
            {
                deathQueue.Enqueue(death);
            }

            public void Awake()
            {
                destroyEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BrittleDeath.prefab").WaitForCompletion();
            }

            public void Start()
            {
                Util.PlaySound("Play_vagrant_R_explode", base.gameObject);
            }

            public void FixedUpdate()
            {
                if (!NetworkServer.active)
                {
                    return;
                }
                if (deathQueue.Count <= 0)
                {
                    RunCleanup();
                    enabled = false;
                    return;
                }
                if (deathQueue.Peek().time.hasPassed)
                {
                    List<CharacterBody> victimBodies = deathQueue.Dequeue().victimBodies;
                    foreach (CharacterBody victim in victimBodies)
                    {
                        DestroyVictim(victim);
                    }
                }
            }

            public void RunCleanup()
            {
                if (hasRunCleanup)
                {
                    return;
                }
                if (CharacterBody.readOnlyInstancesList.Count > 0)
                {
                    for (int i = CharacterBody.readOnlyInstancesList.Count - 1; i >= 0; i--)
                    {
                        CharacterBody body = CharacterBody.readOnlyInstancesList[i];
                        if (body.teamComponent && cleanupTeams.HasTeam(body.teamComponent.teamIndex) && (body.corePosition - transform.position).sqrMagnitude <= range.Value * range.Value)
                        {
                            DestroyVictim(body);
                        }
                    }
                }
                hasRunCleanup = true;
            }

            public void DestroyVictim(CharacterBody victim)
            {
                if (!victim)
                {
                    return;
                }
                if (victim.master)
                {
                    if (victim.master.destroyOnBodyDeath)
                    {
                        Destroy(victim.master.gameObject, 1f);
                    }
                    victim.master.preventGameOver = false;
                }
                EffectManager.SpawnEffect(destroyEffectPrefab, new EffectData
                {
                    origin = victim.corePosition,
                    scale = victim.radius
                }, true);
                Destroy(victim.gameObject);
            }
        }
    }
}