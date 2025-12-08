using RoR2;
using UltitemsCyan.Items.Untiered;
using UnityEngine.Networking;
using BepInEx.Configuration;
using UltitemsCyan.Buffs;

namespace UltitemsCyan.Items.Tier3
{

    // TODO: check if Item classes needs to be public
    public class CorrodingVault : ItemBase
    {
        public static ItemDef item;
        private const int quantityInVault = 15;
        //private const int bonusInVault = 0;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Corroding Vault";
            if (!CheckItemEnabledConfig(itemName, "Red", configs))
            {
                return;
            }
            item = CreateItemDef(
                "CORRODINGVAULT",
                itemName,
                "Breaks at the start of the next stage. Contains white items.",
                "At the start of each stage, this item will <style=cIsUtility>break</style> and gives <style=cIsUtility>15</style> unique white items",
                "This vault is sturdy, but over time the rust will just crack it open",
                ItemTier.Tier3,
                UltAssets.CorrodingVaultSprite,
                UltAssets.CorrodingVaultPrefab,
                [ItemTag.Utility, ItemTag.OnStageBeginEffect, ItemTag.AIBlacklist]
            );
        }


        protected override void Hooks()
        {
            On.RoR2.Stage.BeginServer += Stage_BeginServer;
            //On.RoR2.Run.BeginStage += Run_BeginStage;
            //CharacterBody.onBodyStartGlobal += CharacterBody_onBodyStartGlobal;
        }

        private void Stage_BeginServer(On.RoR2.Stage.orig_BeginServer orig, Stage self)
        {
            orig(self);
            if (!NetworkServer.active)
            {
                //Log.Debug("Running on Client... return...");
                return;
            }
            foreach (CharacterMaster master in CharacterMaster.readOnlyInstancesList)
            {
                if (master.inventory)
                {
                    // Get number of vaults
                    int grabCount = master.inventory.GetItemCountEffective(item.itemIndex);
                    if (grabCount > 0)
                    {
                        // Prioritize consuming temporary Vaults before permanent ones
                        // Consume Corroding Vault
                        Inventory.ItemTransformation.TryTransformResult tryTransformResult;
                        if (new Inventory.ItemTransformation
                        {
                            originalItemIndex = item.itemIndex,
                            newItemIndex = CorrodingVaultConsumed.item.itemIndex,
                            maxToTransform = 1,
                            transformationType = 0
                        }.TryTransform(master.inventory, out tryTransformResult))
                        {
                            // If item succesfully transformed
                            Log.Debug(" Corroding my Vault! ");

                            //tryTransformResult.givenItem

                            // Get all white items
                            PickupIndex[] allWhiteItems = new PickupIndex[Run.instance.availableTier1DropList.Count];
                            Run.instance.availableTier1DropList.CopyTo(allWhiteItems);
                            int length = allWhiteItems.Length;

                            Xoroshiro128Plus rng = new(Run.instance.stageRng.nextUlong);

                            // Give 15 different white items
                            for (int i = 0; i < quantityInVault; i++)
                            {
                                int randItemPos = rng.RangeInt(0, length);

                                ItemIndex foundItem = PickupCatalog.GetPickupDef(allWhiteItems[randItemPos]).itemIndex;
                                master.inventory.GiveItemPermanent(foundItem); //TODO should this match vault's state?

                                GenericPickupController.SendPickupMessage(master, new UniquePickup(allWhiteItems[randItemPos]));

                                allWhiteItems[randItemPos] = allWhiteItems[length - 1];
                                length--;

                                if (length == 0)
                                {
                                    Log.Debug("Ran out of white items...   Resetting Pool");
                                    length = allWhiteItems.Length;
                                    Run.instance.availableTier1DropList.CopyTo(allWhiteItems);
                                }
                            }
                        }

                        
                    }
                }
            }
        }
    }
}