using RoR2;
using System;
using UltitemsCyan.Buffs;
using UltitemsCyan.Items.Tier2;
using UnityEngine;
using BepInEx.Configuration;

namespace UltitemsCyan.Items.Void
{


    // * * * ~ ~ ~ * * * ~ ~ ~ * * * Change to increase TOTAL DAMAGE * * * ~ ~ ~ * * * ~ ~ ~ * * * //
    // Change to be a cooldown instead? (like fin: so unaffected by luck and less dependeant on rapid fire attacks?)

    // TODO: check if Item classes needs to be public
    public class DownloadedRAM : ItemBase
    {
        public static ItemDef item;
        public static ItemDef transformItem;

        public const float downloadedBuffMultiplier = 8f;
        public const int downloadsPerItem = 4;

        private const float downloadChance = 12f;

        public const float notAttackingDelay = 4f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Downloaded RAM";
            if (!CheckItemEnabledConfig(itemName, "Void", configs))
            {
                return;
            }
            item = CreateItemDef(
                "DOWNLOADEDRAM",
                itemName,
                "Chance on hit to temporarily increase damage. Dealing damage refreshes the timer. <style=cIsVoid>Corrupts all Overclocked GPUs</style>.",
                "<style=cIsDamage>12%</style> chance on hit to increase damage by <style=cIsDamage>8%</style>. Maxinum cap of <style=cIsDamage>4</style> <style=cStack>(+4 per stack)</style>. Lose stacks 4 seconds after not inflicting damage. <style=cIsVoid>Corrupts all Overclocked GPUs</style>.",
                "Wow I can't belive it worked! I thought for sure it was a scam!",
                ItemTier.VoidTier2,
                UltAssets.DownloadedRAMSprite,
                UltAssets.DownloadedRAMPrefab,
                [ItemTag.Damage],
                OverclockedGPU.item
            );
        }

        protected override void Hooks()
        {
            //On.RoR2.CharacterBody.OnOutOfDangerChanged += CharacterBody_OnOutOfDangerChanged;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.inventory)
            {
                _ = self.AddItemBehavior<DownloadedVoidBehavior>(self.inventory.GetItemCountEffective(item));
            }
        }

        /*/
        private void CharacterBody_OnOutOfDangerChanged(On.RoR2.CharacterBody.orig_OnOutOfDangerChanged orig, CharacterBody self)
        {
            Log.Warning(" ! Combat Changed ! ");
            try
            {
                if (self && self.outOfCombat)
                {
                    Log.Debug(self.name + "  ...Leaving Combat");
                    self.SetBuffCount(DownloadedBuff.buff.buffIndex, 0);
                }
                else
                {
                    Log.Debug(self.name + " is Entering Combat ! ! !");
                }
            }
            catch (NullReferenceException)
            {
                Log.Debug("Who Downloaded?");
                Log.Debug("Name: " + self.name);
            }
        }
        //*/

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            try
            {
                // If the victum has an inventory
                // and damage isn't rejected?
                if (self && victim && damageInfo.attacker.GetComponent<CharacterBody>() && damageInfo.attacker.GetComponent<CharacterBody>().inventory && !damageInfo.rejected && damageInfo.damageType != DamageType.DoT)
                {
                    CharacterBody inflictor = damageInfo.attacker.GetComponent<CharacterBody>();
                    int grabCount = inflictor.inventory.GetItemCountEffective(item);
                    if (grabCount > 0)
                    {
                        //Log.Warning("RAM Download ! ! ! Def no Viris");
                        //Log.Debug("Is Crit?: " + damageInfo.crit + " Is out of combat? " + inflictor.outOfCombat);

                        //   *   *   *   ADD EFFECT   *   *   *   //

                        DownloadedVoidBehavior behavior = inflictor.GetComponent<DownloadedVoidBehavior>();
                        // Check if behavior valid?
                        behavior.enabled = true;
                        behavior.UpdateStopwatch(Run.instance.time);
                        if (Util.CheckRoll(downloadChance, inflictor.master.luck))
                        {
                            //Log.Debug("downloading");
                            // If you have fewer than the max number of downloads, then grant buff
                            if (inflictor.GetBuffCount(DownloadedBuff.buff) < grabCount * downloadsPerItem)
                            {
                                inflictor.AddBuff(DownloadedBuff.buff);
                            }
                        }
                    }
                }
            }
            catch (NullReferenceException)
            {
                // If error here then elite errors
                //Log.Warning("???What hit Downloading?");
                //Log.Debug("Attacker: " + damageInfo.attacker.GetComponent<CharacterBody>().name);
                //Log.Debug("Victum " + victim.name);
                //Log.Debug("CharacterBody " + victim.GetComponent<CharacterBody>().name);
                //Log.Debug("Inventory " + victim.GetComponent<CharacterBody>().inventory);
                //Log.Debug("Damage rejected? " + damageInfo.rejected);
            }
        }

        //
        public class DownloadedVoidBehavior : CharacterBody.ItemBehavior
        {
            //public const float notAttackingDelay = DownloadedRAM.notAttackingDelay;
            public float attackingStopwatch = 0;
            private bool _attacking = false;

            // Order:
            // Awake(), Enable(), OnStart()
            // Disable(), Destory()

            public void UpdateStopwatch(float newTime)
            {
                //Log.Debug("New attack at " + newTime);
                attackingStopwatch = newTime;
            }

            public bool DealingDamage
            {
                get { return _attacking; }
                set
                {
                    if (_attacking != value)
                    {
                        _attacking = value;
                        //Log.Warning(body.name + " attack ram toggeled!: " + _attacking);
                        // If not attacking
                        if (!_attacking)
                        {
                            //   *   *   *   REMOVE EFFECT   *   *   *   //

                            body.SetBuffCount(DownloadedBuff.buff.buffIndex, 0);
                            enabled = false;
                        }
                    }
                }
            }

#pragma warning disable IDE0051 // Remove unused private members
            private void OnAwake()
            {
                enabled = false;
            }

            private void OnDisable()
            {
                attackingStopwatch = 0;
                DealingDamage = false;
            }

            private void FixedUpdate()
            {
                // If too much time has passed since last dealing damage
                //Log.Debug("RAM Times: " + attackingStopwatch);
                DealingDamage = Run.instance.time <= attackingStopwatch + notAttackingDelay;
            }
        }
        ///
    }
}
