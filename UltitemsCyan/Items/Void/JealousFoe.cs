using RoR2;
using RoR2.Projectile;
using UltitemsCyan.Items.Tier1;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using BepInEx.Configuration;
using UltitemsCyan.Buffs;

namespace UltitemsCyan.Items.Void
{

    // TODO: check if Item classes needs to be public
    public class JealousFoe : ItemBase
    {
        public static ItemDef item;
        public static ItemDef transformItem;

        public const float collectTime = 6f;
        public const float consumeBaseTime = 4f;
        //public const float consumeMinTime = 0f;
        public const float cooldownTime = 6f;

        public const float maxBuffsPerStack = 3f;

        public static readonly GameObject fmpEffectPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DeathProjectile/DeathProjectileTickEffect.prefab").WaitForCompletion();
        public static readonly GameObject fmpPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/DeathProjectile/DeathProjectile.prefab").WaitForCompletion();

        public enum EyePhase
        {
            collecting,
            consuming,
            cooldown
        }

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Jealous Foe";
            if (!CheckItemEnabledConfig(itemName, "Void", configs))
            {
                return;
            }
            item = CreateItemDef(
                "JEALOUSFOE",
                itemName,
                "Trigger On-Kill effects after grabbing pickups. <style=cIsVoid>Corrupts all Toy Robots</style>.",
                "Gain stacks upon grabbing pickups</style>. Maximum cap of 3</style> (+3 per stack)</style>. Trigger an <style=cIsDamage>On-Kill</style> effect per stack every <style=cIsUtility>4s</style> (-30% per stack)</style>. Recharges every <style=cIsUtility>6</style> seconds. <style=cIsVoid>Corrupts all Toy Robots</style>.",
                //"<style=cIsDamage>5%</style> <style=cStack>(+5% per stack)</style> chance of triggering <style=cIsDamage>On-Kill</style> effects when <style=cIsDamage>grabbing pickups</style>. <style=cIsVoid>Corrupts all Toy Robots</style>.",
                "Look at it Jubilat. It just jubilant like some jealous jello jelly.",
                ItemTier.VoidTier1,
                UltAssets.JubilantFoeSprite,
                UltAssets.JubilantFoePrefab,
                [ItemTag.Utility, ItemTag.OnKillEffect],
                ToyRobot.item
            );
        }

        protected override void Hooks()
        {
            On.RoR2.HealthPickup.OnTriggerStay += HealthPickup_OnTriggerStay;
            On.RoR2.ElusiveAntlersPickup.OnTriggerStay += ElusiveAntlersPickup_OnTriggerStay;
            On.RoR2.AmmoPickup.OnTriggerStay += AmmoPickup_OnTriggerStay;
            On.RoR2.BuffPickup.OnTriggerStay += BuffPickup_OnTriggerStay;
            On.RoR2.MoneyPickup.OnTriggerStay += MoneyPickup_OnTriggerStay;
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
        }

        private void HealthPickup_OnTriggerStay(On.RoR2.HealthPickup.orig_OnTriggerStay orig, HealthPickup self, Collider other)
        {
            orig(self, other);
            GotPickup(other);
        }

        private void ElusiveAntlersPickup_OnTriggerStay(On.RoR2.ElusiveAntlersPickup.orig_OnTriggerStay orig, ElusiveAntlersPickup self, Collider other)
        {
            orig(self, other);
            GotPickup(other);
        }

        private void AmmoPickup_OnTriggerStay(On.RoR2.AmmoPickup.orig_OnTriggerStay orig, AmmoPickup self, Collider other)
        {
            orig(self, other);
            GotPickup(other);
        }

        private void BuffPickup_OnTriggerStay(On.RoR2.BuffPickup.orig_OnTriggerStay orig, BuffPickup self, Collider other)
        {
            orig(self, other);
            GotPickup(other);
        }

        private void MoneyPickup_OnTriggerStay(On.RoR2.MoneyPickup.orig_OnTriggerStay orig, MoneyPickup self, Collider other)
        {
            orig(self, other);
            GotPickup(other);
        }

        private void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.inventory)
            {
                _ = self.AddItemBehavior<JealousFoeBehaviour>(self.inventory.GetItemCountEffective(item));
            }
        }

        private void GotPickup(Collider other)
        {
            //Log.Debug("Foe Got Pickup?");
            CharacterBody body = other.GetComponent<CharacterBody>();
            if (body && body.inventory)
            {
                int grabCount = body.inventory.GetItemCountEffective(item);
                if (grabCount > 0 && NetworkServer.active)// && Util.CheckRoll(chancePerStack * grabCount, body.master.luck)
                {
                    body.GetComponent<JealousFoeBehaviour>().GotPickup();
                }
            }
        }

        //Play_affix_void_bug_infect (awakening)
        //Play_voidman_idle_twitch (activation kills)
        //Play_UI_arenaMode_voidCollapse_select (collecting)
        //Play_voidDevastator_spawn_open (collecting ready)
        public class JealousFoeBehaviour : CharacterBody.ItemBehavior
        {
            private EyePhase _currentPhase = EyePhase.collecting;
            public float eyePhaseStopwatch = float.PositiveInfinity;
            public float currentTimer = collectTime;
            private GameObject FakeFoe;

            // Remove Sleepy cooldown
            public void SetCollectingPhase()
            {
                if (_currentPhase == EyePhase.cooldown)
                {
                    Log.Debug(" ! ! ! ! ! ! Phase Set ! ! Coll Lecting");
                    // There should already be no Cooldown Buff
                    _currentPhase = EyePhase.collecting;
                    //_ = Util.PlaySound("Play_UI_arenaMode_voidCollapse_select", body.gameObject);
                }
            }

            // Convert Drowsy buffs to Awake buffs
            public void SetConsumingPhase()
            {
                if (_currentPhase == EyePhase.collecting)
                {
                    Log.Debug(" ! ! ! ! ! ! Phase Set ! ! conSUME");
                    _currentPhase = EyePhase.consuming;
                    // Convert Drowsy buffs to awake buffs
                    int buffCount = body.GetBuffCount(EyeDrowsyBuff.buff);
                    body.SetBuffCount(EyeDrowsyBuff.buff.buffIndex, 0);
                    body.SetBuffCount(EyeAwakeBuff.buff.buffIndex, buffCount);

                    _ = Util.PlaySound("Play_affix_void_bug_infect", body.gameObject);
                }
            }

            // Remove Awake buffs and add Sleepy cooldown buff
            public void SetCooldownPhase()
            {
                if (_currentPhase == EyePhase.consuming)
                {
                    Log.Debug(" ! ! ! ! ! ! Phase Set ! ! c o o l down");
                    // There should already be no Awake Buffs
                    _currentPhase = EyePhase.cooldown;
                    eyePhaseStopwatch = float.PositiveInfinity;
                    body.AddTimedBuff(EyeSleepyBuff.buff, cooldownTime);
                }
            }

            public void GotPickup()
            {
                if (_currentPhase == EyePhase.collecting)
                {
                    // If timer hasn't started
                    if (eyePhaseStopwatch == float.PositiveInfinity)
                    {
                        eyePhaseStopwatch = Run.instance.time;
                        currentTimer = collectTime;
                        //Log.Debug(" | | | TIME | | | Collecting ! New timer is " + currentTimer);
                        // next is CheckTimer
                    }
                    // Give buff if below max
                    if (body.GetBuffCount(EyeDrowsyBuff.buff) < stack * maxBuffsPerStack)
                    {
                        body.AddBuff(EyeDrowsyBuff.buff);
                        EffectData effectData = new()
                        {
                            origin = body.corePosition,
                            rotation = Quaternion.identity
                        };
                        EffectManager.SpawnEffect(fmpEffectPrefab, effectData, false);
                        _ = Util.PlaySound("Play_UI_arenaMode_voidCollapse_select", body.gameObject);

                    }
                }
            }

            // If player is at full health
            public void FixedUpdate()
            {
                // If enough time passed...
                //Log.Debug(" | | | TIME | | | " + _currentPhase + " : " + Run.instance.time + " > " + eyePhaseStopwatch + " + " + currentTimer);
                if (Run.instance.time > eyePhaseStopwatch + currentTimer)
                {
                    CheckTimer();
                }
            }

            // If timer in collecting then switch to consuming
            // If timer in consuming then consume another stack
            // (Awake Buff: stack empty then switch to cooldown)
            // (Sleepy Buff: switch to collecting when stack is lost)
            private void CheckTimer()
            {
                //Log.Debug(" | | | TIME | | | check run: " + Run.instance.time);
                // ...while in collecting then switch to consuming
                if (_currentPhase == EyePhase.collecting)
                {
                    eyePhaseStopwatch = Run.instance.time;
                    // Quadratic equation with mininum
                    // t * 2 / (n + 1)
                    currentTimer = consumeBaseTime * 2 / (stack + 1);
                    //Log.Debug(" | | | TIME | | | Eating! New timer is " + currentTimer);

                    SetConsumingPhase();

                    // An instant activation
                    body.RemoveBuff(EyeAwakeBuff.buff);
                    ActivateDeath();
                }
                // ...while in consuming then consume
                else if (_currentPhase == EyePhase.consuming)
                {
                    eyePhaseStopwatch = Run.instance.time;

                    // Consume buff and trigger onKillEffects
                    body.RemoveBuff(EyeAwakeBuff.buff);
                    ActivateDeath();
                    // next is Awake LastStackRemoved


                    /*// If no stacks left switch to cooldown
                    if (body.GetBuffCount(EyeAwakeBuff.buff) == 0)
                    {
                        // currentTimer doesn't need to account for Cooldown duration because can use buff timer for that

                        //Log.Debug(" | | | TIME | | | Cooldown! New timer is " + currentTimer);
                        SetCooldownPhase();
                    }
                    //*/
                }
                else
                {
                    Log.Warning("##########  Uh oh, Jealous Foe is not supposed to be here...");
                }
            }

            private void ActivateDeath()
            {
                if (!FakeFoe)
                {
                    FakeFoe = Instantiate(fmpPrefab, body.footPosition, Quaternion.identity);
                    FakeFoe.transform.localScale = new Vector3(0f, 0f, 0f);
                    Destroy(FakeFoe.GetComponent<DestroyOnTimer>());
                    Destroy(FakeFoe.GetComponent<DeathProjectile>());
                    Destroy(FakeFoe.GetComponent<ApplyTorqueOnStart>());
                    Destroy(FakeFoe.GetComponent<ProjectileDeployToOwner>());
                    Destroy(FakeFoe.GetComponent<Deployable>());
                    Destroy(FakeFoe.GetComponent<ProjectileStickOnImpact>());
                    Destroy(FakeFoe.GetComponent<ProjectileController>());
                }
                FakeFoe.transform.position = body.footPosition; // Not needed?

                HealthComponent health = FakeFoe.GetComponent<HealthComponent>();

                DamageInfo damageInfo = new()
                {
                    attacker = body.gameObject,
                    crit = body.RollCrit(),
                    damage = body.baseDamage,
                    position = body.footPosition,
                    procCoefficient = 0f,
                    damageType = DamageType.Generic,
                    damageColorIndex = DamageColorIndex.Item
                };
                DamageReport damageReport = new(damageInfo, health, damageInfo.damage, health.combinedHealth);
                //GlobalEventManager.instance.OnCharacterDeath(val3);
                GlobalEventManager.instance.OnCharacterDeath(damageReport);

                EffectData effectData = new()
                {
                    origin = body.corePosition,
                    rotation = Quaternion.identity
                };
                EffectManager.SpawnEffect(fmpEffectPrefab, effectData, false);

                _ = Util.PlaySound("Play_voidman_idle_twitch", body.gameObject);
            }

            public void OnAwake()
            {
            }

            public void OnDisable()
            {
                //Log.Debug("Take my Eye!   -   or you know just disable it");
                if (body)
                {
                    body.SetBuffCount(EyeDrowsyBuff.buff.buffIndex, 0);
                    body.SetBuffCount(EyeAwakeBuff.buff.buffIndex, 0);
                    body.ClearTimedBuffs(EyeSleepyBuff.buff.buffIndex);
                }
            }

            public void OnDestroy()
            {
                //Log.Debug("Take my Eye! set it aside!");
                Destroy(FakeFoe);
            }
        }
    }
}