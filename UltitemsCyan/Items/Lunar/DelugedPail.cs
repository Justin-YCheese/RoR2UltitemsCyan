using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using HG;

namespace UltitemsCyan.Items.Lunar
{

    // TODO: check if Item classes needs to be public
    public class DelugedPail : ItemBase
    {
        public static ItemDef item;

        private const float attackPerWhite = 2.5f;
        private const float regenPerGreen = 0.05f;
        private const float speedPerRed = 10f;
        private const float critPerBoss = 10f;
        //private const float armourPerMisc = 2f;
        //private const float healthPerLunar = 5f;
        private const float jumpPerLunar = 1f;
        private const float stackPercent = 20f;

        public bool inDelugedAlready = false;

        public readonly GameObject ShrineUseEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/ShrineUseEffect.prefab").WaitForCompletion();

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Deluged Pail";
            if (!CheckItemEnabledConfig(itemName, "Lunar", configs))
            {
                return;
            }
            item = CreateItemDef(
                "DELUGEDPAIL",
                itemName,
                "Gain stats for each item held... <style=cDeath>BUT picking up an item triggers a restack.</style>",
                "Gain <style=cIsDamage>2.5% attack</style> per common, <style=cIsHealing>0.05 regen</style> per <style=cIsHealing>uncommon</style>, <style=cIsUtility>10% speed</style> per legendary</style>, <style=cIsDamage>10% crit</style> per <style=cIsDamage>boss</style> item, and <style=cIsUtility>1% jump height</style> per <style=cIsUtility>lunar</style> <style=cStack>(+20% of each stat per stack)</style>. Trigger a <style=cDeath>restack</style> for non lunar items.",
                "It's a tuning fork? no it's just a sand pail. The sand in the pail shifts with a sound which hums through it. Like a melody of waves, or to be less romantic, like a restless static.",
                ItemTier.Lunar,
                UltAssets.SandPailSprite,
                UltAssets.SandPailPrefab,
                [ItemTag.Utility]
            );
        }


        protected override void Hooks()
        {
            //On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;

            // Calculate Stats
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;

            // Trigger Restack of picking up an item
            On.RoR2.Inventory.GiveItemPermanent_ItemIndex_int += Inventory_GiveItemPermanent_ItemIndex_int;
            On.RoR2.Inventory.GiveItemTemp += Inventory_GiveItemTemp;
            //On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
        }

        private void CheckPailRestack(Inventory inventory, ItemIndex itemIndex)
        {
            if (NetworkServer.active && !inDelugedAlready && inventory && inventory.GetItemCountEffective(item) > 0) // Hopefully fix multiple triggers and visual bug?
            {
                ItemDef iDef = ItemCatalog.GetItemDef(itemIndex);
                ItemTierDef iTierDef = ItemTierCatalog.GetItemTierDef(iDef.tier);
                // Validate check, and pass if not lunar unless is pail
                if (iDef && iTierDef && iTierDef.canRestack && (iTierDef.tier != ItemTier.Lunar || iDef == item)) // Valid Check (check iDef and iTierDef)
                {
                    inDelugedAlready = true;
                    CharacterBody player = CharacterBody.readOnlyInstancesList.ToList().Find((body) => body.inventory == inventory);
                    if (player)
                    {
                        //Log.Warning("Spork the inventory");
                        SporkRestackInventory(inventory, new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));
                        // Effect after restock
                        EffectManager.SpawnEffect(ShrineUseEffect, new EffectData
                        {
                            origin = player.transform.position,
                            rotation = Quaternion.identity,
                            scale = 0.5f,
                            color = new Color(0.2392f, 0.8196f, 0.917647f) // Cyan Lunar color
                        }, true);
                        /*
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
                        {
                            origin = base.transform.position,
                            rotation = Quaternion.identity,
                            scale = 1f,
                            color = new Color(1f, 0.23f, 0.6337214f)
                        }, true);
                        //*/
                    }
                    inDelugedAlready = false;
                }
            }
        }

        private void Inventory_GiveItemTemp(On.RoR2.Inventory.orig_GiveItemTemp orig, Inventory self, ItemIndex itemIndex, float countToAdd)
        {
            orig(self, itemIndex, countToAdd);
            CheckPailRestack(self, itemIndex);
        }

        private void Inventory_GiveItemPermanent_ItemIndex_int(On.RoR2.Inventory.orig_GiveItemPermanent_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int countToAdd)
        {
            orig(self, itemIndex, countToAdd);
            CheckPailRestack(self, itemIndex);
        }

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory) // Valid Check
            {
                Inventory inventory = sender.inventory;
                int grabCount = inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    //Log.Warning("Sonorous Recalculate");
                    // Boss - Crits
                    // White - Damage
                    // Green - Healing
                    // Red - Speed
                    // VoidWhite - Damage
                    // VoidGreen - Healing
                    // VoidRed - Speed
                    // Lunar - Health? Jump Power?
                    // Untiered - Armor?
                    // Armor, Attack Speed, Health,
                    // Jump Power, Shield, Cooldowns
                    int[] statTiers = new int[6]; // 0:Misc  1:Damage  2:Healing  3:Speed  4:Crits  5:Jump Height

                    List<ItemIndex> playerItems = inventory.itemAcquisitionOrder;

                    // Go Through All Items
                    foreach (ItemIndex itemIndex in playerItems)
                    {
                        int tier = 0; // Misc
                        ItemTier itemTier = ItemCatalog.GetItemDef(itemIndex).tier;
                        if (itemTier is ItemTier.Lunar)
                        {
                            tier = 1; // Jump Height
                        }
                        else if (itemTier is ItemTier.Tier1 or ItemTier.VoidTier1)
                        {
                            tier = 2; // Damage
                        }
                        else if (itemTier is ItemTier.Tier2 or ItemTier.VoidTier2)
                        {
                            tier = 3; // Healing
                        }
                        else if (itemTier is ItemTier.Tier3 or ItemTier.VoidTier3)
                        {
                            tier = 4; // Speed
                        }
                        else if (itemTier is ItemTier.Boss or ItemTier.VoidBoss)
                        {
                            tier = 5; // Crits
                        }
                        statTiers[tier] += inventory.GetItemCountEffective(itemIndex);
                    }
                    float statMultiplier = 1f + (grabCount - 1) * stackPercent / 100f;
                    //Log.Debug("stat Multiplier: " + statMultiplier);
                    args.jumpPowerMultAdd += statTiers[1] * jumpPerLunar / 100f * statMultiplier;
                    //Log.Debug("Pail Damage is: " + sender.baseDamage + " + " + statTiers[2] * attackPerWhite * statMultiplier + "%");
                    args.damageMultAdd += statTiers[2] * attackPerWhite / 100f * statMultiplier;
                    // Regen increases per level
                    //Log.Debug("Pail Regen is: " + sender.baseRegen + " + " + statTiers[3] * (regenPerGreen + regenPerGreen / 5 * sender.level) * statMultiplier);
                    args.regenMultAdd += statTiers[3] * regenPerGreen * (1f + 0.2f * sender.level) * statMultiplier;
                    args.moveSpeedMultAdd += statTiers[4] * speedPerRed / 100f * statMultiplier;
                    args.critAdd += statTiers[5] * critPerBoss * statMultiplier;
                }
            }
        }

        //
        private void Inventory_GiveItem_ItemIndex_int(On.RoR2.Inventory.orig_GiveItem_ItemIndex_int orig, Inventory self, ItemIndex itemIndex, int count)
        {
            //Log.Debug("orig IN Sonorous Pail");
            orig(self, itemIndex, count);
            //Log.Debug("orig OUT Sonorous Pail");
            if (NetworkServer.active && !inDelugedAlready && self) // Hopefully fix multiple triggers and visual bug?
            {
                ItemDef iDef = ItemCatalog.GetItemDef(itemIndex);
                ItemTierDef iTierDef = ItemTierCatalog.GetItemTierDef(iDef.tier);
                // Validate check, and pass if not lunar unless is pail
                if (iDef && iTierDef && iTierDef.canRestack && (iTierDef.tier != ItemTier.Lunar || iDef == item)) // Valid Check (check iDef and iTierDef)
                {
                    inDelugedAlready = true;
                    CharacterBody player = CharacterBody.readOnlyInstancesList.ToList().Find((body) => body.inventory == self);
                    if (player && self.GetItemCountEffective(item) > 0) // Valid Check
                    {
                        //Log.Warning("Spork the inventory");
                        SporkRestackInventory(self, new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));
                        // Effect after restock
                        EffectManager.SpawnEffect(ShrineUseEffect, new EffectData
                        {
                            origin = player.transform.position,
                            rotation = Quaternion.identity,
                            scale = 0.5f,
                            color = new Color(0.2392f, 0.8196f, 0.917647f) // Cyan Lunar color
                        }, true);
                        /*
                        EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/ShrineUseEffect"), new EffectData
                        {
                            origin = base.transform.position,
                            rotation = Quaternion.identity,
                            scale = 1f,
                            color = new Color(1f, 0.23f, 0.6337214f)
                        }, true);
                        //*/
                    }
                    inDelugedAlready = false;
                }
            }
        }

        public void SporkRestackInventory(Inventory inventory, Xoroshiro128Plus rng)
        {
            //Log.Debug("Restock my sporks!");
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.Inventory::ShrineRestackInventory(Xoroshiro128Plus)' called on client");
                return;
            }
            using (CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out List<ItemIndex> restackList))
            {
                using (CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out List<ItemIndex> playerItems))
                {
                    bool flag = false;
                    foreach (ItemTierDef itemTierDef in ItemTierCatalog.allItemTierDefs)
                    {
                        if (itemTierDef.canRestack)
                        {
                            int countPerm = 0;
                            float countTemp = 0f;
                            restackList.Clear();
                            playerItems.Clear();
                            inventory.effectiveItemStacks.GetNonZeroIndices(playerItems);
                            foreach (ItemIndex itemIndex in playerItems)
                            {
                                inventory.effectiveItemStacks.GetStackValue(itemIndex);
                                ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                                if (itemTierDef.tier == itemDef.tier && itemDef.DoesNotContainTag(ItemTag.ObjectiveRelated) && itemDef.DoesNotContainTag(ItemTag.PowerShape))
                                {
                                    countPerm += inventory.GetItemCountPermanent(itemIndex);
                                    countTemp += inventory.GetItemCountTemp(itemIndex);
                                    restackList.Add(itemIndex);
                                    //inventory.ResetItemPermanent(itemIndex);
                                    //inventory.ResetItemTemp(itemIndex);
                                }
                            }
                            if (restackList.Count > 0)
                            {
                                ItemIndex keptItem = rng.NextElementUniform(restackList);

                                // Adjust count of kept item
                                inventory.GiveItemPermanent(keptItem, countPerm - inventory.GetItemCountPermanent(keptItem));
                                inventory.GiveItemTemp(keptItem, countTemp - inventory.GetItemCountTemp(keptItem));

                                // Remove all other items
                                _ = restackList.Remove(keptItem);
                                foreach (ItemIndex index in restackList)
                                {
                                    //inventory.itemAcquisitionOrder.Remove(index);
                                    inventory.ResetItemPermanent(index);
                                    inventory.ResetItemTemp(index);
                                }
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        inventory.SetDirtyBit(8U);
                    }
                }
            }
        }
    }
}