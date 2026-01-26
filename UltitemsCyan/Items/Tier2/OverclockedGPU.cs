using RoR2;
using UnityEngine;
using BepInEx.Configuration;
using UnityEngine.Networking;

namespace UltitemsCyan.Items.Tier2
{
    // Possibly make it so that it gains 8% +2%perStack max of 32% + 40+
    // 3% +3% max of 10 stacks
    // 5% max of 6 stacks +6
    // 
    // But then would be exponential

    public class OverclockedGPU : ItemBase
    {
        public static ItemDef item;

        private const int maxOverclockedPerItem = 10;

        // For Overclocked Buff
        public const float buffAttackSpeedPerItem = 3.5f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Overclocked GPU";
            if (!CheckItemEnabledConfig(itemName, "Green", configs))
            {
                return;
            }
            item = CreateItemDef(
                "OVERCLOCKEDGPU",
                itemName,
                "Increase attack speed on kill. Stacks 10 times. Resets upon getting hurt.",
                "Killing an enemy increases <style=cIsDamage>attack speed</style> by <style=cIsDamage>3.5%</style>. Maximum cap of <style=cIsDamage>10</style> <style=cStack>(+10 per stack)</style> stacks. Lose stacks upon getting hit.",
                "GPU GPU",
                ItemTier.Tier2,
                UltAssets.OverclockedGPUSprite,
                UltAssets.OverclockedGPUPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Damage, ItemTag.OnKillEffect]
            );
        }


        protected override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        // Give Overclocked buff on kill
        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            //Log.Warning("Overclocking Killed");
            if (self && damageReport.attacker && damageReport.attackerBody && damageReport.attackerBody.inventory)
            {
                CharacterBody killer = damageReport.attackerBody;
                int grabCount = killer.inventory.GetItemCountEffective(item);
                int buffCount = killer.GetBuffCount(Buffs.OverclockedBuff.buff);
                // If body has the item and has fewer than the max stack then add buff
                if (grabCount > 0 && buffCount < maxOverclockedPerItem * grabCount) // maxOverclockedPerStack * grabCount
                {
                    // Don't have any buffs yet
                    if (buffCount == 0)
                    {
                        _ = Util.PlaySound("Play_item_goldgat_windup", killer.gameObject);
                    }
                    else
                    {
                        //Play_wCrit
                        _ = Util.PlaySound("Play_item_proc_crowbar", killer.gameObject);
                        _ = Util.PlaySound("Play_wDroneDeath", killer.gameObject);
                    }
                    killer.AddBuff(Buffs.OverclockedBuff.buff);
                }
            }
            // TODO check if goes in beginning or end
            orig(self, damageReport);
        }

        // Remove Overclocked buff when hit
        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            //Log.Warning("Overclocking hit");
            try
            {
                if (NetworkServer.active && self && victim && victim.GetComponent<CharacterBody>() && !damageInfo.rejected && damageInfo.damageType != DamageType.DoT)
                {
                    CharacterBody injured = victim.GetComponent<CharacterBody>();
                    if (injured.HasBuff(Buffs.OverclockedBuff.buff))
                    {
                        _ = Util.PlaySound("Play_item_goldgat_winddown", injured.gameObject);
                        injured.SetBuffCount(Buffs.OverclockedBuff.buff.buffIndex, 0);
                    }
                }
            }
            catch
            {
                //Log.Warning("Overloading GPU Hit Error?");
            }
        }
    }
}