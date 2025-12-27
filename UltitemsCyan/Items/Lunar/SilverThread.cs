using BepInEx.Configuration;
using RoR2;
using static System.Math;
using System.Collections.Generic;
using UltitemsCyan.Items.Untiered;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UltitemsCyan.Items.Lunar
{

    // TODO: check if Item classes needs to be public
    public class SilverThread : ItemBase
    {
        public static ItemDef item;

        private const int maxStack = 3;
        private const int extraItemAmount = 1;
        private const int costMultiplier = extraItemAmount + 1; // 2

        //private const float percentPerStack = 50f;
        //private const float deathSnapTime = 600f; // 10 minutes

        private const float baseThreadChance = 50f;
        private const float stackThreadChance = 25f;

        //private bool inSilverAlready = false;

        private static readonly GameObject BrittleDeathEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/BrittleDeath.prefab").WaitForCompletion();

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Silver Thread";
            if (!CheckItemEnabledConfig(itemName, "Lunar", configs))
            {
                return;
            }
            item = CreateItemDef(
                "SILVERTHREAD",
                itemName,
                "Chance to gain additional items... <style=cDeath>BUT doubles printer cost</style> and <style=cDeath>chance of losing items or dying upon being attacked</style>. Upon death, this item will be consumed.",
                "<style=cIsUtility>50%</style> <style=cStack>(+25% chance per stack)</style> chance to pick up <style=cIsUtility>1</style> additional item. Printers and cauldrons cost are <style=cDeath>doubled</style>. You have a chance of <style=cDeath>snapping</style> equal to <style=cIsUtility>100%</style> <style=cStack>(+100% per stack)</style> of health lost. Snapping either <style=cDeath>breaks your last item</style> or <style=cDeath>kills you</style>. <style=cIsUtility>Upon death</style>, this item will be <style=cIsUtility>consumed</style>. <style=cIsUtility>Unaffected by luck</style>.",
                "The end of the abacus of life. A King's Riches lays before you, but at the end of a strand which has been snapped intwine.",
                ItemTier.Lunar,
                UltAssets.SilverThreadSprite,
                UltAssets.SilverThreadPrefab,
                [ItemTag.Utility]
            );
        }

        protected override void Hooks()
        {
            // Gain additional items
            //On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int += Inventory_GiveItemPermanent_ItemIndex_int;
            On.RoR2.Inventory.GiveItemTemp += Inventory_GiveItemTemp;

            // Chance of Death
            //IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            On.RoR2.HealthComponent.SendDamageDealt += HealthComponent_SendDamageDealt;

            // Increase cauldron and 3D printer cost
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            // Increase scrapper cost
            On.RoR2.ScrapperController.BeginScrapping_UniquePickup += ScrapperController_BeginScrapping_UniquePickup;
            // Remove Item on Death
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
        }
        private int MaxStack(Inventory inv)
        {
            return Min(inv.GetItemCountEffective(item), maxStack);
        }

        // Return true if Silver Thread should give an additional item
        private bool CheckSilverThreadRoll(Inventory inventory, ItemIndex itemIndex)
        {
            //Log.Debug(" ??? ??? SilverThread Checking Rolls");
            int grabCount = MaxStack(inventory);
            if (grabCount <= 0 || itemIndex == 0 || !ItemCatalog.GetItemDef(itemIndex))
            {
                //Log.Debug("SilverThread found either none or impossible item? Index: " + itemIndex);
            }
            else if (NetworkServer.active && ItemCatalog.GetItemDef(itemIndex).tier != ItemTier.NoTier && itemIndex != item.itemIndex)
            {
                //Log.Debug("yes I do");
                //Log.Debug("Thread Chance: " + (baseThreadChance + stackThreadChance * (grabCount - 1)));
                if (Util.CheckRoll(baseThreadChance + stackThreadChance * (grabCount - 1)))
                {
                    //Log.Debug("Extra silver: " + ItemCatalog.GetItemDef(itemIndex).name);
                    // TODO add effect
                    return true;
                }
            }
            return false;
        }

        private void Inventory_GiveItemTemp(On.RoR2.Inventory.orig_GiveItemTemp orig, Inventory self, ItemIndex itemIndex, float countToAdd)
        {
            //Log.Debug("SilverThread got Temporary Item");
            if (countToAdd == 1f && self && CheckSilverThreadRoll(self, itemIndex))
            {
                orig(self, itemIndex, countToAdd + 1f);
            }
            else
            {
                orig(self, itemIndex, countToAdd);
            }
        }

        private void Inventory_GiveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_GiveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int countToAdd)
        {
            //Log.Debug("SilverThread got Permanent Permanent Item");
            if (countToAdd == 1 && self && CheckSilverThreadRoll(self, itemIndex))
            {
                orig(self, itemIndex, countToAdd + 1);
            }
            else
            {
                orig(self, itemIndex, countToAdd);
            }
        }

        private void HealthComponent_SendDamageDealt(On.RoR2.HealthComponent.orig_SendDamageDealt orig, DamageReport damageReport)
        {
            orig(damageReport);

            CharacterBody attackerBody = damageReport.attackerBody;
            CharacterBody victimBody = damageReport.victimBody;
            //Log.Debug("Health: " + hc.fullCombinedHealth + "\t Body: " + cb.GetUserName() + "\t Damage: " + td);
            //Log.Warning("Damage Info " + di.ToString() + " with " + di.damage + " initial damage");
            if (victimBody && victimBody.master && victimBody.master.inventory && attackerBody)
            {
                //if (aCb.master.inventory){ Log.Debug("and has inventory"); }
                int grabCount = MaxStack(victimBody.master.inventory);
                if (grabCount > 0)
                {
                    //Log.Debug(victimBody.GetUserName() + " takes " + damageReport.damageDealt + "\t silver thread damage with " + victimBody.healthComponent.fullCombinedHealth + "\t health");
                    float deathChance = damageReport.damageDealt / victimBody.healthComponent.fullCombinedHealth * 100f * grabCount;
                    if (deathChance > 100f)
                    {
                        deathChance = 100f;
                    }
                    //Log.Debug("Chance of Snapping: " + deathChance);
                    if (Util.CheckRoll(deathChance))
                    {
                        SnapBody(victimBody, attackerBody);
                    }
                }
            }
        }

        // Thread snaps, Lose life or items
        private static void SnapBody(CharacterBody body, CharacterBody killer)
        {
            Log.Warning(body.GetUserName() + "'s thread was snapped by chance");

            EffectManager.SpawnEffect(BrittleDeathEffect, new EffectData
            {
                origin = body.transform.position,
                rotation = Quaternion.identity,
                scale = 2f,
            }, true);

            if (Util.CheckRoll(50f, 0))
            {
                // Remove Item Stack
                Log.Debug("An item snapped...");
                // TODO doublecheck this orders temporary items as well
                List<ItemIndex> itemList = body.inventory.itemAcquisitionOrder;
                ItemDef lastItem = ItemCatalog.GetItemDef(itemList[^1]);
                // Remove full stack including Permanent, Temp, and Channeled
                body.inventory.ResetItemPermanent(lastItem.itemIndex);
                body.inventory.ResetItemTemp(lastItem.itemIndex);
                //body.inventory.RemoveItemChanneled(lastItem.itemIndex, body.inventory.GetItemCountChanneled(lastItem));
                // Give a Silver Threads for the stack broken
                body.inventory.GiveItemPermanent(SilverThreadConsumed.item.itemIndex);
                CharacterMasterNotificationQueue.SendTransformNotification(
                    body.master,
                    lastItem.itemIndex,
                    SilverThreadConsumed.item.itemIndex,
                    CharacterMasterNotificationQueue.TransformationType.None);
            }
            else
            {
                // Kill Player
                Log.Warning("The player snapped...");
                Chat.AddMessage("Your thread of life has snapped...");
                body.healthComponent.Suicide(killer.gameObject);
            }
        }

        // Remove Silver on Death
        private void CharacterBody_OnDeathStart(On.RoR2.CharacterBody.orig_OnDeathStart orig, CharacterBody self)
        {
            orig(self);
            //Log.Warning("Silver Normal Death?");
            if (self && self.master && self.master.inventory)
            {
                int grabCount = self.master.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    _ = new Inventory.ItemTransformation
                    {
                        originalItemIndex = item.itemIndex, // Silver Thread
                        newItemIndex = SilverThreadConsumed.item.itemIndex, // Snapped Silver Thread
                        maxToTransform = 2147483647, // Transforms all
                        transformationType = 0
                    }.TryTransform(self.master.inventory, out _);
                }
            }
        }

        // Increase 3D printer / Cauldron cost
        private void PurchaseInteraction_OnInteractionBegin(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            bool runOrig = true;
            if (NetworkServer.active && self && activator && activator)
            {
                // If an item cost other than treasure cache cost
                if (self.costType is
                    CostTypeIndex.WhiteItem or
                    CostTypeIndex.GreenItem or
                    CostTypeIndex.RedItem or
                    CostTypeIndex.BossItem or
                    CostTypeIndex.LunarItemOrEquipment
                    )
                {
                    //Log.Debug("Silver Purchase check");
                    CharacterBody player = activator.GetComponent<CharacterBody>();
                    if (player.master.inventory.GetItemCountEffective(item) > 0)
                    {
                        runOrig = false;
                        //Log.Debug("Self Cost? " + self.cost + " * " + costMultiplier);
                        self.cost *= costMultiplier;
                        //Log.Debug("New Self Cost? " + self.cost);

                        orig(self, activator);

                        self.cost /= costMultiplier;
                        //Log.Debug("Post Self Cost? " + self.cost);
                    }
                }
            }
            //
            if (runOrig)
            {
                orig(self, activator);
            }//*/
        }

        // Make Scrapper return fewer items per Silver Thread Held
        private void ScrapperController_BeginScrapping_UniquePickup(On.RoR2.ScrapperController.orig_BeginScrapping_UniquePickup orig, ScrapperController self, UniquePickup pickupToTake)
        {
            Log.Warning("My ****NEW**** Silver Scrapping check");
            bool runOrig = true;
            if (NetworkServer.active && self)
            {
                CharacterBody player = self.interactor.GetComponent<CharacterBody>();
                if (player && player.master.inventory && player.master.inventory.GetItemCountEffective(item) > 0)
                {
                    Log.Debug("Silver Scrapping custom function");
                    // body has a silver thread in their inventory
                    runOrig = false;

                    PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupToTake.pickupIndex);
                    if (pickupDef == null) { return; }

                    ItemDef itemDef = ItemCatalog.GetItemDef((pickupDef != null) ? pickupDef.itemIndex : ItemIndex.None);
                    if (!itemDef) { return; }

                    PickupDef scrapPickupDef = PickupCatalog.GetPickupDef(PickupCatalog.FindScrapIndexForItemTier(itemDef.tier));
                    ItemIndex scrapItemIndex = (scrapPickupDef != null) ? scrapPickupDef.itemIndex : ItemIndex.None;

                    if (!self.interactor.TryGetComponent(out CharacterBody characterBody)) { return; }
                    Inventory inventory = characterBody.inventory;
                    if (!inventory) { return; }

                    // Get Item Count
                    // Scrap Limit is so that the item only scraps a multiple of the cost multiplier
                    int silverScrapLimit;
                    if (pickupToTake.isTempItem)
                    {
                        int count = Mathf.FloorToInt(player.master.inventory.GetItemCountTemp(itemDef.itemIndex));
                        silverScrapLimit = count - count % costMultiplier;
                    }
                    else
                    {
                        int count = player.master.inventory.GetItemCountPermanent(itemDef.itemIndex);
                        silverScrapLimit = count - count % costMultiplier;
                    }

                    Log.Debug(" Silver Scrap Limit: " + silverScrapLimit);

                    if (silverScrapLimit < costMultiplier)
                    {
                        return;
                    }

                    Inventory.ItemTransformation.TryTransformResult tryTransformResult;
                    if (new Inventory.ItemTransformation
                    {
                        allowWhenDisabled = false,
                        forbidPermanentItems = pickupToTake.isTempItem,
                        forbidTempItems = !pickupToTake.isTempItem,
                        minToTransform = costMultiplier,
                        maxToTransform = silverScrapLimit,
                        originalItemIndex = itemDef.itemIndex,
                        newItemIndex = ItemIndex.None,
                        transformationType = ItemTransformationTypeIndex.None
                    }.TryTransform(inventory, out tryTransformResult))
                    {
                        //Log.Debug(" stackValues before: " + tryTransformResult.takenItem.stackValues.permanentStacks + " | " + tryTransformResult.takenItem.stackValues.temporaryStacksValue);
                        tryTransformResult.takenItem.stackValues.permanentStacks /= costMultiplier;
                        tryTransformResult.takenItem.stackValues.temporaryStacksValue /= costMultiplier;
                        //Log.Debug(" stackValues after: " + tryTransformResult.takenItem.stackValues.permanentStacks + " | " + tryTransformResult.takenItem.stackValues.temporaryStacksValue);
                        Inventory.ItemAndStackValues takenItem = tryTransformResult.takenItem;
                        takenItem.itemIndex = scrapItemIndex;
                        _ = takenItem.AddAsPickupsToList(self.pickupPrintQueue);
                        if (characterBody)
                        {
                            for (int i = 0; i < tryTransformResult.totalTransformed; i++)
                            {
                                ScrapperController.CreateItemTakenOrb(characterBody.corePosition, self.gameObject, tryTransformResult.takenItem.itemIndex);
                            }
                        }
                    }
                    if (self.esm)
                    {
                        self.esm.SetNextState(new EntityStates.Scrapper.WaitToBeginScrapping());
                    }
                }
            }
            // If checks failed, run original function
            if (runOrig)
            {
                //Log.Debug("runOrig in SilverThread");
                orig(self, pickupToTake);
                //Log.Debug("runOrig out SilverThread");
            }
        }

    }
}