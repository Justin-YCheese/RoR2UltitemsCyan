using BepInEx.Configuration;
using Rewired.Utils;
using RoR2;
using System.Collections.Generic;
using UnityEngine;

namespace UltitemsCyan.Items.Tier1
{

    // TODO: check if Item classes needs to be public
    public class ToyRobot : ItemBase
    {
        public static ItemDef item;

        private const float pickupRange = 10f;
        private const float minPickupChance = 10f;
        private const float ratioPickupChance = 80f;

        private const float barrierGained = 8f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Toy Robot";
            if (!CheckItemEnabledConfig(itemName, "White", configs))
            {
                return;
            }
            item = CreateItemDef(
                "TOYROBOT",
                itemName,
                "Grab pickups from further away. Gain temporary barrier from pickups.",
                "Pull in pickups from <style=cIsUtility>20m</style> <style=cStack>(+10m per stack)</style> away. Gain <style=cIsHealing>6</style> <style=cStack>(+6 per stack)</style> <style=cIsHealing>temporary barrier</style> from pickups.",
                "They march to you like a song carriers their steps. More robots have a weaker pull",
                ItemTier.Tier1,
                UltAssets.ToyRobotSprite,
                UltAssets.ToyRobotPrefab,
                [ItemTag.Utility, ItemTag.Healing]
            );
        }


        protected override void Hooks()
        {
            // Barrier from pickups
            On.RoR2.HealthPickup.OnTriggerStay += HealthPickup_OnTriggerStay;
            On.RoR2.ElusiveAntlersPickup.OnTriggerStay += ElusiveAntlersPickup_OnTriggerStay;
            On.RoR2.MoneyPickup.OnTriggerStay += MoneyPickup_OnTriggerStay;
            On.RoR2.BuffPickup.OnTriggerStay += BuffPickup_OnTriggerStay;
            On.RoR2.AmmoPickup.OnTriggerStay += AmmoPickup_OnTriggerStay;
            // ToyRobot Behaviour
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        //https://github.com/TheMysticSword/MysticsItems/blob/main/Items/Tier2/ExplosivePickups.cs
        private void HealthPickup_OnTriggerStay(On.RoR2.HealthPickup.orig_OnTriggerStay orig, HealthPickup self, Collider other)
        {
            orig(self, other);
            CheckBarrier(other);
        }

        private void ElusiveAntlersPickup_OnTriggerStay(On.RoR2.ElusiveAntlersPickup.orig_OnTriggerStay orig, ElusiveAntlersPickup self, Collider other)
        {
            orig(self, other);
            CheckBarrier(other);
        }

        private void MoneyPickup_OnTriggerStay(On.RoR2.MoneyPickup.orig_OnTriggerStay orig, MoneyPickup self, Collider other)
        {
            orig(self, other);
            CheckBarrier(other);
        }

        private void BuffPickup_OnTriggerStay(On.RoR2.BuffPickup.orig_OnTriggerStay orig, BuffPickup self, Collider other)
        {
            orig(self, other);
            CheckBarrier(other);
        }

        private void AmmoPickup_OnTriggerStay(On.RoR2.AmmoPickup.orig_OnTriggerStay orig, AmmoPickup self, Collider other)
        {
            orig(self, other);
            CheckBarrier(other);
        }

        private void CheckBarrier(Collider grabber)
        {
            CharacterBody body = grabber.GetComponent<CharacterBody>();
            if (body && body.healthComponent && body.inventory)
            {
                int grabCount = body.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    //Log.Debug("staying toy barrier...");
                    body.healthComponent.AddBarrier(barrierGained * grabCount);
                }
            }
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            if (self && self.inventory)
            {
                _ = self.AddItemBehavior<ToyRobotBehaviour>(self.inventory.GetItemCountEffective(item));
            }
            orig(self);
        }

        public class ToyRobotBehaviour : CharacterBody.ItemBehavior
        {
            //private SphereCollider sphereSearch;
            private SphereSearch sphereSearch;
            private List<Collider> colliders;
            //private float maxDistance = 0; measuring distance

            public void FixedUpdate()
            {
                if (sphereSearch == null || !body || body.transform.position == null) //Needs to be attatched to a body so we check if its null
                    return;

                //sphereSearch.center = body.transform.position;
                sphereSearch.origin = body.transform.position;
                sphereSearch.radius = stack * pickupRange;

                //GravitationControllers have sphere colliders to check whenever a player is in radius no matter what...
                colliders.Clear();
                sphereSearch.RefreshCandidates().OrderCandidatesByDistance().GetColliders(colliders);
                foreach (Collider pickUp in colliders)
                {
                    // Get Gravitate Pickup if it has one
                    GravitatePickup gravitatePickup = pickUp.gameObject.GetComponent<GravitatePickup>();
                    if (gravitatePickup && gravitatePickup.gravitateTarget == null && gravitatePickup.teamFilter.teamIndex == body.teamComponent.teamIndex)
                    {
                        // If it does not have a gravitation target, then pull in
                        // Chance to pickup, so that one player doesn't pickup all stuff
                        // Log.Warning("Toy Pickup for " + body.GetUserName() + "\t is " + (minPickupChance + (ratioPickupChance / stack)));
                        if (Util.CheckRoll(minPickupChance + ratioPickupChance / stack))
                        {
                            //Log.Debug("     Got");

                            /*/
                            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
                            {
                                origin = pickUp.transform.position,
                                rotation = Quaternion.identity,
                                scale = 0.5f,
                                color = new Color(0.2392f, 0.8196f, 0.917647f) // Cyan Lunar color
                            }, true);//*/
                            gravitatePickup.gravitateTarget = body.transform;
                        }



                        /*/ Failed Check closest player method
                        // As a Pickup
                        Log.Warning("Toy Robot on the job! for " + body.GetUserName());
                        Vector3 pickUpTransform = pickUp.transform.position;
                        // Detect other Sphere Colliders
                        float minDistance = float.MaxValue;
                        //float minDistance = Vector3.Distance(pickUp.transform.position, body.transform.position);

                        CharacterBody target = body;
                        Collider[] overlappingSpheres = Physics.OverlapSphere(pickUpTransform, 0.5f); // Get all spheres overlapping with this sphere

                        for (int i = 0; i < overlappingSpheres.Length; i++)
                        {
                            Log.Debug("Type: " + overlappingSpheres[i].GetType() + " is sphere? " + (overlappingSpheres[i].GetType() == typeof(SphereCollider)));
                            if (overlappingSpheres[i].GetType() == typeof(SphereCollider) && overlappingSpheres[i].GetComponent<CharacterBody>())
                            {
                                var playerBody = overlappingSpheres[i].GetComponent<CharacterBody>();
                                if (playerBody)
                                {
                                    Log.Debug("Found Player? " + playerBody.GetUserName());
                                    float distance = Vector3.Distance(pickUpTransform, overlappingSpheres[i].transform.position);
                                    if (distance < minDistance)
                                    {
                                        // Found a closer player, will fly towards them
                                        minDistance = distance;
                                        Log.Debug("Get Closer Character Body?");
                                        target = overlappingSpheres[i].GetComponent<CharacterBody>();
                                        Log.Debug("Target " + target.GetUserName());
                                    }
                                    Log.Debug("Position of sphere search " + i + " is " + overlappingSpheres[i].transform.position);
                                }
                            }
                        }
                        //*/

                        // Print distance of pickup
                        /*/ Measure distance
                        var measureDistance = Vector3.Distance(body.transform.position, pickUp.transform.position);
                        if (maxDistance < measureDistance)
                        {
                            maxDistance = measureDistance;
                            Log.Debug("Max distance: " + maxDistance);
                        }//*/

                        //gravitatePickup.gravitateTarget = body.transform;

                        //Body team = 1f
                        //Pickup team = Player
                        //gravitatePickup.maxSpeed = 40
                        //Gravitate tag: untagged
                        // 16 - 24 -
                    }
                }
            }

            public void Start()
            {
                //Log.Warning("Got my some sphere!");
                colliders = [];
                sphereSearch = new SphereSearch()
                {
                    mask = LayerIndex.pickups.mask,
                    queryTriggerInteraction = QueryTriggerInteraction.Collide
                    //We do not need to filter by team as a gravitate pickup OnTriggerEnter already does it
                };
            }


            public void OnDestroy()
            {
                sphereSearch = null;
                //Log.Warning("Sphere gone? " + sphereSearch.IsNullOrDestroyed());
            }
        }
    }
}