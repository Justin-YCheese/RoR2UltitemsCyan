using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using System.Collections.Generic;
using UnityEngine.Networking;
using MonoMod.Cil;

//using Mono.Cecil.Cil;
using Mono.Cecil.Cil;
using System;

namespace UltitemsCyan.Items.Tier2
{
    /* Notes
     * 
     * Works with Focused Convergence,
     * 
     * 
     *  Test // Void fields cells // NullWardBaseState.wardRadiusOff = 0.2f + (0.2f * FindTotalMultiplier());
     *  Test Gold Fields
     *  Test Mythrixs Pillars
     *  
     */

    public class TinyIgloo : ItemBase
    {

        public static ItemDef item;

        //private readonly List<InitialZone> zoneList = [];
        public static readonly List<NetworkBehaviour> zoneList = [];

        // Healing
        private const int basePercent = 30;
        private const int perStackPercent = 10;
        private const float extraZoneMul = .5f; // How much additional zones increase healing scaled off base

        // Counting Zones
        private const int maxZoneCount = 15;
        private const float radiusPerOverheal = 50f; // How much percentage of radius increase from 100 percentage of overheal

        // Max Zone Radius
        private const float baseMaxRadius = 60f; // +60% radius size
        private const float perStackMaxRadius = 30f;

        //private float storeToFullHealth = 0f; // To store health value between functions

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Tiny Igloo";
            if (!CheckItemEnabledConfig(itemName, "Green", configs))
            {
                return;
            }
            item = CreateItemDef(
                "TINYIGLOO",
                itemName,
                "Increase healing per zones occupied. Healing increases zone size.",
                "While in a zone, <style=cIsHealing>heal 30%</style> <style=cStack>(+10% per stack)</style> more plus half as much for each <style=cIsDamage>additional zone</style> occupied. Healing will <style=cIsDamage>increases the size</style> of the zone for <style=cIsHealing>50%</style> of the amount <style=cIsHealing>healed</style>. Increase max size by <style=cIsDamage>60%</style> <style=cStack>(+30% per stack)</style>.",
                "It's like a snowball effect but for zones. Get it? But there already existed a snow globe item, so I went for something similar",
                ItemTier.Tier2,
                UltAssets.TinyIglooSprite,
                UltAssets.TinyIglooPrefab,
                [ItemTag.Healing, ItemTag.Utility, ItemTag.AIBlacklist, ItemTag.HoldoutZoneRelated]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.BuffWard.Start += BuffWard_Start;
            On.RoR2.HoldoutZoneController.Start += HoldoutZoneController_Start;
            //On.RoR2.HalcyoniteShrineInteractable.Start += HalcyoniteShrineInteractable_Start;
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            IL.RoR2.HealthComponent.HandleHeal += HealthComponent_HandleHeal;
        }


        // Add buff wards to global list
        private void BuffWard_Start(On.RoR2.BuffWard.orig_Start orig, BuffWard self)
        {
            orig(self);
            self.gameObject.AddComponent<IglooBuffWardController>().SetWard(self);
        }

        // Add zone to global list
        private void HoldoutZoneController_Start(On.RoR2.HoldoutZoneController.orig_Start orig, HoldoutZoneController self)
        {
            Log.Debug(" <><><><> Tiny Igloo Start Zone");
            if (self.applyFocusConvergence) //If zone can get shrunk, it can grow too
            {
                _ = self.gameObject.AddComponent<IglooHoldoutZoneController>();
            }
            else
            {
                Log.Warning(" >      < No focus convergence?");
            }
            orig(self);
        }

        /*// Add gold zones to global list
        private void HalcyoniteShrineInteractable_Start(On.RoR2.HalcyoniteShrineInteractable.orig_Start orig, HalcyoniteShrineInteractable self)
        {
            Log.Debug(" <><><><> Tiny Igloo Start HalcyoniteShrine");
            self.gameObject.AddComponent<IglooHalcyoniteShrineController>().SetShrine(self);
            orig(self);
        }*/

        // Increase amount healed
        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            if (nonRegen)
            {
                amount = ZoneHealAmountMultiplier(self, amount);
            }
            return orig(self, amount, procChainMask, nonRegen);
        }

        private float ZoneHealAmountMultiplier(HealthComponent self, float amount)
        {
            if (self && self.body && self.body.inventory)
            {
                int grabCount = self.body.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    int zoneCount = GetOccupiedZoneList(self.body, true).Length;

                    float zoneMultiplier = zoneCount == 0 ? 0 : (zoneCount + 1) * extraZoneMul;
                    float itemMultiplier = (basePercent + (grabCount - 1) * perStackPercent) / 100f;

                    float multiplier = 1 + itemMultiplier * zoneMultiplier;

                    return amount * multiplier;
                }
            }
            return amount;
        }

        private void HealthComponent_HandleHeal(ILContext il)
        {
            ILCursor c = new(il); // Make new ILContext

            // Inject code just after loading healMessage
            // just reading values, should break anything else

            // After HealthComponent.HealMessage healMessage = netMsg.ReadMessage<HealthComponent.HealMessage>();
            if (c.TryGotoNext(MoveType.After,
                x => x.Match(OpCodes.Callvirt),     // 1   0001    callvirt instance !!0['com.unity.multiplayer-hlapi.Runtime']UnityEngine.Networking.NetworkMessage::ReadMessage <class RoR2.HealthComponent/HealMessage>()
                x => x.MatchStloc(0)                // 2	0006	stloc.0
            ))
            {

                Log.Debug(" * * * Start C Index: " + c.Index + " > " + c.ToString());

                c.Index++;

                Log.Debug(" * * *       C Index: " + c.Index + " > " + c.ToString());

                _ = c.Emit(OpCodes.Ldloc, 0);     // Load HealthComponent.HealMessage
                // Run custom code
                _ = c.EmitDelegate<Action<HealthComponent.HealMessage>>((hm) =>
                {
                    GameObject tO = hm.target;
                    if (tO)
                    {
                        CharacterBody cB = tO.GetComponent<CharacterBody>();
                        int gC = cB.inventory.GetItemCountEffective(item);
                        if (gC > 0)
                        {
                            HealthComponent hC = tO.GetComponent<HealthComponent>();

                            //Log.Debug(" +++ Send Heal +++ | Healed = " + hm.amount);
                            //Log.Warning(" +++ Send Heal +++ | Heal Percent: " + hm.amount / hC.fullHealth * 100 + " include Wards?: " + NetworkServer.active);

                            // Increase radius for over heals
                            // Only include ward zones if network is active
                            IncreaseEachRadius(GetOccupiedZoneList(cB, NetworkServer.active), hm.amount / hC.fullHealth, gC);
                        }
                    }
                });

                //_ = c.Emit(OpCodes.Stloc, num14); // Store Total Damage
                //
                //}
                //else
                //{
                //    Log.Warning("Koala cannot find 'for (int k = 0; k < num15; k++){}'");
                //}
            }
            else
            {
                Log.Warning("TinyIgloo cannot find 'HealthComponent.HealMessage healMessage = netMsg.ReadMessage<HealthComponent.HealMessage>();'");
            }
        }

        // Get list of zones the body is in
        private NetworkBehaviour[] GetOccupiedZoneList(CharacterBody body, bool includeWards)
        {
            List<NetworkBehaviour> occupiedZones = new(maxZoneCount);
            foreach (NetworkBehaviour initZone in zoneList)
            {
                // Randomize list??? (otherwise prioritize oldest zone over newer zones)
                // If already counted max number of zones
                // If body is in buffward
                if (includeWards && initZone is BuffWard ward
                    && (body.transform.position - ward.transform.position).magnitude <= Mathf.Abs(ward.calculatedRadius))
                {
                    occupiedZones.Add(initZone);
                }
                // If body is in holdout zone
                else if (initZone is HoldoutZoneController holdout
                    && holdout.IsBodyInChargingRadius(body))
                {
                    occupiedZones.Add(initZone);
                }
                /*// If body is in Halcyonite Shrine zone
                else if (initZone is HalcyoniteShrineInteractable halshrine
                    && (body.transform.position - halshrine.transform.position).magnitude <= Mathf.Abs(halshrine.radius))
                {
                    occupiedZones.Add(initZone);
                }*/
                // break if already found max amount
                if (occupiedZones.Count >= maxZoneCount)
                {
                    break;
                }
            }
            //Log.Debug(" ^ ^ ^ list.Count = " + occupiedZones.Count);
            return [.. occupiedZones];
        }

        private void IncreaseEachRadius(NetworkBehaviour[] inZoneList, float overhealPer, int grabCount)
        {
            if (inZoneList.Length <= 0)
            {
                return;
            }
            //Log.Warning(" +++++ +++++ +++++ Increasing radius");
            overhealPer *= radiusPerOverheal;
            // Try to increase size of each zone the player is in
            foreach (NetworkBehaviour initZone in inZoneList)
            {
                // If body is in buff ward
                if (initZone is BuffWard ward)
                {
                    //Log.Debug(" . in Ward");

                    ward.GetComponent<IglooBuffWardController>().IncreaseSize(overhealPer, grabCount);
                    /*float radiusIncrease = RadiusChange(initZone, ward.radius, overhealPer, grabCount);
                    //ward.radius += radiusIncrease;
                    ward.Networkradius += radiusIncrease;
                    Log.Debug(" _ _ _ _ N E W Ward radius " + ward.radius);*/
                }
                // If body is in holdout zone
                else if (initZone is HoldoutZoneController holdout)
                {
                    //Log.Debug(" . in Holdout");

                    holdout.GetComponent<IglooHoldoutZoneController>().IncreaseSize(overhealPer, grabCount);

                    /*holdout.baseRadius += RadiusChange(initZone, holdout.baseRadius, overhealPer, grabCount);
                    Log.Debug(" _ _ _ _ N E W Ward radius " + holdout.baseRadius + " | current " + holdout.currentRadius + " | velocity " + holdout.radiusVelocity);*/
                }
                /*// If body is in buff ward
                else if (initZone is HalcyoniteShrineInteractable halshrine)
                {
                    Log.Debug(" . in Shrine");

                    halshrine.GetComponent<IglooHalcyoniteShrineController>().IncreaseSize(overhealPer, grabCount);
                    *//*float radiusIncrease = RadiusChange(initZone, ward.radius, overhealPer, grabCount);
                    //ward.radius += radiusIncrease;
                    ward.Networkradius += radiusIncrease;
                    Log.Debug(" _ _ _ _ N E W Ward radius " + ward.radius);*//*
                }*/
            }
        }

        public class IglooBuffWardController : MonoBehaviour
        {
            private BuffWard ward = null!;
            private float currentMultiplier = 100f;

            private void Awake()
            {
                currentMultiplier = 100f;
            }

            // Called on Startup
            public void SetWard(BuffWard aWard)
            {
                Log.Debug(" ()() Add Buffward");
                ward = aWard;
                zoneList.Add(ward);
            }

            private void OnDisable()
            {
                Log.Debug(" ()() Remove Buffward");
                _ = zoneList.Remove(ward);
            }

            public void IncreaseSize(float overhealPer, int grabCount)
            {
                float maxRadiusPercent = 100 + (baseMaxRadius + (grabCount - 1) * perStackMaxRadius);
                if (currentMultiplier < maxRadiusPercent)
                {
                    // Get Radius, divide, then multiply
                    float setRadius = ward.Networkradius;
                    //Log.Debug(" _ _ + + WARD setRadius old: " + setRadius + " | currentPercent: " + currentPercent);
                    setRadius /= currentMultiplier / 100f;
                    currentMultiplier = Mathf.Min(currentMultiplier + overhealPer, maxRadiusPercent);
                    setRadius *= currentMultiplier / 100f;
                    ward.Networkradius = Mathf.Max(ward.Networkradius, setRadius);
                    //Log.Debug(" _ _ + + WARD setRadius new = " + ward.Networkradius + " | currentPercent: " + currentPercent + " | maxPercent: " + maxRadiusPercent);
                }
            }
        }


        public class IglooHoldoutZoneController : MonoBehaviour
        {
            private HoldoutZoneController _holdoutZoneController = null!;
            private float currentMultiplier = 100f;

            private Run.FixedTimeStamp _enabledTime;
            private readonly static Color _materialColor = new(0.7f, 0.7f, 1f, 0.8f);
            private readonly static float currentMultMaxColor = 300f; // Max color when current mult 100 + 300 = 400%
            private readonly static float colorMix = .7f; // the ratio of base color and tiny Igloo color (1 = full igloo color)
            private readonly static float startupDelay = 3f; // delay before size change

            private void Awake()
            {
                currentMultiplier = 100f;
                _holdoutZoneController = GetComponent<HoldoutZoneController>();
                //_materialColor = _holdoutZoneController.baseIndicatorColor;
            }

            private void OnEnable()
            {
                Log.Debug(" ()() Add Holdout Zone");
                _enabledTime = Run.FixedTimeStamp.now;
                zoneList.Add(_holdoutZoneController);
                _holdoutZoneController.calcRadius += ApplyRadius;
                _holdoutZoneController.calcColor += ApplyColor;
            }

            private void OnDisable()
            {
                var print = zoneList.Remove(_holdoutZoneController);
                Log.Warning(" ==== ++++ Remove Igloo from List (Never Called?)" + print);
                _holdoutZoneController.calcRadius -= ApplyRadius;
                _holdoutZoneController.calcColor -= ApplyColor;
            }

            public void IncreaseSize(float overhealPer, int grabCount)
            {
                float maxRadiusPercent = 100 + (baseMaxRadius + (grabCount - 1) * perStackMaxRadius);
                if (currentMultiplier < maxRadiusPercent)
                {
                    //Log.Debug(" _ _ + + ZONE currentMultiplier old: " + currentMultiplier);
                    currentMultiplier = Mathf.Min(currentMultiplier + overhealPer, maxRadiusPercent);
                    //Log.Debug(" _ _ + + ZONE currentMultiplier new: " + currentMultiplier + " | maxPercent: " + maxRadiusPercent);
                }
            }

            private void ApplyRadius(ref float radius)
            {
                if (_enabledTime.timeSince > startupDelay)
                {
                    radius *= currentMultiplier / 100;
                }
            }

            private void ApplyColor(ref Color color)
            {
                color = Color.Lerp(color, _materialColor, Mathf.Min((currentMultiplier - 100f) / currentMultMaxColor, 1f) * colorMix);
            }
        }

        // Mechanicly increases size but dosn't visually
        /*public class IglooHalcyoniteShrineController : MonoBehaviour
        {
            private HalcyoniteShrineInteractable halshrine = null!;
            private float currentMultiplier = 100f;

            private void Awake()
            {
                currentMultiplier = 100f;
            }

            // Called on Startup
            public void SetShrine(HalcyoniteShrineInteractable aShrine)
            {
                Log.Debug(" ()() Add HalcyoniteShrine");
                halshrine = aShrine;
                zoneList.Add(halshrine);
            }

            private void OnDisable()
            {
                Log.Debug(" ()() Remove HalcyoniteShrine");
                _ = zoneList.Remove(halshrine);
            }

            public void IncreaseSize(float overhealPer, int grabCount)
            {
                float maxRadiusPercent = 100 + (baseMaxRadius + (grabCount - 1) * perStackMaxRadius);
                if (currentMultiplier < maxRadiusPercent)
                {
                    // Get Radius, divide, then multiply
                    float setRadius = halshrine.radius;
                    //float setScale = halshrine.shrineHalcyoniteBubble.transform;
                    
                    Log.Warning(" _ _ + + SHRINE setRadius old: " + setRadius + " mag: ?? | currentPercent: " + currentMultiplier);
                    Log.Warning(" _ _ + + SHRINE lossy: " + halshrine.shrineHalcyoniteBubble.transform.lossyScale);
                    
                    setRadius /= currentMultiplier / 100f;
                    //setScale /= currentMultiplier / 100f;
                    currentMultiplier = Mathf.Min(currentMultiplier + overhealPer, maxRadiusPercent);
                    setRadius *= currentMultiplier / 100f;
                    //setScale *= currentMultiplier / 100f;
                    halshrine.radius = Mathf.Max(halshrine.radius, setRadius);
                    //halshrine.shrineHalcyoniteBubble.transform.localScale *= Mathf.Max(halshrine.shrineHalcyoniteBubble.transform.localScale.magnitude, setScale);
                    //Log.Warning(" _ _ + + SHRINE setRadius new = " + halshrine.radius + " mag: " + halshrine.shrineHalcyoniteBubble.transform.localScale + " | currentPercent: " + currentMultiplier + " | maxPercent: " + maxRadiusPercent);
                    Log.Warning(" _ _ + + SHRINE setRadius new = " + halshrine.radius + " mag: ?? | currentPercent: " + currentMultiplier + " | maxPercent: " + maxRadiusPercent);

                }
            }
        }*/
    }
}