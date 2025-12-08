using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections.Generic;
using BepInEx.Configuration;

// Notes:
// Doesn't take Consumed Regenerative Scrap nor Sale Star

namespace UltitemsCyan.Items.Tier2
{

    // TODO: check if Item classes needs to be public
    public class DegreeScissors : ItemBase
    {
        public static ItemDef item;
        private const int consumedPerScissor = 2;
        private const int scrapsPerConsumed = 2;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "1000 Degree Scissors";
            if (!CheckItemEnabledConfig(itemName, "Green", configs))
            {
                return;
            }
            item = CreateItemDef(
                "DEGREESCISSORS",
                itemName,
                "Melts two consumed items into scraps. Otherwise melts itself.",
                "At the start of each stage, <style=cIsUtility>melts</style> two <style=cIsUtility>consumed</style> items into <style=cIsUtility>2 common scraps</style> each. If no scissor is used, then it <style=cIsUtility>melts</style> itself.",
                "What's Youtube?",
                ItemTier.Tier2,
                UltAssets.DegreeScissorsSprite,
                UltAssets.DegreeScissorsPrefab,
                [ItemTag.Utility, ItemTag.OnStageBeginEffect, ItemTag.AIBlacklist]
            );
        }


        protected override void Hooks()
        {
            //CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
            //On.RoR2.Run.BeginStage += Run_BeginStage;
            On.RoR2.Stage.BeginServer += Stage_BeginServer;
        }

        private void Stage_BeginServer(On.RoR2.Stage.orig_BeginServer orig, Stage self)
        {
            //Log.Debug(" / / / Into Server Begin");
            orig(self);
            //Log.Debug(" / / / Out the Server Begin");
            if (!NetworkServer.active)
            {
                //Log.Debug("Running on Client... return...");
                return;
            }
            foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList)
            {
                if (master.inventory)
                {
                    int grabCount = master.inventory.GetItemCountEffective(item.itemIndex) * consumedPerScissor; // 2 consumed items per Scissor
                    if (grabCount > 0)
                    {
                        //Log.Warning("Scissors on body start global..." + master.name);
                        // Get inventory
                        List<ItemIndex> itemsInInventory = master.inventory.itemAcquisitionOrder;

                        // Aggregate list of consumed items in inventory
                        List<ItemDef> consumedItems = GetUntieredItems(itemsInInventory);

                        // Convert items
                        MeltItems(master, consumedItems, grabCount);
                    }
                }
            }
        }

#pragma warning disable IDE0051 // Remove unused private members
        private void Run_BeginStage(On.RoR2.Run.orig_BeginStage orig, Run self)
#pragma warning restore IDE0051 // Remove unused private members
        {
            //Log.Debug(" . . . In Beginin Stage");
            orig(self);
            //Log.Debug(" . . . Outing the Begin Stage");
        }

        private List<ItemDef> GetUntieredItems(List<ItemIndex> list)
        {
            List<ItemDef> consumedItems = [];
            foreach (ItemIndex index in list)
            {
                ItemDef definition = ItemCatalog.GetItemDef(index);
                // If item is untiered, can be removed, and not hidden
                // Don't need to check for regenerating scrap because it is restored before this check
                if (definition.tier.Equals(ItemTier.NoTier) && !definition.hidden) //definition.name.ToUpper().Contains("CONSUMED") // Checking for "consumed"
                {
                    //Log.Debug("Adding consumed item " + definition.name);
                    consumedItems.Add(definition);
                }
            }
            return consumedItems;
        }

        private void MeltItems(CharacterMaster master, List<ItemDef> consumedItems, int grabCount)
        {
            int length = consumedItems.Count;
            if (length > 0)
            {
                int scrapsCounted = 0;

                while (grabCount > 0)
                {
                    grabCount--; // Reduce usage first incase of break
                    int itemPos = Random.Range(0, length);
                    ItemDef selectedItem = consumedItems[itemPos]; // Don't need to subtract 1 from length because random excludes the max
                                                                   // Remove 1 consumed item
                                                                   //Log.Debug("Removing " + selectedItem.name); // + " at " + itemPos);
                    master.inventory.RemoveItemPermanent(selectedItem);

                    // Give 2 white scraps
                    //self.inventory.GiveItem(ItemCatalog.FindItemIndex("ScrapWhite"), scrapsPerConsumed);
                    scrapsCounted += scrapsPerConsumed;
                    

                    CharacterMasterNotificationQueue.SendTransformNotification(
                        master,
                        selectedItem.itemIndex,
                        RoR2Content.Items.ScrapWhite.itemIndex,
                        CharacterMasterNotificationQueue.TransformationType.Default);

                    // If ran out of that consumable item in player's inventory

                    // TODO check if temporary items will mess with this count for scissors
                    if (master.inventory.GetItemCountPermanent(selectedItem) <= 0)
                    {
                        //Log.Debug("Out of " + selectedItem.name);
                        //Log.Debug("New length of " + (length - 1));
                        consumedItems.RemoveAt(itemPos);
                        length--;
                        // If list is empty break loop
                        if (length == 0)
                        {
                            //Log.Debug("Scissors can't cut Empty List");
                            break;
                        }
                    }
                }
                master.inventory.GiveItemPermanent(ItemCatalog.FindItemIndex("ScrapWhite"), scrapsCounted);
            }
            else
            {
                // Player doesn't have any consumed items
                //Log.Warning(master.name + " has no consumed items: Scissors cuts itself");
                // Remove a scissors (Garenteed to have at least scissors)
                
                Log.Debug("Out of garbage for scissors");
                if (master.inventory.GetItemCountPermanent(item.itemIndex) > 0)
                {
                    Log.Debug("But can scrap a permananet Scissor!");
                    master.inventory.RemoveItemPermanent(item);
                    // Give 2 white scraps
                    master.inventory.GiveItemPermanent(ItemCatalog.FindItemIndex("ScrapWhite"), scrapsPerConsumed);
                }
            }
            // Really doesn't need sound, can't hear anyways
            //Util.PlaySound("Play_merc_sword_impact", self.gameObject);
        }










        /*/
        protected void CharacterBody_onBodyStartGlobal(CharacterBody self)
        {
            if (NetworkServer.active && self && self.inventory)
            {
                // Get number of scissors
                int grabCount = self.inventory.GetItemCount(item.itemIndex) * consumedPerScissor; // 2 consumed items per Scissor
                if (grabCount > 0)
                {
                    Log.Warning("Scissors on body start global..." + self.GetUserName());
                    // Get inventory
                    System.Collections.Generic.List<ItemIndex> itemsInInventory = self.inventory.itemAcquisitionOrder;

                    // Print items in inventory
                    Log.Debug("Items in inventory: " + itemsInInventory.ToString());
                    foreach (ItemIndex item in itemsInInventory)
                    {
                        Log.Debug(" - " + ItemCatalog.GetItemDef(item).name);
                    }///

                    // Aggregate list of consumed items in inventory
                    System.Collections.Generic.List<ItemDef> consumedItems = [];
                    foreach (ItemIndex index in itemsInInventory)
                    {
                        ItemDef definition = ItemCatalog.GetItemDef(index);
                        // If item is untiered, can be removed, and not hidden
                        // Don't need to check for regenerating scrap because it is restored before this check
                        if (definition.tier.Equals(ItemTier.NoTier) && !definition.hidden) //definition.name.ToUpper().Contains("CONSUMED") // Checking for "consumed"
                        {
                            //Log.Debug("Adding consumed item " + definition.name);
                            consumedItems.Add(definition);
                        }
                    }

                    // Print consumeable items
                    Log.Debug("List of consumeable items:");
                    foreach (ItemDef item in consumedItems)
                    {
                        Log.Debug(" - " + item.name + " (" + self.inventory.GetItemCount(item) + ")");
                    
                    }///

                    // Get length of list
                    int length = consumedItems.Count;
                    //Log.Debug("Length: " + length);
                    // for each scissors in inventory remove a random consumed item
                    if (length > 0)
                    {
                        int scrapsCounted = 0;

                        while (grabCount > 0)
                        {
                            grabCount--; // Reduce usage first incase of break
                            int itemPos = Random.Range(0, length);
                            ItemDef selectedItem = consumedItems[itemPos]; // Don't need to subtract 1 from length because random excludes the max
                            // Remove 1 consumed item
                            //Log.Debug("Removing " + selectedItem.name); // + " at " + itemPos);
                            self.inventory.RemoveItem(selectedItem);

                            // Give 2 white scraps
                            //self.inventory.GiveItem(ItemCatalog.FindItemIndex("ScrapWhite"), scrapsPerConsumed);
                            scrapsCounted += scrapsPerConsumed;
                            CharacterMasterNotificationQueue.SendTransformNotification(
                                self.master,
                                selectedItem.itemIndex,
                                RoR2Content.Items.ScrapWhite.itemIndex,
                                CharacterMasterNotificationQueue.TransformationType.Default);

                            // If ran out of that consumable item in player's inventory
                            if (self.inventory.GetItemCount(selectedItem) <= 0)
                            {
                                Log.Debug("Out of " + selectedItem.name);
                                //Log.Debug("New length of " + (length - 1));
                                consumedItems.RemoveAt(itemPos);
                                length--;
                                // If list is empty break loop
                                if (length == 0)
                                {
                                    Log.Debug("Scissors can't cut Empty List");
                                    break;
                                }
                            }
                        }
                        self.inventory.GiveItem(ItemCatalog.FindItemIndex("ScrapWhite"), scrapsCounted);
                    }
                    else
                    {
                        // Player doesn't have any consumed items
                        Log.Warning(self.GetUserName() + " has no consumed items: Scissors cuts itself");
                        // Remove a scissors (Garenteed to have at least scissors)
                        self.inventory.RemoveItem(item);
                        // Give 2 white scraps
                        self.inventory.GiveItem(ItemCatalog.FindItemIndex("ScrapWhite"), scrapsPerConsumed);
                    }
                    // Really doesn't need sound, can't hear anyways
                    //Util.PlaySound("Play_merc_sword_impact", self.gameObject);
                }
            }
        }
        //*/

    }
}