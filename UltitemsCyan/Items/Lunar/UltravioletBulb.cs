using BepInEx.Configuration;
using R2API;
using RoR2;
using UnityEngine;

namespace UltitemsCyan.Items.Lunar
{

    // TODO: check if Item classes needs to be public
    public class UltravioletBulb : ItemBase
    {
        public static ItemDef item;
        private const float dontResetChance = 50f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Ultraviolet Bulb";
            if (!CheckItemEnabledConfig(itemName, "Lunar", configs))
            {
                return;
            }
            item = CreateItemDef(
                "ULTRAVIOLETBULB",
                itemName,
                "Chance to instantly reset a skill after it's used... <style=cDeath>BUT triples all cooldown</style>",
                "Have a <style=cIsUtility>50%</style> <style=cStack>(+50% per stack)</style> chance to <style=cIsUtility>reset a skill cooldown</style> and <style=cDeath>triple all cooldowns</style> <style=cStack>(per stack)</style>",
                "Voilent Stacks exponetially. Clover is like another bulb",
                ItemTier.Lunar,
                UltAssets.UltravioletBulbSprite,
                UltAssets.UltravioletBulbPrefab,
                [ItemTag.Utility]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.CharacterBody.OnSkillActivated += CharacterBody_OnSkillActivated;
            RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            On.RoR2.GenericSkill.CalculateFinalRechargeInterval += GenericSkill_CalculateFinalRechargeInterval;
        }

        private float GenericSkill_CalculateFinalRechargeInterval(On.RoR2.GenericSkill.orig_CalculateFinalRechargeInterval orig, GenericSkill self)
        {
            return self.baseRechargeInterval > 0 ? Mathf.Max(0.5f, self.baseRechargeInterval * self.cooldownScale - self.flatCooldownReduction) : 0;
        }

        protected void CharacterBody_OnSkillActivated(On.RoR2.CharacterBody.orig_OnSkillActivated orig, CharacterBody self, GenericSkill skill)
        {
            //Log.Warning("Faulty Bulb On Skill Activated...");
            if (self && self.inventory && skill && skill.skillDef.baseRechargeInterval > 0)
            {
                //Log.Debug("Cooldown remain: " + skill.cooldownRemaining + " Scale: " + skill.cooldownScale + " Base Interval: " + skill.skillDef.baseRechargeInterval + " Reset Cooldown?: " + skill.skillDef.resetCooldownTimerOnUse);
                int grabCount = self.inventory.GetItemCountEffective(item.itemIndex); // Change Luck
                if (grabCount > 0)
                {
                    grabCount += (int)self.master.luck;
                    //Log.Debug("garbCount: " + grabCount);
                    //Log.Debug("itemIndex: " + item.itemIndex);
                    float resetFraction = dontResetChance / 100;
                    float procChance = 100f;
                    for (int i = 0; i < grabCount; i++)
                    {
                        procChance *= resetFraction;
                    }
                    // fleaDropChance = 100 - dontResetChance ^ n
                    procChance = 100f - procChance;
                    //Log.Debug("fleaDropChance: " + fleaDropChance);
                    bool reset = Util.CheckRoll(procChance);
                    if (reset)
                    {
                        //Log.Debug("New Bulb Reseting for: " + self.GetUserName());
                        //skill.RestockContinuous(); // Doesn't do anything?
                        //skill.RestockSteplike();
                        skill.ApplyAmmoPack();
                        _ = Util.PlaySound("Play_mage_m2_zap", self.gameObject);
                        _ = Util.PlaySound("Play_mage_m2_zap", self.gameObject);
                        _ = Util.PlaySound("Play_item_proc_chain_lightning", self.gameObject);
                        _ = Util.PlaySound("Play_item_proc_chain_lightning", self.gameObject);
                        //Util.PlaySound("Play_item_proc_chain_lightning", self.gameObject);
                        //Util.PlaySound("Play_mage_m2_impact", self.gameObject);
                        _ = Util.PlaySound("Play_item_use_BFG_explode", self.gameObject);
                    }
                    else
                    {
                        //Log.Debug("Cooldowb Scale for: " + self.name);
                        //skill.cooldownScale = 2^grabCount;
                    }
                }
            }
            orig(self, skill);
        }
        private void RecalculateStatsAPI_GetStatCoefficients(CharacterBody sender, RecalculateStatsAPI.StatHookEventArgs args)
        {
            if (sender && sender.inventory)
            {
                int grabCount = sender.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    int increase = 1;
                    if (grabCount < 9) // Max cap on exponential 
                    {
                        for (int i = 0; i < grabCount; i++)
                        {
                            increase *= 3;
                        }
                    }
                    else
                    {
                        increase = 9999;
                    }
                    increase--;
                    //Log.Debug("New Bulb Cooldown Extend? " + (increase + 1));
                    args.allSkills.cooldownMultAdd += increase;
                }
            }
        }
    }
}