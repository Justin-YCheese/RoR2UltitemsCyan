using BepInEx.Configuration;
using RoR2;
using System;
//using static RoR2.GenericPickupController;

namespace UltitemsCyan.Items.Tier1
{

    // TODO: check if Item classes needs to be public
    public class CremeBrulee : ItemBase
    {
        public static ItemDef item;
        private const float threshold = 100f;
        private const float percentHealing = 4f;
        private const float flatHealing = 16f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Crème Brûlée";
            if (!CheckItemEnabledConfig(itemName, "White", configs))
            {
                return;
            }
            item = CreateItemDef(
                "CREMEBRULEE",
                itemName,
                "Heal when hitting full health enemies.",
                "<style=cIsHealing>Heal</style> for <style=cIsHealing>16</style> plus an additional <style=cIsHealing>4%</style> <style=cStack>(+4% per stack)</style> when dealing damage to <style=cIsDamage>full health</style> enemies",
                "Super Sugar Crust!",
                ItemTier.Tier1,
                UltAssets.CremeBruleeSprite,
                UltAssets.CremeBruleePrefab,
                [ItemTag.CanBeTemporary, ItemTag.Healing]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            try
            {
                // If the victum has an inventory
                // and damage isn't rejected?
                if (self && damageInfo.attacker.GetComponent<CharacterBody>() && damageInfo.attacker.GetComponent<CharacterBody>().inventory && !damageInfo.rejected) //&& damageInfo.damageType != DamageType.DoT
                {
                    CharacterBody inflictor = damageInfo.attacker.GetComponent<CharacterBody>();
                    int grabCount = inflictor.inventory.GetItemCountEffective(item);
                    if (grabCount > 0)
                    {
                        //Log.Warning("La Creme health");
                        //Log.Debug("Health: " + self.health + " Combined Health: " + self.fullHealth + " Combined Fraction: " + self.combinedHealthFraction);
                        if (self.combinedHealthFraction >= threshold / 100f)
                        {
                            //Log.Debug("Heal Attacker, Initial: " + inflictor.healthComponent.health);
                            _ = inflictor.healthComponent.Heal(inflictor.healthComponent.fullHealth * percentHealing / 100f * grabCount + flatHealing, damageInfo.procChainMask);
                            //Log.Debug("Healing: " + ((inflictor.healthComponent.fullHealth * percentHealing / 100f) + flatHealing));
                            _ = Util.PlaySound("Play_item_proc_thorns", inflictor.gameObject);
                            /*/
                            EffectManager.SpawnEffect(LegacyResourcesAPI.Load<GameObject>("Prefabs/Effects/HealthOrbEffect"), new EffectData
                            {
                                origin = self.transform.position,
                                rotation = Quaternion.identity,
                                scale = 0.5f,
                                color = new Color(1, 1, 0.58f) // Cyan Lunar color
                            }, true);//*/
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                //Log.Warning("What La Creme Hit?");
            }
            orig(self, damageInfo);
        }
    }
}