using BepInEx.Configuration;
using BepInEx.Logging;
using IL.RoR2.Items;
using On.RoR2.Items;
using Rewired.Utils;

//using Rewired.Utils;
using RoR2;
//using System.Collections;
using System.Collections.Generic;
//using System.ComponentModel;
using UnityEngine;
//using UnityEngine.Yoga;

namespace UltitemsCyan.Items.Tier1
{

    // TODO: check if Item classes needs to be public
    public class ToyRobot : ItemBase
    {
        public static ItemDef item;

        private const float pickupRange = 10f;
        private const float minPickupChance = 8f;
        private const float ratioPickupChance = 72f;

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
                "Pull in pickups from <style=cIsUtility>20m</style> <style=cStack>(+10m per stack)</style> away. Gain <style=cIsHealing>8</style> <style=cStack>(+8 per stack)</style> <style=cIsHealing>temporary barrier</style> from pickups.",
                "They march to you like a song carriers their steps. More robots have a weaker pull",
                ItemTier.Tier1,
                UltAssets.ToyRobotSprite,
                UltAssets.ToyRobotPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility, ItemTag.Healing]
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
            //On.RoR2.JunkPickup.OnTriggerStay += 
            //On.RoR2.
            // ToyRobot Behaviour
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        //https://github.com/TheMysticSword/MysticsItems/blob/main/Items/Tier2/ExplosivePickups.cs
        private void HealthPickup_OnTriggerStay(On.RoR2.HealthPickup.orig_OnTriggerStay orig, HealthPickup self, Collider other)
        {
            orig(self, other);
            Log.Logger(LogLevel.Debug, " ---- || ---- Toy Pickup for HealthPickup");
            CheckBarrier(other);
        }

        private void ElusiveAntlersPickup_OnTriggerStay(On.RoR2.ElusiveAntlersPickup.orig_OnTriggerStay orig, ElusiveAntlersPickup self, Collider other)
        {
            orig(self, other);
            Log.Logger(LogLevel.Debug, " ---- || ---- Toy Pickup for ElusiveAntlersPickup");
            CheckBarrier(other);
        }

        private void MoneyPickup_OnTriggerStay(On.RoR2.MoneyPickup.orig_OnTriggerStay orig, MoneyPickup self, Collider other)
        {
            orig(self, other);
            Log.Logger(LogLevel.Debug, " ---- || ---- Toy Pickup for MoneyPickup");
            CheckBarrier(other);
        }

        private void BuffPickup_OnTriggerStay(On.RoR2.BuffPickup.orig_OnTriggerStay orig, BuffPickup self, Collider other)
        {
            orig(self, other);
            Log.Logger(LogLevel.Debug, " ---- || ---- Toy Pickup for BuffPickup");
            CheckBarrier(other);
        }

        private void AmmoPickup_OnTriggerStay(On.RoR2.AmmoPickup.orig_OnTriggerStay orig, AmmoPickup self, Collider other)
        {
            orig(self, other);
            Log.Logger(LogLevel.Debug, " ---- || ---- Toy Pickup for AmmoPickup");
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
                    Log.Logger(LogLevel.Debug, "giving toy barrier...");
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

            //[ItemDefAssociation(useOnServer = true, useOnClient = false)]
            //public static ItemDef GetItemDef()
            //{
            //    Log.Logger(LogLevel.Debug, "ToyRobotBehaviour GetItemDef");
            //    return item;
            //}

            public void FixedUpdate()
            {
                if (sphereSearch == null || !body) //Needs to be attatched to a body so we check if its null
                {
                    Log.Logger(LogLevel.Debug, "Toy FixedUpdate Return");
                    return;
                }

                Log.Logger(LogLevel.Debug, "Toy 1");

                //sphereSearch.center = body.transform.position;
                sphereSearch.origin = body.transform.position;
                sphereSearch.radius = stack * pickupRange;

                //GravitationControllers have sphere colliders to check whenever a player is in radius no matter what...
                colliders.Clear();

                sphereSearch.RefreshCandidates().GetColliders(colliders);

                Log.Logger(LogLevel.Debug, "Toy 2");

                foreach (Collider pickUp in colliders)
                {
                    // Temp Don't keep Sphere Collider check
                    if (pickUp == null && pickUp.transform == null) //|| pickUp.gameObject == null
                    {
                        continue;
                    }

                    // Get Gravitate Pickup if it has one
                    GravitatePickup gravitatePickup = pickUp.transform.root.GetComponentInChildren<GravitatePickup>();

                    //Log.Logger(LogLevel.Debug, "Toy 3 | Component? " + (gravitatePickup == null));
                    // For some reason GetComponent() or something, gravitatePickup is always null
                    if (gravitatePickup != null)
                    {
                        Log.Logger(LogLevel.Debug, " | Target? " + (gravitatePickup.gravitateTarget == null));
                        Log.Logger(LogLevel.Debug, " | Team? " + (gravitatePickup.teamFilter.teamIndex == body.teamComponent.teamIndex));
                    }

                    if (gravitatePickup && gravitatePickup.gravitateTarget == null && gravitatePickup.teamFilter.teamIndex == body.teamComponent.teamIndex)
                    {
                        if (pickUp.gameObject != null)
                        {
                            Log.Logger(LogLevel.Debug, " ---- || ---- Toy Pickup: Found " + pickUp.gameObject.name);
                        }
                        if (Util.CheckRoll(minPickupChance + ratioPickupChance / stack))
                        {
                            Log.Logger(LogLevel.Debug, " ---- || ---- Toy Pickup: Gravitate");
                            gravitatePickup.gravitateTarget = body.transform;
                        }
                    }
                }
            }

            public void Awake()
            {
                Log.Logger(LogLevel.Warning, "Got my some sphere!");
#pragma warning disable IDE0028 // Simplify collection initialization
                colliders = new List<Collider>();
#pragma warning restore IDE0028 // Simplify collection initialization
                sphereSearch = new SphereSearch()
                {
                    mask = LayerIndex.pickups.mask,
                    queryTriggerInteraction = QueryTriggerInteraction.Collide
                    //We do not need to filter by team as a gravitate pickup OnTriggerEnter already does it
                };
                Log.Logger(LogLevel.Debug, "Sphere here? " + sphereSearch.IsNullOrDestroyed());
            }

            public void OnDestroy()
            {
                sphereSearch = null;
                Log.Logger(LogLevel.Warning, "Sphere gone? " + sphereSearch.IsNullOrDestroyed());
            }
        }

        //public class ToyRobotBehaviour : CharacterBody.ItemBehavior
        //{
        //    //private SphereCollider sphereSearch;
        //    private SphereSearch sphereSearch;
        //    private List<Collider> colliders;
        //    //private float maxDistance = 0; measuring distance

        //    public void FixedUpdate()
        //    {
        //        if (sphereSearch == null || !body || body.transform.position == null) //Needs to be attatched to a body so we check if its null
        //        {
        //            return;
        //        }

        //        Log.Logger(LogLevel.Debug, "Toy 1");

        //        //sphereSearch.center = body.transform.position;
        //        sphereSearch.origin = body.transform.position;
        //        sphereSearch.radius = stack * pickupRange;

        //        //GravitationControllers have sphere colliders to check whenever a player is in radius no matter what...
        //        colliders.Clear();
        //        sphereSearch.RefreshCandidates().OrderCandidatesByDistance().GetColliders(colliders);

        //        Log.Logger(LogLevel.Debug, "Toy 2");

        //        foreach (Collider pickUp in colliders)
        //        {
        //            // Get Gravitate Pickup if it has one
        //            GravitatePickup gravitatePickup = pickUp.gameObject.GetComponent<GravitatePickup>();

        //            Log.Logger(LogLevel.Debug, "Toy 3 | Component? " + gravitatePickup +
        //                        " | Target? " + (gravitatePickup.gravitateTarget == null) +
        //                        " | Team? " + (gravitatePickup.teamFilter.teamIndex == body.teamComponent.teamIndex));

        //            if (gravitatePickup && gravitatePickup.gravitateTarget == null && gravitatePickup.teamFilter.teamIndex == body.teamComponent.teamIndex)
        //            {
        //                Log.Logger(LogLevel.Debug, " ---- || ---- Toy Pickup: Found " + pickUp.gameObject.name);
        //                if (Util.CheckRoll(minPickupChance + ratioPickupChance / stack))
        //                {
        //                    gravitatePickup.gravitateTarget = body.transform;
        //                }
        //            }
        //        }
        //    }

        //    public void Start()
        //    {
        //        Log.Logger(LogLevel.Warning, "Got my some sphere!");
        //        colliders = [];
        //        sphereSearch = new SphereSearch()
        //        {
        //            mask = LayerIndex.pickups.mask,
        //            queryTriggerInteraction = QueryTriggerInteraction.Collide
        //            //We do not need to filter by team as a gravitate pickup OnTriggerEnter already does it
        //        };
        //        Log.Logger(LogLevel.Debug, "Sphere here? " + sphereSearch.IsNullOrDestroyed());
        //    }


        //    public void OnDestroy()
        //    {
        //        sphereSearch = null;
        //        Log.Logger(LogLevel.Warning, "Sphere gone? " + sphereSearch.IsNullOrDestroyed());
        //    }
        //}
    }
}