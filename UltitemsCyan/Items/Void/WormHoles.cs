using RoR2;
using BepInEx.Configuration;
using UltitemsCyan.Items.Tier3;
using System;
using UnityEngine.AddressableAssets;
using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Networking;
using EntityStates;

namespace UltitemsCyan.Items.Void
{

    // TODO: check if Item classes needs to be public
    public class WormHoles : ItemBase
    {
        public static ItemDef item;
        public static ItemDef transformItem;

        private readonly GameObject voidCamp = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidCamp/VoidCamp.prefab").WaitForCompletion();
        private GameObject wormZone;

        public const int wormBaseRadius = 12;
        public const int wormRadiusPerItem = 3;

        // How long each worm last
        public const int wormDuration = 8;

        public const float wormFogTickPeriod = 0.3f;  // Tick Timer (default 0.5)
        public const float wormFogDamage = 0.056f;    // Percent Health Damage (default 0.025)
        public const float wormFogDamageRamp = -.51f; // Percent Health Damage Ramp, Negative so field deals less damage over time (default 0.1)
        public const float wormFogRampCooldown = 1;   // this is the time until Ramp damage
        // These Values are so that an enemy that says in the hole the entire time takes just over 25% of their health

        // How long until another worm can be spawned (relevant if you have multiple stacks)
        public const float wormCooldown = 0.12f;

        public const int wormsHolesPerStack = 1;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Worm Holes";
            if (!CheckItemEnabledConfig(itemName, "Void", configs))
            {
                return;
            }
            item = CreateItemDef(
                "WORMHOLES",
                itemName,
                "High damage hits also create a void bubble. <style=cIsVoid>Corrupts all Grapevine</style>.",
                "Hits that deal more than 400% damage also create a void bubble of 15m (+3m per stack) that last for 8 seconds. Can have 1 (+1 per stack) bubble at a time. <style=cIsVoid>Corrupts all Grapevine</style>.",
                "Get it? It's a worm with holes!",
                ItemTier.VoidTier3,
                UltAssets.WormHolesSprite,
                UltAssets.WormHolesPrefab,
                [ItemTag.Damage, ItemTag.Utility],
                Grapevine.item
            );
            _ = R2API.ContentAddition.AddEntityState<IdleWorm>(out _);
        }

        protected override void Hooks()
        {
            // Remove a blank seed collapse message
            On.RoR2.Chat.SendBroadcastChat_ChatMessageBase += Chat_SendBroadcastChat_ChatMessageBase;
            // Spawn worm if 400% damage attack
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        // A collapsed seed sends a message which I can edit to be null, but the int method doesn't check empty string
        private void Chat_SendBroadcastChat_ChatMessageBase(On.RoR2.Chat.orig_SendBroadcastChat_ChatMessageBase orig, ChatMessageBase message)
        {
            if (message != null)
            {
                if (message is Chat.SimpleChatMessage simpleMessage && simpleMessage.baseToken == null)
                {
                    return;
                }
            }
            orig(message);
        }

        // Spawn worm if 400% damage attack
        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);

            try
            {
                if (self && victim && damageInfo.attacker.GetComponent<CharacterBody>() && damageInfo.attacker.GetComponent<CharacterBody>().inventory && !damageInfo.rejected && damageInfo.damageType != DamageType.DoT)
                {
                    CharacterBody inflictor = damageInfo.attacker.GetComponent<CharacterBody>();
                    int grabCount = inflictor.inventory.GetItemCountEffective(item);
                    if (grabCount > 0 && damageInfo.damage / inflictor.damage >= 4f)
                    {
                        // If damage greater than 400% then create void bubble
                        TrySpawnHole(inflictor, grabCount, damageInfo.position);
                    }
                }
            }
            catch (NullReferenceException)
            {
                //Log.Warning("???What Worm Holes?");
            }
        }

        private void TrySpawnHole(CharacterBody attacker, int grabCount, Vector3 position)
        {
            // Create my worm zone GameObject
            if (wormZone == null)
            {
                CreateWormZoneGameObject(position);
            }

            // Get worm zones the player has
            WormHoleToken HoleToken = attacker.gameObject.GetComponent<WormHoleToken>();
            if (!HoleToken)
            {
                HoleToken = attacker.gameObject.AddComponent<WormHoleToken>();
            }

            // If adding an extra hole goes over the max active
            // Also GetCleanCount removes old zones
            if (HoleToken.GetCleanCount() + 1 > wormsHolesPerStack * grabCount)
            {
                //Log.Debug(" worm worm --- Too many worms DO NOT Spawn");
                return;
            }
            // If it is too soon to add another worm
            else if (!HoleToken.GetCooldownReady())
            {
                //Log.Debug(" worm worm --- Too fast! There's a worm cooldown");
                return;
            }

            // Start Creating Worm Zone !!!
            GameObject wormZoneInstance = UnityEngine.Object.Instantiate(wormZone, position, Quaternion.identity);
            wormZoneInstance.SetActive(true);

            SphereZone sphere = wormZoneInstance.transform.Find("Camp 1 - Void Monsters & Interactables").GetComponent<SphereZone>();
            sphere.radius = wormBaseRadius + wormRadiusPerItem * grabCount;
            sphere.Networkradius = wormBaseRadius + wormRadiusPerItem * grabCount;

            // Add zone to owned zones
            HoleToken.EnqueueWorm(wormZoneInstance);
            NetworkServer.Spawn(wormZoneInstance);
        }

        // Should be Ran once to create Worm Zone Object
        private void CreateWormZoneGameObject(Vector3 position)
        {
            wormZone = UnityEngine.Object.Instantiate(voidCamp);

            wormZone.SetActive(false);

            wormZone.transform.localScale = new Vector3(1f, 1f, 1f);
            wormZone.transform.rotation = Quaternion.identity;
            wormZone.transform.position = new Vector3(0f, 0f, 0f);

            UnityEngine.Object.Destroy(wormZone.GetComponent<OutsideInteractableLocker>());

            EntityStateMachine stateMachine = wormZone.GetComponent<EntityStateMachine>();
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(IdleWorm));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(IdleWorm));

            Transform modelCenterEmitter = wormZone.transform.Find("mdlVoidFogEmitter");
            Transform modelBase = modelCenterEmitter.transform.Find("mdlVoidFogEmitterBase");
            if (modelBase)
            {
                //Log.Debug(" worm worm --- Found and deactivate mdlVoidFogEmitter base");
                modelBase.gameObject.SetActive(false);
            }

            // Doesn't Work for some reason...
            //Transform modelSphere = modelCenterEmitter.transform.Find("mdlVoidFogEmitterSphere");
            //if (modelSphere)
            //{
            //    //Log.Debug(" worm worm --- Found and moved Sphere");
            //    // -11 y for offset of Sphere model
            //    modelSphere.position = new Vector3(position.x, position.y - 11f, position.z);
            //}

            Transform voidDecal = wormZone.transform.Find("Decal");
            if (voidDecal)
            {
                //Log.Debug(" worm worm --- Found and deactivate Decal");
                voidDecal.gameObject.SetActive(false);
            }

            Transform CampOne = wormZone.transform.Find("Camp 1 - Void Monsters & Interactables");
            if (CampOne)
            {
                //Log.Debug(" worm worm --- Found and deactivate Camp One Director");
                UnityEngine.Object.Destroy(CampOne.gameObject.GetComponent<CampDirector>());
                UnityEngine.Object.Destroy(CampOne.gameObject.GetComponent<CombatDirector>());

                //Log.Debug(" worm worm --- SphereZone");
                SphereZone sphere = CampOne.gameObject.GetComponent<SphereZone>();
                sphere.radius = wormBaseRadius;
                sphere.Networkradius = wormBaseRadius;

                TeamFilter filter = CampOne.gameObject.GetComponent<TeamFilter>();
                filter.teamIndexInternal = (int)TeamIndex.None;
                filter.defaultTeam = TeamIndex.None;

                FogDamageController fog = CampOne.gameObject.GetComponent<FogDamageController>();
                fog.teamFilter = filter;
                fog.invertTeamFilter = true;

                fog.tickPeriodSeconds = wormFogTickPeriod;
                fog.dangerBuffDuration = wormFogTickPeriod + .1f;
                fog.healthFractionPerSecond = wormFogDamage;
                fog.healthFractionRampCoefficientPerSecond = wormFogDamageRamp;
                fog.healthFractionRampIncreaseCooldown = wormFogRampCooldown;
                fog.initialSafeZones = [sphere];
            }

            Transform CampTwo = wormZone.transform.Find("Camp 2 - Flavor Props & Void Elites");
            if (CampTwo)
            {
                //Log.Debug(" worm worm --- Found and deactivate Camp Two Director");
                CampTwo.gameObject.SetActive(false);
            }

            //_ = wormZone.AddComponent<WormHoleTimer>();
        }

        public class WormHoleToken : MonoBehaviour
        {
            private readonly float summonWormCooldown = wormCooldown;
            private float summonWormStopwatch = 0; // Zero so initial check passes

            public Queue<GameObject> ownedVoidHoles = [];

            public void EnqueueWorm(GameObject worm)
            {
                summonWormStopwatch = Run.instance.time;
                ownedVoidHoles.Enqueue(worm);
            }

            // Removes any null holes then returns count
            public int GetCleanCount()
            {
                // If Timer ended and Back to Idle (after deactive) then remove from queue
                // Apparently the idle state for void camps after deactivation is the generic idle and not void camp idle
                while (ownedVoidHoles.Count > 0 && ownedVoidHoles.Peek().GetComponent<EntityStateMachine>().state is Idle)
                {
                    //Log.Debug(" worm worm --- clean state | end");
                    GameObject hole = ownedVoidHoles.Dequeue();
                    Destroy(hole);
                    NetworkServer.Destroy(hole);
                }
                return ownedVoidHoles.Count;
            }

            public bool GetCooldownReady()
            {
                return Run.instance.time > summonWormStopwatch + summonWormCooldown;
            }

            public void Destory()
            {
                Log.Debug(" | | | Destroyed Worm Token | | |");
                while (ownedVoidHoles.Count > 0)
                {
                    GameObject hole = ownedVoidHoles.Dequeue();
                    Destroy(hole);
                    NetworkServer.Destroy(hole);
                }
                Log.Debug(" | | | Destroyed Worm Token End | | |");
            }
        }

        // To remove the objective tracker being added
        public class IdleWorm : EntityStates.VoidCamp.Idle
        {
            private float wormStopwatch = float.PositiveInfinity; // Infinity so that worm cannot instantly deactivate

            public override void OnEnter()
            {
                //base.OnEnter();
                RoR2.Audio.LoopSoundDef loopDef = ScriptableObject.CreateInstance<RoR2.Audio.LoopSoundDef>();
                loopDef.startSoundName = "Play_voidBarnacle_idle_loop";
                loopDef.stopSoundName = "Stop_voidBarnacle_idle_loop";
                
                loopPtr = RoR2.Audio.LoopSoundManager.PlaySoundLoopLocal(gameObject, loopDef);
                indicatedNetIds = new HashSet<NetworkInstanceId>();
                wormStopwatch = Run.instance.time;
            }

            public override void FixedUpdate()
            {
                //Log.Debug(" | | | Dictionary Timer: " + gameObject.GetComponent<FogDamageController>().dictionaryValidationTimer);
                
                if (Run.instance.time > wormStopwatch + wormDuration)
                {
                    //Log.Debug(" | | | TIME OVER | | | | | | | | |");
                    DeactivateWorm();
                }
            }

            public override void OnExit()
            {
                RoR2.Audio.LoopSoundManager.StopSoundLoopLocal(loopPtr);
            }
            public void DeactivateWorm()
            {
                //Log.Debug(" | | | Deactivate Worm ");
                outer.SetNextState(new EntityStates.VoidCamp.Deactivate
                {
                    completeObjectiveChatMessageToken = null
                });
            }
        }
    }
}