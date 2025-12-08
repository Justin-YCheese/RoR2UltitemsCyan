using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
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
            On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;

            // Chance of Death
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;

            // Increase cauldron and 3D printer cost
            On.RoR2.PurchaseInteraction.OnInteractionBegin += PurchaseInteraction_OnInteractionBegin;
            // Increase scrapper cost
            On.RoR2.ScrapperController.BeginScrapping_UniquePickup += ScrapperController_BeginScrapping_UniquePickup;
            //On.RoR2.ScrapperController.BeginScrapping += ScrapperController_BeginScrapping; // Old Scrapper Method
            // Remove Item on Death
            On.RoR2.CharacterBody.OnDeathStart += CharacterBody_OnDeathStart;
        }

        private int MaxStack(Inventory inv)
        {//Test This Code
            return Math.Min(inv.GetItemCountEffective(item), maxStack);
        }

        // Chance of Death on hit
        private void HealthComponent_TakeDamageProcess(ILContext il)
        {
            ILCursor c = new(il); // Make new ILContext

            int num14 = -1;

            //Log.Warning("Silver Thread Take Damage");

            // Inject code just before damage is subtracted from health
            // Go just before the "if (num14 > 0f && this.barrier > 0f)" line, which is equal to the following instructions

            // 1170 ldloc.s V_49 (49)             // Previous For loop k value
            if (c.TryGotoNext(MoveType.Before,                        // 1673 callvirt remove Buff... // Previous For Loop branch
                x => x.MatchLdloc(out num14),                         // 1674 ldloc.s V_8 (8)
                x => x.MatchLdcR4(0f),                                // 1675 ldc.r4 0
                x => x.Match(OpCodes.Ble_Un_S),                       // 1676 ble.un.s 1200 (0DE8) ldloc.s V_8 (8)
                x => x.MatchLdarg(0),                                 // 1677 ldarg.0
                x => x.MatchLdfld<HealthComponent>("barrier"),        // 1678 ldfld float32 RoR2.HealthComponent::barrier
                x => x.MatchLdcR4(0f),                                // 1679 ldc.r4 0
                x => x.Match(OpCodes.Ble_Un_S))                       // 1680 ble.un.s 1200 (0DE8) ldloc.s V_8 (8)
            )
            {

                //Log.Debug(" * * * Start C Index: " + c.Index + " > " + c.ToString());
                // [Warning:UltitemsCyan] * **Start C Index: 1173 > // ILCursor: System.Void DMD<RoR2.HealthComponent::TakeDamage>?-822050560::RoR2.HealthComponent::TakeDamage(RoR2.HealthComponent,RoR2.DamageInfo), 1173, Next
                // IL_0e8a: blt.s IL_0e70
                // IL_0e8f: ldloc.s V_7

                c.Index++;


                //Log.Debug(" * * * +1 Working Index: " + c.Index + " > " + c.ToString());
                // [Warning:UltitemsCyan]  * * * Working Index: 1174 > // ILCursor: System.Void DMD<RoR2.HealthComponent::TakeDamage>?-822050560::RoR2.HealthComponent::TakeDamage(RoR2.HealthComponent,RoR2.DamageInfo), 1174, None
                // IL_0e8f: ldloc.s V_7
                // IL_0e91: ldc.r4 0

                //c.GotoNext(MoveType.Before, x => x.MatchLdcR4(0f));
                //Log.Warning(" * * * Before LdcR4 0f: " + c.Index + " > " + c.ToString());
                // [Warning:UltitemsCyan]  * * * Before LdcR4 0f: 1174 > // ILCursor: System.Void DMD<RoR2.HealthComponent::TakeDamage>?-822050560::RoR2.HealthComponent::TakeDamage(RoR2.HealthComponent,RoR2.DamageInfo), 1174, Next
                // IL_0e8f: ldloc.s V_7
                // IL_0e91: ldc.r4 0

                _ = c.Emit(OpCodes.Ldarg, 0);       // Load Health Component
                _ = c.Emit(OpCodes.Ldloc, 1);       // Load Attacker Character Body
                _ = c.Emit(OpCodes.Ldloc, num14);   // Load Total Damage

                // Run custom code
                _ = c.EmitDelegate<Action<HealthComponent, CharacterBody, float>>((hc, aCb, td) =>
                {
                    CharacterBody cb = hc.body;
                    //Log.Debug("Health: " + hc.fullCombinedHealth + "\t Body: " + cb.GetUserName() + "\t Damage: " + td);
                    //Log.Warning("Damage Info " + di.ToString() + " with " + di.damage + " initial damage");
                    if (cb && cb.master && cb.master.inventory && aCb)
                    {
                        //if (aCb.master.inventory){ Log.Debug("and has inventory"); }
                        int grabCount = MaxStack(cb.master.inventory);
                        if (grabCount > 0)
                        {
                            Log.Debug(cb.GetUserName() + " takes " + td + "\t silver thread damage with " + hc.fullCombinedHealth + "\t health");
                            float deathChance = td / hc.fullCombinedHealth * 100f * grabCount;
                            if (deathChance > 100f)
                            {
                                deathChance = 100f;
                            }
                            Log.Debug("Chance of Snapping: " + deathChance);
                            if (Util.CheckRoll(deathChance))
                            {
                                SnapBody(cb, aCb);
                            }
                        }
                    }
                });
                //Log.Debug(il.ToString());
            }
            else
            {
                Log.Warning("Silver cannot find '(num14 > 0f && this.barrier > 0f)'");
            }
        }

        // Kill character body
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
                body.inventory.RemoveItemPermanent(lastItem.itemIndex, body.inventory.GetItemCountPermanent(lastItem));
                body.inventory.RemoveItemTemp(lastItem.itemIndex, body.inventory.GetItemCountTemp(lastItem));
                body.inventory.RemoveItemChanneled(lastItem.itemIndex, body.inventory.GetItemCountChanneled(lastItem));
                //body.inventory.GiveItem(SilverThreadConsumed.item); // Don't actually give item but at least send notification of a broken item
                CharacterMasterNotificationQueue.SendTransformNotification(
                    body.master,
                    item.itemIndex,
                    SilverThreadConsumed.item.itemIndex,
                    CharacterMasterNotificationQueue.TransformationType.Default);
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
                    Log.Debug("Removing Silver threads from " + self.GetUserName()); //-JYPrint
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
                    Log.Warning("Silver Purchase check");
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
                if (player && player.master.inventory)
                {
                    if (player.master.inventory.GetItemCountEffective(item) > 0)
                    {
                        //Log.Debug("Silver Scrapping custom function");
                        // body has a silver thread in their inventory
                        runOrig = false;

                        //self.itemsEaten = 0; // TODO REPLACE
                        PickupDef pickupDef = PickupCatalog.GetPickupDef(pickupToTake.pickupIndex);
                        if (pickupDef != null && self.interactor)
                        {
                            //self.lastScrappedItemIndex = pickupDef.itemIndex;
                            int scrapCount = Mathf.Min(self.maxItemsToScrapAtATime * costMultiplier, player.inventory.GetItemCountPermanent(pickupDef.itemIndex));
                            //Log.Debug("Scrap Count: " + scrapCount);
                            //Log.Debug(player.master.inventory.GetItemCount((ItemIndex)intPickupIndex) + " =? " + player.inventory.GetItemCount(pickupDef.itemIndex));
                            if (scrapCount < costMultiplier)
                            {
                                // not enough items to convert item, don't return anything
                                //Log.Debug("Silver Scrapper Consume poor items");
                                //self.itemsEaten = -1; // TODO REPLACE
                            }
                            else
                            {
                                // return reduced amount

                                // So basically the new scrappers know what item they are taking, and will return temporary items
                                // if it used temorary scrap


                                /*
                                Log.Debug("scrapCount: " + scrapCount + " returnCount: " + scrapCount / costMultiplier);
                                player.inventory.RemoveItemPermament(pickupDef.itemIndex, scrapCount);
                                self.itemsEaten += scrapCount / costMultiplier; // TODO REPLACE
                                for (int i = 0; i < scrapCount; i++)
                                {
                                    ScrapperController.CreateItemTakenOrb(player.corePosition, self.gameObject, pickupDef.itemIndex);
                                }
                                if (self.esm)
                                {
                                    self.esm.SetNextState(new EntityStates.Scrapper.WaitToBeginScrapping());
                                }
                                */
                            }
                        }
                    }
                }
            }
            // If checks failed, run original function
            if (runOrig)
            {
                Log.Debug("runOrig in SilverThread");
                orig(self, pickupToTake);
                Log.Debug("runOrig out SilverThread");
            }
        }

        // Make Scrapper return fewer items per Silver Thread Held
        // ******************** OLD OLD OLD CODE CODE CODE ********************
        /*
        private void ScrapperController_BeginScrapping(On.RoR2.ScrapperController.orig_BeginScrapping orig, ScrapperController self, int intPickupIndex)
        {
            Log.Warning("Silver Scrapping check");
            bool runOrig = true;
            if (NetworkServer.active && self)
            {
                CharacterBody player = self.interactor.GetComponent<CharacterBody>();
                if (player && player.master.inventory)
                {
                    //int grabSilverCount = MaxStack(player.master.inventory);
                    if (player.master.inventory.GetItemCountEffective(item) > 0)
                    {
                        //Log.Debug("Silver Scrapping custom function");
                        // body has a silver thread in their inventory
                        runOrig = false;

                        //self.itemsEaten = 0;
                        PickupDef pickupDef = PickupCatalog.GetPickupDef(new PickupIndex(intPickupIndex));
                        if (pickupDef != null && self.interactor)
                        {
                            //self.lastScrappedItemIndex = pickupDef.itemIndex;
                            int scrapCount = Mathf.Min(self.maxItemsToScrapAtATime * costMultiplier, player.inventory.GetItemCountPermanent(pickupDef.itemIndex));
                            //Log.Debug("Scrap Count: " + scrapCount);
                            //Log.Debug(player.master.inventory.GetItemCount((ItemIndex)intPickupIndex) + " =? " + player.inventory.GetItemCount(pickupDef.itemIndex));
                            if (scrapCount < costMultiplier)
                            {
                                // not enough items to convert item, don't return anything
                                //Log.Debug("Silver Scrapper Consume poor items");
                                //self.itemsEaten = -1;
                            }
                            else
                            {
                                // return reduced amount
                                //Log.Debug("scrapCount: " + scrapCount + " returnCount: " + scrapCount / costMultiplier);
                                player.inventory.RemoveItem(pickupDef.itemIndex, scrapCount);
                                //self.itemsEaten += scrapCount / costMultiplier;
                                for (int i = 0; i < scrapCount; i++)
                                {
                                    ScrapperController.CreateItemTakenOrb(player.corePosition, self.gameObject, pickupDef.itemIndex);
                                }
                                if (self.esm)
                                {
                                    self.esm.SetNextState(new EntityStates.Scrapper.WaitToBeginScrapping());
                                }
                            }
                        }
                    }
                }
            }
            // If checks failed, run original function
            if (runOrig)
            {
                //Log.Debug("runOrig in SilverThread");
                orig(self, intPickupIndex);
                //Log.Debug("runOrig out SilverThread");
            }
        }
        */

        // Increase Items gained when given
        public void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            //Log.Debug("SilverThread please start give item");
            // && !inSilverAlready
            if (!ItemCatalog.GetItemDef(itemIndex))
            {
                Log.Debug("SilverThread found impossible item? Index: " + itemIndex);
            }
            if (NetworkServer.active && count == 1 && self && ItemCatalog.GetItemDef(itemIndex).tier != ItemTier.NoTier && itemIndex != item.itemIndex)
            {
                // Precaution incase something causes an infinity loop of items
                //inSilverAlready = true;
                //Log.Debug("Do you have silver?");
                int grabCount = MaxStack(self);
                if (grabCount > 0)
                {
                    //Log.Debug("yes I do");
                    //Log.Debug("Thread Chance: " + (baseThreadChance + (stackThreadChance * (grabCount - 1))));
                    if (Util.CheckRoll(baseThreadChance + stackThreadChance * (grabCount - 1)))
                    {
                        Log.Debug("Extra silver: " + ItemCatalog.GetItemDef(itemIndex).name);
                        // TODO add effect
                        count += extraItemAmount;
                    }

                }
            }
            //Log.Debug("GiveItem_ItemIndex in orig SilverThread");
            orig(self, itemIndex, count);
            //Log.Debug("GiveItem_ItemIndex out orig SilverThread");
        }
    }
}