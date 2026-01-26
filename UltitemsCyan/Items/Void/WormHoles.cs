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

        // The worm hole visually is shrinking when spawned (doesn't actually change anything visually (zone looks like is spawns infinitly large)
        //public const int wormInitialVisualRadius = 32;
        // Initial size is 15 + 3m per stack
        public const int wormBaseRadius = 12;
        public const int wormRadiusPerItem = 3;

        // How many seconds each worm last
        public const int wormDuration = 8;
        // Size of the center's orb (changes scale of void area too so is annoying to add)
        //public const float emitterOrbScaleMultiplier = 0.72f;
        public const float emitterOrbVerticalOffset = -2.15f;

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
                "Hits that deal <style=cIsDamage>more than 400% damage</style> also create a <style=cIsVoid>void bubble</style> of <style=cIsUtility>15m</style> <style=cStack>(+3m per stack)</style> that last for <style=cIsUtility>8</style> seconds. Can have <style=cIsUtility>1</style> <style=cStack>(+1 per stack)</style> bubble at a time. <style=cIsVoid>Corrupts all Grapevine</style>.",
                "Get it? It's a worm with holes!",
                ItemTier.VoidTier3,
                UltAssets.WormHolesSprite,
                UltAssets.WormHolesPrefab,
                [ItemTag.Damage, ItemTag.Utility],
                Grapevine.item
            );
            CreateWormZoneGameObject();
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
                if (NetworkServer.active && self && victim && damageInfo.attacker.GetComponent<CharacterBody>() && damageInfo.attacker.GetComponent<CharacterBody>().inventory && !damageInfo.rejected && damageInfo.damageType != DamageType.DoT)
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
                Log.Debug(" worm worm --- Too many worms DO NOT Spawn");
                return;
            }
            // If it is too soon to add another worm
            else if (!HoleToken.GetCooldownReady())
            {
                Log.Debug(" worm worm --- Too fast! There's a worm cooldown");
                return;
            }

            // Start Creating Worm Zone !!!
            GameObject wormZoneInstance = UnityEngine.Object.Instantiate(wormZone, position, Quaternion.identity);
            //GameObject wormZoneInstance = UnityEngine.Object.Instantiate(wormZone, position, Quaternion.identity);

            wormZoneInstance.SetActive(true);

            //SphereZone sphere = wormZoneInstance.transform.Find("Camp 1 - Void Monsters & Interactables").GetComponent<SphereZone>();
            //sphere.radius = wormBaseRadius + wormRadiusPerItem * grabCount;
            //sphere.Networkradius = wormBaseRadius + wormRadiusPerItem * grabCount;

            // Calculate and set syncedRadius
            Log.Warning(" | | | server set radius  | | |");
            WormHoleSync wormSyncSize = wormZoneInstance.GetComponent<WormHoleSync>();
            SphereZone sphere = wormZoneInstance.transform.Find("Camp 1 - Void Monsters & Interactables").GetComponent<SphereZone>();

            Log.Warning(" | | | sync: " + wormSyncSize.syncedRadius + " old radius: " + sphere.radius + " | | |");

            wormSyncSize.syncedRadius = wormBaseRadius + wormRadiusPerItem * grabCount;
            wormSyncSize.OnRadiusChanged(wormSyncSize.syncedRadius);
            
            Log.Warning(" | | | sync: " + wormSyncSize.syncedRadius + " NEW radius: " + sphere.radius + " | | |");

            // Add zone to owned zones
            HoleToken.EnqueueWorm(wormZoneInstance);
            NetworkServer.Spawn(wormZoneInstance);
        }

        // Should be Ran once to create Worm Zone Object
        private void CreateWormZoneGameObject()
        {
            //wormZone = UnityEngine.Object.Instantiate(voidCamp);
            wormZone = R2API.PrefabAPI.InstantiateClone(voidCamp, "UltitemsWormHoleZone", false);

            wormZone.SetActive(false);

            wormZone.transform.localScale = new Vector3(1f, 1f, 1f);
            wormZone.transform.rotation = Quaternion.identity;
            wormZone.transform.position = new Vector3(0f, 0f, 0f);

            UnityEngine.Object.Destroy(wormZone.GetComponent<OutsideInteractableLocker>());

            EntityStateMachine stateMachine = wormZone.GetComponent<EntityStateMachine>();
            stateMachine.initialStateType = new SerializableEntityStateType(typeof(IdleWorm));
            stateMachine.mainStateType = new SerializableEntityStateType(typeof(IdleWorm));

            Transform modelCenterEmitter = wormZone.transform.Find("mdlVoidFogEmitter");
            if (modelCenterEmitter)
            {
                // Note: Scaling a sphere by radius means multiplying by 2 (diameter)
                // or adjusting based on the original prefab's base scale.
                //modelCenterEmitter.localScale = Vector3.one * emitterOrbScaleMultiplier;
                Animator animator = modelCenterEmitter.GetComponent<Animator>();
                if (animator)
                {
                    //Log.Debug(" worm worm --- Found and shifted animator of orb");
                    animator.rootPosition = new Vector3(0, emitterOrbVerticalOffset, 0);
                }
                Transform modelBase = modelCenterEmitter.transform.Find("mdlVoidFogEmitterBase");
                if (modelBase)
                {
                    //Log.Debug(" worm worm --- Found and deactivate mdlVoidFogEmitter base");
                    modelBase.gameObject.SetActive(false);
                    //UnityEngine.Object.Destroy(modelBase.gameObject);
                }
            }
            

            //UnityEngine.Object.Destroy(wormZone.gameObject.GetComponent("mdlVoidFogEmitter").gameObject.GetComponent("mdlVoidFogEmitterBase"));

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
                //UnityEngine.Object.Destroy(voidDecal.gameObject);
            }

            //UnityEngine.Object.Destroy(wormZone.gameObject.GetComponent("Decal"));

            Transform CampOne = wormZone.transform.Find("Camp 1 - Void Monsters & Interactables");
            if (CampOne)
            {
                Log.Debug(" worm worm --- Found and deactivate Camp One Director");
                UnityEngine.Object.Destroy(CampOne.gameObject.GetComponent<CampDirector>());
                UnityEngine.Object.Destroy(CampOne.gameObject.GetComponent<CombatDirector>());

                Log.Debug(" worm worm --- SphereZone");
                //SphereZone sphere = CampOne.gameObject.GetComponent<SphereZone>();
                //sphere.radius = wormInitialVisualRadius;
                //sphere.Networkradius = wormInitialVisualRadius;

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
                //fog.initialSafeZones = [sphere];
            }

            _ = wormZone.AddComponent<WormHoleSync>();

            Transform CampTwo = wormZone.transform.Find("Camp 2 - Flavor Props & Void Elites");
            if (CampTwo)
            {
                Log.Debug(" worm worm --- Found and deactivate Camp Two Director");
                CampTwo.gameObject.SetActive(false);
                //UnityEngine.Object.Destroy(CampTwo.gameObject);
            }

            //if (wormZone.GetComponent<NetworkIdentity>())
            //{
            //    // We access the internal field to clear it. 
            //    // This prevents the new prefab from "thinking" it's still a VoidCamp.
            //    System.Reflection.FieldInfo assetIdField = typeof(NetworkIdentity).GetField("m_AssetId",
            //        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            //    assetIdField.SetValue(wormZone.GetComponent<NetworkIdentity>(), NetworkHash128.Parse("0"));
            //}

            //UnityEngine.Object.Destroy(wormZone.gameObject.GetComponent("Camp 2 - Flavor Props & Void Elites"));

            //_ = wormZone.AddComponent<WormHoleTimer>();

            //Log.Debug(" worm worm --- Done Modifications, Now Register");

            R2API.PrefabAPI.RegisterNetworkPrefab(wormZone);
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
                //Log.Debug(" | | | Destroyed Worm Token | | |");
                while (ownedVoidHoles.Count > 0)
                {
                    GameObject hole = ownedVoidHoles.Dequeue();
                    Destroy(hole);
                    NetworkServer.Destroy(hole);
                }
                //Log.Debug(" | | | Destroyed Worm Token End | | |");
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

        public class WormHoleSync : NetworkBehaviour
        {
            // SyncVar with a hook to update the child when the value arrives
            //[SyncVar (hook = nameof(OnRadiusChanged))]
            [SyncVar] public float syncedRadius;

            [Server]
            public void OnRadiusChanged(float newRadius)
            {
                //Log.Warning(" | | | worm worm setting safezone for Worm Hole Fog | | |");
                syncedRadius = newRadius;
                // Find the child and apply the radius locally on the client
                Transform campOne = transform.Find("Camp 1 - Void Monsters & Interactables");
                if (campOne)
                {
                    //Log.Warning(" | | | worm worm camp 1 | | |");
                    SphereZone sphere = campOne.GetComponent<SphereZone>();
                    if (sphere)
                    {
                        sphere.radius = newRadius;
                        sphere.Networkradius = newRadius;
                        //Log.Warning(" | | | worm setting safe zone | | |");
                        campOne.gameObject.GetComponent<FogDamageController>().safeZones = [sphere];
                    }
                }
            }
        }
    }
}