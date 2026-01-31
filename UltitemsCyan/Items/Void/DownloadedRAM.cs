using BepInEx.Configuration;
using RoR2;
using System;
using UltitemsCyan.Buffs;
using UltitemsCyan.Items.Tier2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UltitemsCyan.Items.Void
{


    // * * * ~ ~ ~ * * * ~ ~ ~ * * * Change to increase TOTAL DAMAGE * * * ~ ~ ~ * * * ~ ~ ~ * * * //
    // Change to be a cooldown instead? (like fin: so unaffected by luck and less dependeant on rapid fire attacks?)

    // TODO: check if Item classes needs to be public
    public class DownloadedRAM : ItemBase
    {

        //"RoR2/Base/Imp/ImpDeathEffect.prefab" (like a dark puddle)
        public readonly GameObject ImpEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/EliteVoid/VoidInfestorLeapEffect.prefab").WaitForCompletion();

        public static ItemDef item;
        public static ItemDef transformItem;

        public const float downloadedBuffMultiplier = 20f;
        public const int downloadsPerItem = 4;

        public const float plugeSpeed = 8f;
        public const float plugeSpeedDivisor = 2f;

        private const float downloadChance = 12f;

        public const float notAttackingDelay = 4f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Downloaded RAM";
            if (!CheckItemEnabledConfig(itemName, "Void", configs))
            {
                return;
            }
            //<style=cIsUtility>plunge</style> enemies and 
            item = CreateItemDef(
                "DOWNLOADEDRAM",
                itemName,
                "Chance on hit to temporarily increase damage. Dealing damage refreshes the timer. <style=cIsVoid>Corrupts all Overclocked GPUs</style>.",
                "<style=cIsDamage>12%</style> chance on hit to increase your damage by <style=cIsDamage>8%</style>, up to <style=cIsDamage>4</style> <style=cStack>(+4 per stack)</style>, for 4s. Dealing damage refreshes the timer. <style=cIsVoid>Corrupts all Overclocked GPUs</style>.",
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

        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            try
            {
                // If the victum has an inventory
                // and damage isn't rejected?
                if (NetworkServer.active && self && victim && damageInfo.attacker.GetComponent<CharacterBody>() && damageInfo.attacker.GetComponent<CharacterBody>().inventory && !damageInfo.rejected && damageInfo.damageType != DamageType.DoT)
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

                            // Launch victim downwards
                            CharacterBody victimBody = victim.GetComponent<CharacterBody>();
                            //victimBody.characterMotor.ModifyGravity()
                            if (victimBody != null && victimBody.characterMotor)
                            {
                                //ref Vector3 refVelocity = ref victimBody.characterMotor.velocity;
                                //Log.Debug("Vertical Velocity: initial " + refVelocity.y);
                                //if (refVelocity.y >= 0f)
                                //{
                                //    refVelocity.y /= plugeSpeedDivisor;
                                //}
                                //refVelocity.x /= plugeSpeedDivisor;
                                //refVelocity.z /= plugeSpeedDivisor;

                                //refVelocity.y -= plugeSpeed;

                                //Log.Debug("                       new " + refVelocity.y);
                            }

                            EffectManager.SpawnEffect(ImpEffect, new EffectData
                            {
                                origin = victimBody.corePosition,
                                //color = new Color(0.2392f, 0.8196f, 0.917647f) // Cyan Lunar color
                            }, true);



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
