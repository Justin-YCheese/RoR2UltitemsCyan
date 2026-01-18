using BepInEx.Configuration;
using R2API;
using RoR2;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using HG;
using static Facepunch.Steamworks.Inventory.Recipe;
using UltitemsCyan.Equipment;
using UltitemsCyan.Items.Tier3;

namespace UltitemsCyan.Items.Food
{

    // TODO: check if Item classes needs to be public
    public class Permaglaze : ItemBase
    {
        public static ItemDef item;

        // TODO change to only 
        private const float regenPercentBase = -25f;
        private const float regenPercentPerStack = 75f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Permaglaze";
            if (!CheckItemEnabledConfig(itemName, "Food", configs))
            {
                return;
            }
            item = CreateItemDef(
                "PERMAGLAZE",
                itemName,
                "Barrier no longer decays. Gain barrier regeneration at full health.",
                "Barrier won't decay. Gain barrier regeneration equal to 50% (+75% per stack) of health regeneration when at full health.",
                "Like Permafrost okay?",
                ItemTier.FoodTier,
                UltAssets.SandPailSprite,
                UltAssets.SandPailPrefab,
                [ItemTag.Utility]
            );

            
        }


        protected override void Hooks()
        {
            // Regen Barrier
            On.RoR2.HealthComponent.Heal += HealthComponent_Heal;
            // Remove Barrier Decay
            On.RoR2.HealthComponent.GetBarrierDecayRate += HealthComponent_GetBarrierDecayRate;
        }

        private float HealthComponent_GetBarrierDecayRate(On.RoR2.HealthComponent.orig_GetBarrierDecayRate orig, HealthComponent self)
        {
            // If you have glaze
            if (self && self.body && self.body.inventory && self.body.inventory.GetItemCountEffective(item) > 0)
            {
                // Then don't lose barrier
                return orig(self) * 0f;
            }
            return orig(self);
        }

        private float HealthComponent_Heal(On.RoR2.HealthComponent.orig_Heal orig, HealthComponent self, float amount, ProcChainMask procChainMask, bool nonRegen)
        {
            // If health regen
            if (self && !nonRegen)
            {
                int grabCount = self.body.inventory.GetItemCountEffective(item);
                //Log.Debug("Get Barrier health fraction: Max Health " + self.body.maxHealth + " | Current Health " + self.health);
                if (grabCount > 0 && self.health >= self.body.maxHealth && amount > 0)
                {
                    self.AddBarrier(amount * (regenPercentBase + grabCount * regenPercentPerStack) / 100f);
                }
            }
            return orig(self, amount, procChainMask, nonRegen);
        }
    }
}