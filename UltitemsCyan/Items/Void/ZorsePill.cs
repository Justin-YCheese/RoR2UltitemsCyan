using RoR2;
using R2API;
using System;
using UltitemsCyan.Buffs;
using UltitemsCyan.Items.Tier2;
using UnityEngine;
using BepInEx.Configuration;

//using static RoR2.DotController;

//using static RoR2.GenericPickupController;

namespace UltitemsCyan.Items.Void
{
    /* Notes:
     * 
     * Moves which deal zero damage will still trigger Zorse and deal a non zero amount of damage
     * Is great with Knockback fin because both multiply total damage dealt (but it's real hard to proc both)
     * 
     * Can spread with Noxious Thorns but doesn't transfer damage multiplier (only base damage transfered)
     *      Check: public void TriggerEnemyDebuffs(DamageReport damageReport)
     *             VineOrb::OnArrival | damageMultiplier = 1f
     * 
     * Use SendDamageDealt instead of DamageReport_ctor?
     * 
     */


    // TODO: check if Item classes needs to be public
    public class ZorsePill : ItemBase
    {
        public static ItemDef item;
        public static ItemDef transformItem;

        private const float percentPerStack = 20f;
        public const float duration = 3f; // Any greater than 3 and the health bar visual dissapears before inflicting damage

        public override void Init(ConfigFile configs)
        {
            const string itemName = "ZorsePill";
            if (!CheckItemEnabledConfig(itemName, "Void", configs))
            {
                return;
            }
            item = CreateItemDef(
                "ZORSEPILL",
                itemName,
                "Starve enemies on hit to deal delayed damage. <style=cIsVoid>Corrupts all HMTs</style>.",
                "Starve an enemy for <style=cIsDamage>20%</style> <style=cStack>(+20% per stack)</style> of TOTAL damage. Status duration <style=cIsDamage>resets</style> when reapplied. <style=cIsVoid>Corrupts all HMTs</style>.",
                "Get this diet pill now! Eat one and it cut's your weight down. Disclaimer: the microbes inside are definitly not eating you from the inside out.",
                ItemTier.VoidTier2,
                UltAssets.ZorsePillSprite,
                UltAssets.ZorsePillPrefab,
                [ItemTag.Damage],
                HMT.item
            );
        }

        protected override void Hooks()
        {
            //On.RoR2.DamageReport.ctor += DamageReport_ctor; // Gets TOTAL damage
            On.RoR2.HealthComponent.SendDamageDealt += HealthComponent_SendDamageDealt;
        }

        private void HealthComponent_SendDamageDealt(On.RoR2.HealthComponent.orig_SendDamageDealt orig, DamageReport damageReport)
        {
            //Log.Debug(" / / / / / Zorse start");
            orig(damageReport);
            //Log.Debug(" / / / / / Zorse mid");
            try
            {
                GameObject victimObject = damageReport.victimBody.gameObject;
                // If the victum has an inventory
                // and damage isn't rejected?
                if (victimObject && damageReport.attackerBody && damageReport.attackerBody.inventory &&
                    !damageReport.damageInfo.rejected && damageReport.damageInfo.damageType != DamageType.DoT &&
                    damageReport.damageDealt > 0)
                {
                    //Log.Debug(" / / / / / Zorse 1");
                    CharacterBody inflictor = damageReport.attackerBody;
                    int grabCount = inflictor.inventory.GetItemCountEffective(item);
                    if (grabCount > 0)
                    {
                        Log.Debug("  ...Starving enemy with reports...");
                        // If you have fewer than the max number of downloads, then grant buff

                        //float damageMultiplier = (basePercentHealth + (percentHealthPerStack * (grabCount - 1))) / 100f;
                        /*Log.Debug("Damage = " + dR.damageDealt + " | dR.info.damage: " + dR.damageInfo.damage
                            + " di.damage: " + dR.damageInfo.damage + " di.crit: " + dR.damageInfo.crit
                            + " multiplier: " + dR.damageInfo.damage / inflictor.damage
                                               * (dR.damageInfo.crit ? 2 : 1)
                                               * grabCount * percentPerStack / 100f);*/
                        /*Log.Debug("Damage = " + dR.damageDealt + " | dR.info.damage: " + dR.damageInfo.damage + " i.recalcDamage: " + inflictor.damageFromRecalculateStats
                            + " di.damage: " + dR.damageInfo.damage + " di.crit: " + dR.damageInfo.crit
                            + " multiplier: " + dR.damageInfo.damage / inflictor.damage * grabCount * percentPerStack / 100f);*/
                        InflictDotInfo inflictDotInfo = new()
                        {
                            victimObject = victimObject,
                            attackerObject = inflictor.gameObject,
                            //totalDamage = 0,
                            dotIndex = ZorseStarvingBuff.index,
                            duration = duration,
                            /*damageMultiplier = dR.damageDealt / inflictor.damageFromRecalculateStats
                                               * (dR.damageInfo.crit ? 2 : 1) * grabCount * percentPerStack / 100f,*/
                            /*damageMultiplier = dR.damageDealt / dR.damageInfo.damage
                                               * (dR.damageDealt / inflictor.damage)
                                               * grabCount * percentPerStack / 100f,*/
                            damageMultiplier = damageReport.damageInfo.damage / inflictor.damage
                                               * (damageReport.damageInfo.crit ? 2 : 1)
                                               * grabCount * percentPerStack / 100f,
                            hitHurtBox = null, // TODO change to something real?
                            maxStacksFromAttacker = null
                        };
                        DotController.InflictDot(ref inflictDotInfo);
                        //EffectManager.SimpleEffect(biteEffect, victim.transform.position, Quaternion.identity, true);
                        //EffectManager.SimpleEffect(biteEffect, victim.transform.position, Quaternion.identity, true);
                        //victim.GetComponent<CharacterBody>().AddTimedBuff();
                    }
                }
            }
            catch (NullReferenceException)
            {
                Log.Debug(" oh...  Zorse Pill had an expected null error");
            }
            //Log.Debug(" / / / / / Zorse end");
        }
    }
}
