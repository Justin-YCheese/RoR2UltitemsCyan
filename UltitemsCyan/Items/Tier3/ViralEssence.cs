using R2API;
using RoR2;
using BepInEx.Configuration;
using System;

namespace UltitemsCyan.Items.Tier3
{
    // Notes:
    // Luminous Shot's counting status isn't a cooldown buff so it counts for smog

    // TODO: check if Item classes needs to be public
    public class ViralEssence : ItemBase
    {
        public static ItemDef item;
        private const float speedPerStackStatus = 20f;
        private const int maxStatusCountperItem = 6;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Viral Essence";
            if (!CheckItemEnabledConfig(itemName, "Red", configs))
            {
                return;
            }
            item = CreateItemDef(
                "VIRALESSENCE",
                itemName,
                "Increase speed per unique status effect.",
                "Increases <style=cIsUtility>movement speed</style> by <style=cIsUtility>20%</style> <style=cStack>(+20% per stack)</style> per <style=cIsDamage>unique status</style> you have." +
                    " Up to a maximum of <style=cIsUtility>6</style> <style=cStack>(+6 per stack)</style>.",
                "Illness",
                ItemTier.Tier3,
                UltAssets.ViralSmogSprite,
                UltAssets.ViralSmogPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility]
            );
        }

        protected override void Hooks()
        {
            // Perhaps I don't need this?
            //On.RoR2.CharacterBody.UpdateBuffs += CharacterBody_UpdateBuffs;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
        }

        //TODO add recalculate on get status effect?

        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                int grabCount = sender.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    //Log.Warning("Viral Smog Active");

                    // Get active buffs list and how many in that list is actually active (length)
                    int activeBuffLength = sender.activeBuffsListCount;
                    BuffIndex[] activeBuffs = sender.activeBuffsList;
                    //Log.Debug("Active Count Length: " + activeBuffLength);

                    int nonCooldownBuffs = activeBuffLength;
                    for (int i = 0; i < activeBuffLength; i++)
                    {
                        BuffDef buffDef = BuffCatalog.GetBuffDef(activeBuffs[i]);
                        if (buffDef.isCooldown)
                        {
                            nonCooldownBuffs--;
                            //Log.Debug(" ~ Cooldown: " + buffDef.name + "\tnew nonCool Count: " + nonCooldownBuffs);
                        }
                        else
                        {
                            //Log.Debug("Not Cool: " + buffDef.name);
                        }
                    }

                    nonCooldownBuffs = Math.Min(nonCooldownBuffs, maxStatusCountperItem * grabCount);

                    //Log.Debug("Viral Smog\nCount: " + nonCooldownBuffs + "\n"
                    //    + "Speed from Virus: " + speedPerStackStatus / 100f * nonCooldownBuffs * grabCount);
                    // Gives 20% speed per status per item
                    if (activeBuffLength > 0)
                    {
                        args.moveSpeedMultAdd += speedPerStackStatus / 100f * nonCooldownBuffs * grabCount;
                    }
                    else
                    {
                        //Log.Debug("Didn't apply speed buff");
                    }
                }
            }
        }
    }
}