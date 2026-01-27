using RoR2;
using UltitemsCyan.Items.Untiered;
using System.Collections.Generic;
//using UnityEngine.Networking;
using UltitemsCyan.Items.Lunar;
using BepInEx.Configuration;
using RoR2.Orbs;
using UnityEngine;
using UnityEngine.Networking;

namespace UltitemsCyan.Equipment
{
    /* NOTES
     * 
     * Removing normal
     * 
     * When dissolving a boss item, the item will still be dropped by the boss for teleporter and tricorn
     *      removing from available items will only effect command
     * If you dissolve void items, then Larva won't corrupt thoes pairs upon dying
     * Prayer Beads do give stats upon being dissolved
     * 
     * 
     * What if intead of immeidantly deleting it, that stack of items becomes temporary, so they are put on a timer...
     * Or ite gives gray solute of the items then also gives a random temporary item in the same tier?
     * 
     * 
     */

    

    // dropPickup has no setter anymore; use currentPickup in general
    // generatedDrops -> generatedPickups
    // ShopTerminalBehavior
    // Various changes in the droptable classes: BasicPickUpDropTable, FreeChestDropTable, etc
    public class Obsolute : EquipmentBase
    {
        public static EquipmentDef equipment;

        private const float shortCooldown = 6f;
        private const float cooldown = 30f;

        // Ratio of total cost 'refunded'
        private const float goldRatio = 0.8f;
        // Base money gained when deleting items
        private const float smallChestCost = 25f; // White and Lunar
        private const float largeChestCost = 50f; // Green and Food
        private const float legendaryChestCost = 400f; // Red
        private const float voidCostMultiplier = 1.5f; // Void variant multiplier

        // Keeps track of the dissolved items of the current stage
        private readonly List<ItemIndex> dissolvedList = [];

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Obsolute";
            if (!CheckItemEnabledConfig(itemName, "Equipment", configs))
            {
                return;
            }
            equipment = CreateItemDef(
                "OBSOLUTE",
                itemName,
                "<style=cDeath>Erase</style> your last item from existence and gain some gold.",
                "<style=cDeath>Erase</style> the last item in your inventory from the run. It will no longer appear, and any instances of the items will <style=cDeath>dissolve</style>. Gain <style=cIsUtility>gold</style> corrisponding to items dissolved. Cannot dissolve scraped, key, or boss items.",
                "Everything returns to grey",
                cooldown,
                true,
                true,
                false,
                UltAssets.ObsoluteSprite,
                UltAssets.ObsolutePrefab
            );
        }

        protected override void Hooks()
        {
            // Clear dissolved so refresh between runs and stages
            // Only really need dissolved for grabing items that were already dropped but dissolved.
            // Dissolved items update by the next stage, so can clear list
            On.RoR2.Run.BeginStage += Run_BeginStage;
            // * * * Erase Items
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            // * * * Maintain removal
            // When getting a dissolved item
            //On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int += Inventory_GiveItemPermanent_ItemIndex_int; ;
            On.RoR2.Inventory.GiveItemTemp += Inventory_GiveItemTemp;
            // When a chest tries dropping a dissolved item
            On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
            On.RoR2.OptionChestBehavior.ItemDrop += OptionChestBehavior_ItemDrop;
            //On.RoR2.PickupDropTable.GenerateDrop += PickupDropTable_GenerateDrop;
        }

        //private PickupIndex PickupDropTable_GenerateDrop(On.RoR2.PickupDropTable.orig_GenerateDrop orig, PickupDropTable self, Xoroshiro128Plus rng)
        //{
        //    PickupIndex pickup = orig(self, rng);
        //    if (dissolvedList.Contains(PickupCatalog.GetPickupDef(pickup).itemIndex))
        //    {
        //        //Log.Debug("Pickup " + PickupCatalog.GetPickupDef(pickup).nameToken + " was dissolved...");
        //        return pickup;
        //    }
        //    else
        //    {
        //        return pickup;
        //    }
        //}

        private void Run_BeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
        {
            //Log.Debug("Universal Dissolved cleared");
            dissolvedList.Clear();
            orig(self);
        }

        // Delete Existing instances of the item, and remove from drops
        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active && !self.equipmentDisabled && equipmentDef == equipment)
            {
                /*/
                if (self.gameObject && self.gameObject.name.Contains("EquipmentDrone"))
                {
                    return false;
                }//*/

                CharacterBody activator = self.characterBody;
                List<ItemIndex> itemList = activator.inventory.itemAcquisitionOrder;

                if (itemList.Count > 0)
                {
                    // Null if player has only untiered items, or no items
                    ItemDef lastItem = GetLastItem(itemList);

                    if (lastItem)
                    {
                        Run thisRun = Run.instance;

                        if (thisRun.isRunStopwatchPaused)
                        {
                            //Log.Debug("In time paused");
                            equipment.cooldown = shortCooldown;
                        }
                        else
                        {
                            //Log.Debug("Outside time paused");
                            equipment.cooldown = cooldown;
                        }

                        Log.Debug("Last Item: " + lastItem.name);

                        // * * * For Every player and monster remove the item
                        foreach (CharacterMaster body in CharacterMaster.readOnlyInstancesList)
                        {
                            Log.Debug("who? " + body.name);
                            // Checks inventory in function
                            DissolveItem(body, lastItem);
                        }

                        int printTestItem = (int)DreamFuel.item.itemIndex;

                        // * * * Remove item from pools
                        thisRun.DisableItemDrop(lastItem.itemIndex);
                        //Run.instance.DisablePickupDrop(PickupCatalog.itemIndexToPickupIndex[(int)lastItem.itemIndex]);
                        _ = thisRun.availableItems.Remove(lastItem.itemIndex);
                        CheckEmptyTierList(lastItem); // also check if empty, if so then add solute to item tier

                        // PRINT TEST
                        Log.Debug(PickupCatalog.itemIndexToPickupIndex[printTestItem] + " is in? "
                            + thisRun.availableLunarCombinedDropList.Contains(PickupCatalog.itemIndexToPickupIndex[printTestItem])
                            + " and " + thisRun.availableLunarItemDropList.Contains(PickupCatalog.itemIndexToPickupIndex[printTestItem]));

                        thisRun.RefreshLunarCombinedDropList(); // Might need to be ran on next frame instead

                        // PRINT TEST
                        Log.Debug(PickupCatalog.itemIndexToPickupIndex[printTestItem] + " is in? "
                            + thisRun.availableLunarCombinedDropList.Contains(PickupCatalog.itemIndexToPickupIndex[printTestItem])
                            + " and " + thisRun.availableLunarItemDropList.Contains(PickupCatalog.itemIndexToPickupIndex[printTestItem]));

                        dissolvedList.Add(lastItem.itemIndex);
                        //Log.Debug(" &&& &&& Refresh Items ");

                        //thisRun.BuildDropTable();

                        //Log.Debug(" &&& &&& Refresh Items end ");


                        // Refresh chest and lunar pools
                        //Log.Warning("Refresing ALL ! ! !");
                        /*foreach (PickupDropTable dropTable in PickupDropTable.instancesList)
                        {
                            Log.Debug(" . " + dropTable.GetType().ToString() + " | " + dropTable.GetPickupCount());
                        }*/



                        //Run.instance.BuildDropTable();
                        //PickupDropTable.RegenerateAll(Run.instance);




                        //foreach (var test in PickupDropTable.instancesList)
                        //{

                        //}

                        //PickupDropTable.instancesList

                        //Log.Debug(dissolvedItems.Contains(lastItem.itemIndex) + " in dessolved items");
                        _ = Util.PlaySound("Play_minimushroom_spore_shoot", self.gameObject);

                        // * * * Remove items from shops and item pickups? Turn into solvent?

                        //Run.instance.shopPortalCount;
                        //Run.instance.

                        /*Interactor[] Interactors = Run.instance.GetComponents<Interactor>();
                        Log.Debug("Length: " + Interactors.Length);
                        foreach (Interactor interactor in Interactors)
                        {
                            Log.Debug("Interactor name: " + interactor.name);
                            if (interactor.GetComponent<ShopTerminalBehavior>())
                            {
                                Log.Warning(" !!!! !!!! !!!! has Shop");
                            }
                        }*/

                        //ShopTerminalBehavior.GenerateNewPickupServer(true);


                        //UpdatePickupDisplayAndAnimations()
                        //PickupIndex pickupIndex = PickupCatalog.FindPickupIndex(itemIndex);

                        //DisableItemDisplay(ItemIndex itemIndex)

                        // Used Obsolute
                        return true;
                    }
                }
                // Have no valid items
                return false;
            }
            else
            {
                // Not Obsolute
                return orig(self, equipmentDef);
            }
        }

        private void Inventory_GiveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_GiveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int countToAdd)
        {
            if (dissolvedList.Contains(itemIndex) && self)
            {
                Log.Debug("Grabbed a disolved item...");
                _ = Util.PlaySound("Play_minimushroom_spore_shoot", self.gameObject);
                itemIndex = GreySolvent.item.itemIndex;
            }
            orig(self, itemIndex, countToAdd);
        }

        private void Inventory_GiveItemTemp(On.RoR2.Inventory.orig_GiveItemTemp orig, Inventory self, ItemIndex itemIndex, float countToAdd)
        {
            if (dissolvedList.Contains(itemIndex) && self)
            {
                Log.Debug("Grabbed a disolved item...");
                _ = Util.PlaySound("Play_minimushroom_spore_shoot", self.gameObject);
                itemIndex = GreySolvent.item.itemIndex;
            }
            orig(self, itemIndex, countToAdd);
        }

        private void ChestBehavior_ItemDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            // If item in chest is in dissolved list then reroll untill it isn't
            // Using currentPickup instead of dropPickup
            if (dissolvedList.Count > 0 && dissolvedList.Contains(PickupCatalog.GetPickupDef(self.currentPickup.pickupIndex).itemIndex))
            {
                //Log.Debug(" // Chest Universal has a dissolved item: " + PickupCatalog.GetPickupDef(self.currentPickup.pickupIndex).nameToken);
                // When rerolled will use updated avaialbe items list
                self.Roll();
                //Log.Debug("Is still dissolved?.. " + PickupCatalog.GetPickupDef(self.dropPickup).nameToken + " | " + dissolvedList.Contains(PickupCatalog.GetPickupDef(self.dropPickup).itemIndex));
            }
            orig(self);
        }

        private void OptionChestBehavior_ItemDrop(On.RoR2.OptionChestBehavior.orig_ItemDrop orig, OptionChestBehavior self)
        {
            // If item in chest is in dissolved list then reroll untill it isn't
            if (dissolvedList.Count > 0)
            {
                // May be more efficent to just reroll if there are dissolved items
                self.Roll();

                /*/ If any of the items in the potential are dissolved
                foreach (PickupIndex pickup in self.generatedDrops)
                {
                    if (dissolvedList.Contains(PickupCatalog.GetPickupDef(pickup).itemIndex))
                    {
                        // Contains a dissolved item
                        self.Roll();
                        break;
                    }
                }
                //*/
                // When rerolled will use updated avaialbe items list

                //Log.Debug("Is still dissolved?.. " + PickupCatalog.GetPickupDef(self.dropPickup).nameToken + " | " + dissolvedList.Contains(PickupCatalog.GetPickupDef(self.dropPickup).itemIndex));
            }
            orig(self);
        }

        /*/
        private void ChestBehavior_Roll(On.RoR2.ChestBehavior.orig_Roll orig, ChestBehavior self)
        {
            if (self)
            {
                Log.Debug("Rolling chest's Universe for " + self.name);
            }
            else
            {
                Log.Warning("Chest Behavior can be null ?!?!?!");
            }

            orig(self);
        }
        //*/

        private ItemDef GetLastItem(List<ItemIndex> list)
        {
            // Go through inventory in reverse order
            for (int i = list.Count - 1; i >= 0; i--)
            {
                ItemDef item = ItemCatalog.GetItemDef(list[i]);
                if (item.tier is not ItemTier.NoTier and not ItemTier.Boss and not ItemTier.VoidBoss)
                {
                    // Don't dissolve world unique items
                    List<ItemTag> tagList = [.. item.tags];
                    if (!tagList.Contains(ItemTag.WorldUnique))
                    {
                        // return last non untiered non unique item
                        return item;
                    }
                }
            }
            // Found nothing
            return null;
        }

        private void DissolveItem(CharacterMaster body, ItemDef item)
        {
            if (!body)
            {
                return;
            }
            Inventory inventory = body.inventory;
            if (inventory)
            {
                // TODO check if channeled items can be removed
                int grabCount = inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    Inventory.ItemTransformation.TryTransformResult tryTransformResult;
                    if (new Inventory.ItemTransformation
                    {
                        originalItemIndex = item.itemIndex,
                        newItemIndex = GreySolvent.item.itemIndex,
                        maxToTransform = 2147483647,
                        transformationType = 0
                    }.TryTransform(inventory, out tryTransformResult))
                    {
                        // If item succesfully transformed
                        if (body.GetBody())
                        {
                            RefundGoldForItem(body.GetBody(), tryTransformResult.totalTransformed, item.tier);
                        }
                    }
                }
            }
        }

        private void RefundGoldForItem(CharacterBody characterBody, int grabCount, ItemTier tier)
        {
            float chestCost = 1f;
            switch (tier)
            {
                // Multiply cost if a void item
                case ItemTier.VoidTier1:
                    chestCost *= voidCostMultiplier;
                    goto case ItemTier.Tier1;
                case ItemTier.VoidTier2:
                    chestCost *= voidCostMultiplier;
                    goto case ItemTier.Tier2;
                case ItemTier.VoidTier3:
                    chestCost *= voidCostMultiplier;
                    goto case ItemTier.Tier3;
                // Base chest cost of items
                case ItemTier.Tier1:
                case ItemTier.Lunar:
                    chestCost *= smallChestCost;
                    break;
                case ItemTier.Tier2:
                case ItemTier.FoodTier:
                    chestCost *= largeChestCost;
                    break;
                case ItemTier.Tier3:
                    chestCost *= legendaryChestCost;
                    break;
                case ItemTier.Boss:
                case ItemTier.NoTier:
                case ItemTier.VoidBoss:
                case ItemTier.AssignedAtRuntime:
                default:
                    break;
            }

            GoldOrb goldOrb = new()
            {
                origin = characterBody.corePosition,
                target = characterBody.mainHurtBox,
                goldAmount = (uint)((float)(grabCount * chestCost * goldRatio) * Run.instance.difficultyCoefficient)
            };
            //Log.Warning(" $.$ $.$ Gold Earned! chestCost: " + chestCost + " totalRefund: " + (uint)((float)(grabCount * chestCost * goldRatio) * Run.instance.difficultyCoefficient));
            OrbManager.instance.AddOrb(goldOrb);
            EffectManager.SimpleImpactEffect(HealthComponent.AssetReferences.gainCoinsImpactEffectPrefab, characterBody.corePosition, Vector3.up, true);
        }

        // Will properly replace tier across run unless Run.BuildDropTable() is called
        private void CheckEmptyTierList(ItemDef item)
        {
            List<PickupIndex> list = null;
            switch (item.tier)
            {
                case ItemTier.Tier1:
                    list = Run.instance.availableTier1DropList;
                    break;
                case ItemTier.Tier2:
                    list = Run.instance.availableTier2DropList;
                    break;
                case ItemTier.Tier3:
                    list = Run.instance.availableTier3DropList;
                    break;
                case ItemTier.Lunar:
                    list = Run.instance.availableLunarItemDropList;
                    break;
                case ItemTier.VoidTier1:
                    list = Run.instance.availableVoidTier1DropList;
                    break;
                case ItemTier.VoidTier2:
                    list = Run.instance.availableVoidTier2DropList;
                    break;
                case ItemTier.VoidTier3:
                    list = Run.instance.availableVoidTier3DropList;
                    break;
                case ItemTier.FoodTier:
                    list = Run.instance.availableFoodTierDropList;
                    break;
                case ItemTier.Boss:
                case ItemTier.VoidBoss:
                case ItemTier.NoTier:
                    Log.Warning(" !!! " + item.name + " Shouldn't have been removed by Obsolute because of tier");
                    break;
                case ItemTier.AssignedAtRuntime:
                    Log.Warning(" !!! " + item.name + " Oh! Didn't expect AssignedAtRuntime tier item after runtime?");
                    break;
                default:
                    Log.Warning(" !!! " + item.name + " Huh? Not a handeled tier case for Obsolute");
                    break;
            }
            if (list.Count == 0)
            {
                Log.Debug(item.tier.ToString() + " | replace with Solute");
                list.Add(PickupCatalog.itemIndexToPickupIndex[(int)GreySolvent.item.itemIndex]);
                Log.Warning("Solute is " + (list.Contains(PickupCatalog.itemIndexToPickupIndex[(int)GreySolvent.item.itemIndex]) ? "in" : "NOT IN") + " list now...");
            }
        }
    }
}