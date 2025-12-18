using BepInEx.Configuration;
using R2API;
using RoR2;
using System;
using System.Linq;
using UnityEngine;

namespace UltitemsCyan.Items
{

    // TODO: check if Item classes needs to be public
    public class PowerChips : ItemBase
    {
        public static ItemDef item;
        private const float dontResetFraction = 0.50f;

        public override void Init(ConfigFile configs)
        {
            string itemName = "Power Chips";
            if (!CheckItemEnabledConfig(itemName, "Lunar", configs))
            {
                return;
            }
            item = CreateItemDef(
                "POWERCHIPS",
                itemName,
                "a",
                "b",
                "c",
                ItemTier.Lunar,
                Ultitems.mysterySprite,
                Ultitems.mysteryPrefab,
                //Ultitems.Assets.PowerChipsSprite,
                //Ultitems.Assets.PowerChipsPrefab,
                [ItemTag.Utility]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.ChestBehavior.ItemDrop += ChestBehavior_ItemDrop;
        }

        private void ChestBehavior_ItemDrop(On.RoR2.ChestBehavior.orig_ItemDrop orig, ChestBehavior self)
        {
            //self.tier1Chance = .8f for regular chest
            //Log.Warning("Tier 1 Chance: " + self.tier1Chance);
            //self.dropTransform = Transform.
            //Log.Debug("Player Controller: " + self.playerControllerId);

            //Log.Debug("Rolled Pickup: " + self.HasRolledPickup());
            //Log.Debug("" + self.)
        }
    }
}