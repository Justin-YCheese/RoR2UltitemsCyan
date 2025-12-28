using RoR2;
using UltitemsCyan.Items.Tier3;
using UnityEngine;

namespace UltitemsCyan.Buffs
{
    public class SporkBleedBuff : BuffBase
    {
        public static BuffDef buff;
        //private const float bleedChance = PigsSpork.sporkBleedChancePerItem;

        public override void Init()
        {
            buff = DefineBuff("Spork Bleed Buff", false, false, UltAssets.SporkBleedSprite);
            //Log.Info(buff.name + " Initialized");
            Hooks();
        }

        protected void Hooks()
        {
            //RecalculateStatsAPI.GetStatCoefficients += RecalculateStatsAPI_GetStatCoefficients;
            //On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            //On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
        }

        /*/
        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);
            if (self && self.HasBuff(buff))
            {
                //Log.Debug("Orig Bleed Chance: " + self.bleedChance);
                int grabCount = self.inventory.GetItemCountEffective(PigsSpork.item);
                float lostHealthPercent = Mathf.Max(self.healthComponent.combinedHealthFraction, 1f);
                self.bleedChance += bleedChance * grabCount * lostHealthPercent;
                //Log.Debug("New Bleed Chance: " + self.bleedChance);
                //Debug.Log(sender.name + "Birthday modifier: " + (rottingBuffMultiplier / 100f * buffCount));
            }
        }
        //*/

        /*/
        public static void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            // If no attacker then skip
            // ? if (di == null || self.body == null || di.rejected || !di.attacker || di.inflictor == null || di.attacker == self.gameObject) ?
            // Bugged Code, if don't go to orig(self, damageInfo) after finding
            if (damageInfo.attacker)
            {
                CharacterBody attacker = damageInfo.attacker.GetComponent<CharacterBody>();
                int buffCount = attacker.GetBuffCount(buff);

                if (buffCount > 0)
                {
                    //Log.Debug("Birthday Candles Buffs: " + buffCount);
                    //Log.Debug("damage:      \t" + damageInfo.damage);
                    damageInfo.damage *= 1 + birthdayBuffBaseMultiplier + (rottingBuffMultiplier * buffCount);
                    //Log.Debug("damage after:\t" + damageInfo.damage);
                }
            }
            // Has to be after damage changed to update damage
            orig(self, damageInfo);
        }//*/
    }
}