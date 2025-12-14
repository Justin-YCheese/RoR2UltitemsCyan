using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
//using static RoR2.GenericPickupController;

namespace UltitemsCyan.Items.Tier1
{

    // TODO: Make better sound and visuals
    // Play_env_roach_scatter
    public class FleaBag : ItemBase
    {
        public static ItemDef item;
        private static readonly GameObject FleaOrb = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Tooth/HealPack.prefab").WaitForCompletion();
        //private static GameObject FleaOrbPrefab;
        //public static GameObject FleaEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Beetle/BeetleWardOrbEffect.prefab").WaitForCompletion();
        private static readonly GameObject FleaEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/HealthOrbEffect.prefab").WaitForCompletion();
        private const float fleaDropChance = 3f;
        private const int fleaDropCritMultiplier = 2;

        // For Flea Pickup
        public const float baseBuffDuration = 15f;
        //public const float buffDurationPerItem = 0f; // Increase for buff?

        // For Tick Crit Buff
        public const float baseTickMultiplier = 0f;
        public const float tickPerStack = 16f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Flea Bag";
            if (!CheckItemEnabledConfig(itemName, "White", configs))
            {
                return;
            }
            item = CreateItemDef(
                "FLEABAG",
                itemName,
                "Chance on hit to drop a tick which gives critical chance. Critical Strikes drop more ticks.",
                "<style=cIsDamage>3%</style> chance on hit to drop a bag which gives a max of <style=cIsDamage>16%</style> <style=cStack>(+16% per stack)</style> <style=cIsDamage>critical chance</style> for 15 seconds. <style=cIsDamage>Critical strikes</style> are twice as likely to drop a bag.",
                "Is this movie popcorn? oh, no it isn't",
                ItemTier.Tier1,
                UltAssets.FleaBagSprite,
                UltAssets.FleaBagPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Damage]
            );
        }

        /*/ Mystics Items Star Gazer
        private void loadPrefab()
        {
            FleaOrbPrefab = CreateBlankPrefab("UltitemsCyan_FleaOrb");
            FleaOrbPrefab.AddComponent<NetworkTransform>();
            //HopooShaderToMaterial.Standard.Apply(starPrefab.GetComponentInChildren<Renderer>().sharedMaterial);
            //HopooShaderToMaterial.Standard.Emission(starPrefab.GetComponentInChildren<Renderer>().sharedMaterial, 1f, new Color32(25, 180, 171, 255));

            var trajectory = FleaOrbPrefab.AddComponent<VelocityRandomOnStart>();
            trajectory.baseDirection = Vector3.up;
            trajectory.directionMode = VelocityRandomOnStart.DirectionMode.Hemisphere;
            trajectory.coneAngle = 75;
            trajectory.maxSpeed = 35;
            trajectory.minSpeed = 20;
            trajectory.minAngularSpeed = 0f;
            trajectory.maxAngularSpeed = 0f;

            //
            var setRandomRotation = FleaOrbPrefab.transform.Find("mdlStar").gameObject.AddComponent<SetRandomRotation>();
            setRandomRotation.setRandomXRotation = true;
            setRandomRotation.setRandomYRotation = true;
            setRandomRotation.setRandomZRotation = true;
            ///

            var destroyOnTimer = FleaOrbPrefab.AddComponent<DestroyOnTimer>();
            destroyOnTimer.duration = 5f;
            destroyOnTimer.resetAgeOnDisable = false;

            var blink = FleaOrbPrefab.AddComponent<BeginRapidlyActivatingAndDeactivating>();
            blink.blinkFrequency = 20f;
            blink.delayBeforeBeginningBlinking = destroyOnTimer.duration - 1f;
            blink.blinkingRootObject = FleaOrbPrefab.gameObject; // starPrefab.transform.Find("mdlStar").gameObject;

            var teamFilter = FleaOrbPrefab.GetComponent<TeamFilter>();

            var pickupTrigger = FleaOrbPrefab.transform.Find("PickupTrigger").gameObject;
            pickupTrigger.AddComponent<TeamFilter>();

            var buffPickup = pickupTrigger.AddComponent<FleaPickup>();
            //buffPickup.amount = itemCount;
            buffPickup.baseObject = FleaOrbPrefab;
            buffPickup.teamFilter = teamFilter;
            //buffPickup.pickupEffect = FleaEffect;

            buffPickup.pickupEffect = FleaEffect;
            var effectComponent = buffPickup.pickupEffect.AddComponent<EffectComponent>();
            effectComponent.soundName = "Play_hermitCrab_idle_VO";
            var vfxAttributes = buffPickup.pickupEffect.AddComponent<VFXAttributes>();
            vfxAttributes.vfxIntensity = VFXAttributes.VFXIntensity.Low;
            vfxAttributes.vfxPriority = VFXAttributes.VFXPriority.Low;
            buffPickup.pickupEffect.AddComponent<DestroyOnTimer>().duration = 2f;
            //MysticsItemsContent.Resources.effectPrefabs.Add(buffPickup.pickupEffect);

            var gravitationController = FleaOrbPrefab.transform.Find("GravitationController").gameObject;
            var gravitatePickup = gravitationController.AddComponent<GravitatePickup>();
            gravitatePickup.rigidbody = FleaOrbPrefab.GetComponent<Rigidbody>(); ;
            gravitatePickup.teamFilter = teamFilter;
            gravitatePickup.acceleration = 5f;
            gravitatePickup.maxSpeed = 40f;
        }
        //*/

        protected override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            try
            {
                // If the victum has an inventory
                // and damage isn't rejected?
                if (NetworkServer.active && self && victim && damageInfo.attacker && damageInfo.attacker.GetComponent<CharacterBody>() && damageInfo.attacker.GetComponent<CharacterBody>().inventory
                    && !damageInfo.rejected && damageInfo.procCoefficient > 0f)// && !damageInfo.procChainMask.HasProc(ProcType.LoaderLightning)
                {

                    CharacterBody inflictor = damageInfo.attacker.GetComponent<CharacterBody>();

                    int grabCount = inflictor.inventory.GetItemCountEffective(item);
                    if (grabCount > 0)
                    {
                        //Log.Warning("FleaBag on Hit chance: " + fleaDropChance * (damageInfo.crit ? fleaDropCritMultiplier : 1));
                        if (Util.CheckRoll(fleaDropChance * (damageInfo.crit ? fleaDropCritMultiplier : 1), inflictor.master.luck))
                        {
                            //Log.Warning("Dropping flea from " + victim.name);
                            //RoR2.BuffPickup.Instantiate(item);
                            _ = Util.PlaySound("Play_hermitCrab_idle_VO", victim.gameObject);
                            SpawnOrb(victim.transform.position, victim.transform.rotation, TeamComponent.GetObjectTeam(inflictor.gameObject), grabCount);
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                //Log.Warning("What Flea Hit?");
                //Log.Debug("Victum " + victim.name);
                //Log.Debug("CharacterBody " + victim.GetComponent<CharacterBody>().name);
                //Log.Debug("Inventory " + victim.GetComponent<CharacterBody>().inventory);
                //Log.Debug("Damage rejected? " + damageInfo.rejected);
            }
        }

        // From Mystic Items Utils
        /*public static GameObject CreateBlankPrefab(string name)
        {
            GameObject gameObject = PrefabAPI.InstantiateClone(new GameObject(name), name, false);
            _ = gameObject.AddComponent<NetworkIdentity>();
            //gameObject.AddComponent<NetworkHelper.MysticsRisky2UtilsNetworkHelper>(); // Probably don't need
            PrefabAPI.RegisterNetworkPrefab(gameObject);
            return gameObject;
        }*/

        //
        public static void SpawnOrb(Vector3 position, Quaternion rotation, TeamIndex teamIndex, int itemCount)
        {
            GameObject orb = UnityEngine.Object.Instantiate(FleaOrb);
            if (orb)
            {
                //Log.Debug("Flea Orb is loaded");
            }

            orb.transform.position = position;
            orb.transform.rotation = rotation;
            orb.GetComponent<TeamFilter>().teamIndex = teamIndex;

            // * * Additions * * //
            VelocityRandomOnStart trajectory = orb.GetComponent<VelocityRandomOnStart>();
            trajectory.maxSpeed = 35;
            trajectory.minSpeed = 20;
            trajectory.directionMode = VelocityRandomOnStart.DirectionMode.Hemisphere;
            trajectory.coneAngle = 75;
            //trajectory.maxAngularSpeed = 100;

            //orb.GetComponent<ParticleSystem>().startColor = new Color(0f, 0f, 0f, 1f); // ERROR
            //orb.GetComponent<TrailRenderer>().startColor = new Color(0f, 0f, 0f, 1f); // ERROR
            //orb.GetComponent<TrailRenderer>().endColor = new Color(0f, 0f, 0f, 0f); // ERROR

            //orb.GetComponent<DestroyOnTimer>().duration = 5f;
            //Health Pickup
            HealthPickup healthComponent = orb.GetComponentInChildren<HealthPickup>();
            //Log.Debug("Orb has a Health Pickup");
            //healthComponent.flatHealing = 0;
            //healthComponent.fractionalHealing = 0;
            //Log.Debug("health Component? " + healthComponent.alive); // By default true
            healthComponent.alive = false;

            //BuffPickup
            FleaPickup FleaComponent = healthComponent.gameObject.AddComponent<FleaPickup>();

            //FleaPickup FleaComponent = orb.GetComponentInChildren<>().gameObject.AddComponent<FleaPickup>();
            FleaComponent.amount = itemCount;// ** Will Still Need? **

            FleaComponent.baseObject = orb;
            FleaComponent.teamFilter = orb.GetComponent<TeamFilter>();
            FleaComponent.pickupEffect = FleaEffect;

            orb.GetComponent<Rigidbody>().useGravity = true;
            orb.transform.localScale = Vector3.one * (0.8f + itemCount / 12f);
            //orb.transform.localScale = Vector3.one * (.5f + itemCount / 20);

            //Log.Debug("Spawning orb at: " + orb.transform.position);
            NetworkServer.Spawn(orb);
        }
        //*/
    }

    public class FleaPickup : MonoBehaviour
    {
#pragma warning disable IDE0051 // Remove unused private members
        private void OnTriggerStay(Collider other)
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (NetworkServer.active && alive && TeamComponent.GetObjectTeam(other.gameObject) == teamFilter.teamIndex)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body)
                {
                    //int amountOfStacks = Math.Min(amount, maxStack);
                    float duration = baseBuffDuration; //+ buffDurationPerItem * amount
                    //Log.Debug("Flea On Trigger Happened! amount: " + amount + " duration: " + duration);
                    for (int i = 0; i < amount; i++)
                    {
                        //Log.Debug(" . add tick " + i);
                        body.AddTimedBuff(buffDef, duration, amount);
                    }
                    //EffectManager.SpawnEffect(pickupEffect, new EffectData { origin = transform.position }, true);
                    _ = Util.PlaySound("Play_env_light_flicker", body.gameObject);
                    Destroy(baseObject);
                }
            }
        }

        private readonly BuffDef buffDef = Buffs.TickCritBuff.buff;
        private readonly float baseBuffDuration = FleaBag.baseBuffDuration;
        //private float buffDurationPerItem = FleaBag.buffDurationPerItem;
        //private int maxStack = FleaBag.buffMaxStack; // Was for limiting max number of TickCrit stacks
        public int amount;

        public GameObject baseObject;
        public TeamFilter teamFilter;
        public GameObject pickupEffect;

        private bool alive = true;
    }
}
