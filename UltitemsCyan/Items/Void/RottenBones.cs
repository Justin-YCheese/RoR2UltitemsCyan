using RoR2;
using UltitemsCyan.Buffs;
using UltitemsCyan.Items.Tier2;
using UnityEngine.Networking;
using BepInEx.Configuration;

namespace UltitemsCyan.Items.Void
{

    // TODO: check if Item classes needs to be public
    public class RottenBones : ItemBase
    {
        public static ItemDef item;
        public static ItemDef transformItem;

        public const int rotsPerItem = 3;
        public const float rottingBuffMultiplier = 15;
        public const float rotTimeInterval = 180; // 3 minutes

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Rotten Bones";
            if (!CheckItemEnabledConfig(itemName, "Void", configs))
            {
                return;
            }
            item = CreateItemDef(
                "ROTTENBONES",
                itemName,
                "Deal more damage over time. <style=cIsVoid>Corrupts all Birthday Candles</style>.",
                "Increase damage by <style=cIsDamage>15%</style> <style=cStack>(+15% per stack)</style> damage for every 3 minutes</style> passed in a stage, up to a max of <style=cIsDamage>3</style> stacks. <style=cIsVoid>Corrupts all Birthday Candles</style>.",
                "The bitter aftertaste is just the spoilage",
                ItemTier.VoidTier2,
                UltAssets.RottenBonesSprite,
                UltAssets.RottenBonesPrefab,
                [ItemTag.Utility, ItemTag.OnStageBeginEffect],
                BirthdayCandles.item
            );
        }

        protected override void Hooks()
        {
            // TODO Add 
            CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            //On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
        }

        private void CharacterBody_onBodyStartGlobal(CharacterBody self)
        {
            // Add Behavior to player (expectially if the full time intervals have passed)
            if (NetworkServer.active && self && self.inventory)
            {
                _ = self.AddItemBehavior<RottenBonesVoidBehavior>(self.inventory.GetItemCountEffective(item));
            }
        }
        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            // Add Behavior to player (expectially if the full time intervals have passed)
            if (self && self.inventory)
            {
                if (self.inventory.GetItemCountEffective(item) > 0)
                {
                    //Log.Warning("Give Rotting Bones");
                    // If within time intervals give item behavior
                    if (Run.instance.time < Ultitems.stageStartTime + rotTimeInterval * rotsPerItem)
                    {
                        RottenBonesVoidBehavior behavior = self.AddItemBehavior<RottenBonesVoidBehavior>(self.inventory.GetItemCountEffective(item));
                        //Log.Debug("New Bone? Intervals Passed! " + behavior.IntervalsPassed);
                        ApplyRot(self, behavior.IntervalsPassed);
                    }
                    else
                    {
                        // Apply max rot
                        //Log.Debug("Pass rot Time Interval Item Pickup");
                        ApplyRot(self, rotsPerItem);
                    }
                }
                else
                {
                    // If player doesn't have this item anymore
                    //Log.Debug("Inventory no bones");
                    //self.AddItemBehavior<RottenBonesVoidBehavior>(0); // When removed and added the same frame gets messed up
                    self.SetBuffCount(RottingBuff.buff.buffIndex, 0);
                }
            }
        }

        private static void ApplyRot(CharacterBody player, int intervals)
        {
            int grabCount = player.inventory.GetItemCountEffective(item);
            int maxRotStacks = grabCount * intervals;
            if (player.GetBuffCount(RottingBuff.buff) < maxRotStacks)
            {
                player.SetBuffCount(RottingBuff.buff.buffIndex, maxRotStacks);
            }
        }

        /*/ If the player picks up the item after max intervals have passed
        protected void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            orig(self, itemIndex, count);
            if (self && itemIndex == item.itemIndex)
            {
                Log.Warning("Give Rotting Bones");
                // Finds the player that picked up the item
                CharacterBody player = CharacterBody.readOnlyInstancesList.ToList().Find((body2) => body2.inventory == self);
                // If within time intervals give item behavior
                if (Run.instance.time < stageStartTime + (rotTimeInterval * rotsPerItem))
                {
                    RottenBonesVoidBehavior behavior = player.AddItemBehavior<RottenBonesVoidBehavior>(player.inventory.GetItemCount(item));
                    Log.Debug("New Bone? Intervals Passed! " + behavior.intervalsPassed);
                    ApplyRot(player, behavior.intervalsPassed);
                }
                else
                {
                    // Apply max rot
                    Log.Debug("Pass rot Time Interval Item Pickup");
                    ApplyRot(player, rotsPerItem);
                }
            }
        }//*/

        //
        public class RottenBonesVoidBehavior : CharacterBody.ItemBehavior
        {
            private int _intervalsPassed = 0;
            // Order:
            // Awake(), Enable(), Start()
            // Disable(), Destory()

            public int IntervalsPassed
            {
                get { return _intervalsPassed; }
                set
                {
                    //Log.Debug("_intervalsPassed: " + _intervalsPassed);
                    // If not already the same value
                    _intervalsPassed = value;
                    // If full health
                    if (_intervalsPassed > 0)
                    {
                        ApplyRot(body, _intervalsPassed);
                        if (_intervalsPassed >= rotsPerItem)
                        {
                            //Log.Debug("Pass rot Time Interval But Behavior!");
                            enabled = false; // Not sure if removes, but at least stop FixedUpdate for this item from running
                        }
                    }
                }
            }

#pragma warning disable IDE0051 // Remove unused private members
            private void FixedUpdate()
#pragma warning restore IDE0051 // Remove unused private members
            {
                float currentTime = Run.instance.time;
                // If more intervals have passed than currently recorded
                // not actually a while loop, the increment is IntervalsPassed
                while (currentTime > Ultitems.stageStartTime + rotTimeInterval * (_intervalsPassed + 1) && _intervalsPassed < rotsPerItem)
                {
                    //Log.Warning("Rot Math: " + (currentTime - stageStartTime) + "\t/ " + rotTimeInterval + "\t = " + (int)((currentTime - stageStartTime) / rotTimeInterval));
                    IntervalsPassed++;
                }
            }
        }
    }
}