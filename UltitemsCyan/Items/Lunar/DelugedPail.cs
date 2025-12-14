using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

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
                "Gain <style=cIsDamage>2.5% attack</style> per common, <style=cIsHealing>0.05 regen</style> per <style=cIsHealing>uncommon</style>, <style=cIsUtility>10% speed</style> per legendary</style>, <style=cIsDamage>10% crit</style> per <style=cIsDamage>boss</style> item, and <style=cIsUtility>1% jump height</style> per <style=cIsUtility>lunar</style> <style=cStack>(+20% of each stat per stack)</style>. Trigger a <style=cDeath>restack</style> when picking up items.",
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
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.Inventory.GiveItem_ItemIndex_int += Inventory_GiveItem_ItemIndex_int;
        }

        /*/
        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (!inSonorousAlready)
            {
                if (self && self.inventory && self.inventory.GetItemCount(item) > 0)
                {
                    inSonorousAlready = true;
                    Log.Warning("Spork the inventory");
                    //SporkRestackInventory(player.inventory, new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));
                    //self.inventory.ShrineRestackInventory(new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));
                    SporkRestackInventory(self.inventory, self.transform.position, new Xoroshiro128Plus(Run.instance.stageRng.nextUlong));
                    Log.Debug("Effect Spork!");
                    inSonorousAlready = false;
                }
            }
        }//*/

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

                    ItemIndex itemIndex = 0;
                    ItemIndex itemCount = (ItemIndex)ItemCatalog.itemCount;
                    // Go Through All Items
                    while (itemIndex < itemCount)
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
                        // Check next Item
                        itemIndex++;
                    }
                    float statMultiplier = 1f + (grabCount - 1) * stackPercent / 100f;
                    //Log.Debug("stat Multiplier: " + statMultiplier);
                    args.jumpPowerMultAdd += statTiers[1] * jumpPerLunar / 100f * statMultiplier;
                    //Log.Debug("Pail Damage is: " + sender.baseDamage + " + " + (statTiers[1] * attackPerWhite * statMultiplier) + "%");
                    args.damageMultAdd += statTiers[2] * attackPerWhite / 100f * statMultiplier;
                    // Regen increases per level
                    //Log.Debug("Pail Regen is: " + sender.baseRegen + " + " + (statTiers[2] * (regenPerGreen + (regenPerGreen / 5 * sender.level)) * statMultiplier));
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
            if (!ItemCatalog.GetItemDef(itemIndex))
            {
                Log.Debug("Deluged found impossible item? Index: " + itemIndex);
            }
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
                    }
                    inDelugedAlready = false;
                }
            }
        }//*/

        /*/ In Inventory
        public void ShrineRestackInventory([NotNull] Xoroshiro128Plus rng)
		{
			if (!NetworkServer.active)
			{
				Debug.LogWarning("[Server] function 'System.Void RoR2.Inventory::ShrineRestackInventory(Xoroshiro128Plus)' called on client");
				return;
			}
			List<ItemIndex> list;
			using (CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out list))
			{
				List<ItemIndex> list2;
				using (CollectionPool<ItemIndex, List<ItemIndex>>.RentCollection(out list2))
				{
					bool flag = false;
					foreach (ItemTierDef itemTierDef in ItemTierCatalog.allItemTierDefs)
					{
						if (itemTierDef.canRestack)
						{
							int num = 0;
							float num2 = 0f;
							list.Clear();
							list2.Clear();
							this.effectiveItemStacks.GetNonZeroIndices(list2);
							foreach (ItemIndex itemIndex in list2)
							{
								this.effectiveItemStacks.GetStackValue(itemIndex);
								ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
								if (itemTierDef.tier == itemDef.tier && itemDef.DoesNotContainTag(ItemTag.ObjectiveRelated) && itemDef.DoesNotContainTag(ItemTag.PowerShape))
								{
									num += this.GetItemCountPermanent(itemIndex);
									num2 += (float)this.GetItemCountTemp(itemIndex);
									list.Add(itemIndex);
									this.ResetItemPermanent(itemIndex);
									this.ResetItemTemp(itemIndex);
								}
							}
							if (list.Count > 0)
							{
								ItemIndex itemIndex2 = rng.NextElementUniform<ItemIndex>(list);
								this.GiveItemPermanent(itemIndex2, num);
								this.GiveItemTemp(itemIndex2, num2);
								flag = true;
							}
						}
					}
					if (flag)
					{
						base.SetDirtyBit(8U);
					}
				}
			}
		}
        //*/

        public void SporkRestackInventory(Inventory inventory, Xoroshiro128Plus rng)
        {
            //Log.Debug("Restock my sporks!");
            if (!NetworkServer.active)
            {
                Debug.LogWarning("[Server] function 'System.Void RoR2.Inventory::ShrineRestackInventory(Xoroshiro128Plus)' called on client");
                return;
            }
            List<ItemIndex> list = [];
            bool flag = false;
            foreach (ItemTierDef itemTierDef in ItemTierCatalog.allItemTierDefs)
            {
                // In each tier
                //Log.Debug("Which Shelf?: " + itemTierDef.tier);
                if (itemTierDef.canRestack && itemTierDef.tier != ItemTier.Lunar)
                {
                    // Record what items exist and how many items in total
                    int num = 0;
                    list.Clear();
                    
                    // TODO Perhaps use try transform instead?

                    /*/
                    for (int i = 0; i < inventory.itemStacks.Length; i++)
                    {
                        if (inventory.itemStacks[i] > 0) // TODO REPLACE
                        {
                            ItemIndex itemIndex = (ItemIndex)i;
                            //ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
                            if (itemTierDef.tier == ItemCatalog.GetItemDef(itemIndex).tier)
                            {
                                // Add to total items
                                num += inventory.itemStacks[i]; // TODO REPLACE
                                // Add to list
                                list.Add(itemIndex);
                                // Remove from inventory
                                //inventory.itemAcquisitionOrder.Remove(itemIndex);
                            }
                        }
                    }
                    //*/

                    if (list.Count > 0)
                    {
                        // Adjust count of ket item
                        ItemIndex keptItem = rng.NextElementUniform(list);
                        SetItemCount(inventory, keptItem, num);
                        _ = list.Remove(keptItem);
                        // Remove all other items
                        foreach (ItemIndex index in list)
                        {
                            //inventory.itemAcquisitionOrder.Remove(index);
                            SetItemCount(inventory, index, 0);
#pragma warning disable CS0618 // Type or member is obsolete
                            inventory.ResetItem(index);
#pragma warning restore CS0618 // Type or member is obsolete
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

        private void SetItemCount(Inventory inventory, ItemIndex item, int count)
        {
            //var currentCount = inventory.GetItemCount(item);
#pragma warning disable CS0618 // Type or member is obsolete
            inventory.GiveItem(item, count - inventory.GetItemCount(item));
#pragma warning restore CS0618 // Type or member is obsolete
        }
    }
}

/*
public void ShrineRestackInventory([NotNull] Xoroshiro128Plus rng)
{
	if (!NetworkServer.active)
	{
		Debug.LogWarning("[Server] function 'System.Void RoR2.Inventory::ShrineRestackInventory(Xoroshiro128Plus)' called on client");
		return;
	}
	List<ItemIndex> list = new List<ItemIndex>();
	bool flag = false;
	foreach (ItemTierDef itemTierDef in ItemTierCatalog.allItemTierDefs)
	{
		if (itemTierDef.canRestack)
		{
			int num = 0;
			list.Clear();
			for (int i = 0; i < this.itemStacks.Length; i++)
			{
				if (this.itemStacks[i] > 0)
				{
					ItemIndex itemIndex = (ItemIndex)i;
					ItemDef itemDef = ItemCatalog.GetItemDef(itemIndex);
					if (itemTierDef.tier == itemDef.tier)
					{
						num += this.itemStacks[i];
						list.Add(itemIndex);
						this.itemAcquisitionOrder.Remove(itemIndex);
						this.ResetItem(itemIndex);
					}
				}
			}
			if (list.Count > 0)
			{
				this.GiveItem(rng.NextElementUniform<ItemIndex>(list), num);
				flag = true;
			}
		}
	}
	if (flag)
	{
		base.SetDirtyBit(8U);
	}
}
*/