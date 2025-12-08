using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using BepInEx.Configuration;

namespace UltitemsCyan.Items.Tier2
{

    // TODO: Make better sound and visuals
    public class HMT : ItemBase
    {
        public static ItemDef item;

        public GameObject onHitAttacker;

        public const float igniteChance = 10f;

        public const float baseBurnDuration = 6f;
        public const float durationPerItem = 2f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "H.M.T";
            if (!CheckItemEnabledConfig(itemName, "Green", configs))
            {
                return;
            }
            item = CreateItemDef(
                "HMT",
                itemName,
                "Chance to ignite enemies when you inflict something else.",
                "<style=cIsDamage>10%</style> <style=cStack>(+10% per stack)</style> chance to ignite enemies when inflicting something other than burning. Enemies burn for <style=cIsDamage>200%</style> <style=cStack>(+100% per stack)</style> base damage.",
                "Fiery Compilation\r\nSizzling Playlist\r\nBlazing Tracklist\r\nScorching Mix\r\nTorrid Tunes\r\nSweltering Sounds\r\nBoiling Beats\r\nBurning Medley\r\nHeated Harmony\r\nIncandescent Melodies\r\nFlaming Rhythms\r\nSultry Selections\r\nPiping Hot Hits\r\nFervent Fusion\r\nArdent Anthology\r\nWarm Jams\r\nHot-blooded Mixdown\r\nThermal Tracks\r\nCaliente Collection\r\nFeverish Features\r\nToasty Tapes\r\nIgnited Arrangement\r\nGlowing Grooves\r\nLava-like Lineup\r\nSmoldering Series\r\nSteamy Set\r\nInfernal Playlist\r\nRadiant Recordings\r\nBlistering Bangers\r\nSearing Serenade",
                ItemTier.Tier2,
                UltAssets.HMTSprite,
                UltAssets.HMTPrefab,
                [ItemTag.Damage]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.HealthComponent.TakeDamage += HealthComponent_TakeDamage;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            //On.RoR2.CharacterBody.AddBuff_BuffIndex += CharacterBody_AddBuff_BuffIndex;
            On.RoR2.CharacterBody.AddTimedBuff_BuffDef_float += CharacterBody_AddTimedBuff;
            On.RoR2.DotController.InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 += DotController_InflictDot_GameObject;
        }

        private void HealthComponent_TakeDamage(On.RoR2.HealthComponent.orig_TakeDamage orig, HealthComponent self, DamageInfo damageInfo)
        {
            //Log.Debug("HMT Health Take Damage!");
            if (damageInfo.attacker)
            {
                onHitAttacker = damageInfo.attacker;
            }

            orig(self, damageInfo);

            onHitAttacker = null;
            //Log.Debug(" ! HMT Health Off Damage...");
        }

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            //Log.Debug("On Hit! HMT");
            if (damageInfo.attacker)
            {
                if (onHitAttacker && onHitAttacker != damageInfo.attacker)
                {
                    Log.Warning("Health Component attacker and OnHitEnemy attacker are different! Assumption was wrong...   " + onHitAttacker.name + " | " + damageInfo.attacker.name);
                }
                onHitAttacker = damageInfo.attacker;
            }

            orig(self, damageInfo, victim);

            onHitAttacker = null;
            //Log.Debug(" ! HMT Off Hit...");
        }


        private void CharacterBody_AddTimedBuff(On.RoR2.CharacterBody.orig_AddTimedBuff_BuffDef_float orig, CharacterBody self, BuffDef buffDef, float duration)
        {
            orig(self, buffDef, duration);
            // Use external variables to see if was from either TakeDamage or OnHitEnemy and not something else
            if (self && onHitAttacker && buffDef.isDebuff)
            {
                //Log.Debug("Yes ! onhitAttacker !");
                CharacterBody inflictor = onHitAttacker.GetComponent<CharacterBody>();
                if (inflictor && inflictor.inventory && inflictor.master && self.gameObject)
                {
                    int grabCount = inflictor.inventory.GetItemCountEffective(item);
                    // Have item and got chance
                    if (grabCount > 0 && Util.CheckRoll(igniteChance * grabCount, inflictor.master.luck))
                    {
                        // Inflict Burn!
                        InflictBurn(self.gameObject, onHitAttacker, inflictor.inventory, grabCount);
                    }
                }
            }
        }

        // When Inflicting something other than Burn
        private void DotController_InflictDot_GameObject(On.RoR2.DotController.orig_InflictDot_GameObject_GameObject_DotIndex_float_float_Nullable1 orig, GameObject victimObject, GameObject attackerObject, DotController.DotIndex dotIndex, float duration, float damageMultiplier, uint? maxStacksFromAttacker)
        {
            orig(victimObject, attackerObject, dotIndex, duration, damageMultiplier, maxStacksFromAttacker);
            //Log.Debug(" * * * DotController HMT Inflicting burn?");
            if (victimObject && attackerObject
                && dotIndex != DotController.DotIndex.Burn && dotIndex != DotController.DotIndex.Helfire && dotIndex != DotController.DotIndex.StrongerBurn
                     && NetworkServer.active)
            {
                CharacterBody body = attackerObject.GetComponent<CharacterBody>();
                int grabCount = body.inventory.GetItemCountEffective(item);
                // Have item and got chance
                Log.Debug("HMT check roll: " + igniteChance * grabCount + "% chance");
                if (grabCount > 0 && Util.CheckRoll(igniteChance * grabCount, body.master.luck))
                {
                    // Inflict Burn!
                    InflictBurn(victimObject, attackerObject, body.inventory, grabCount);
                }
            }
        }

        private void InflictBurn(GameObject victimObject, GameObject attackerObject, Inventory inventory, int grabCount)
        {
            Log.Debug("Hot Burns! HMT");
            InflictDotInfo inflictDotInfo = new()
            {
                victimObject = victimObject,
                attackerObject = attackerObject,
                //totalDamage = 0,
                dotIndex = DotController.DotIndex.Burn,
                duration = baseBurnDuration + durationPerItem * (grabCount - 1),
                damageMultiplier = 1,
                maxStacksFromAttacker = null
            };
            StrengthenBurnUtils.CheckDotForUpgrade(inventory, ref inflictDotInfo);
            DotController.InflictDot(ref inflictDotInfo);
        }
    }
}