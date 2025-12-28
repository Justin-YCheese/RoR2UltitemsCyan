using RoR2;
using UnityEngine;
using System.Collections.Generic;
using UltitemsCyan.Buffs;
using BepInEx.Configuration;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;

namespace UltitemsCyan.Items.Tier3
{
    // Notes
    // Unless explosion everytime hit below 25%
    public class PigsSpork : ItemBase
    {
        public static ItemDef item;

        public const float sporkBleedChancePerItem = 100f;
        public const float sporkBaseDuration = 15f;

        private static readonly GameObject willOWisp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ExplodeOnDeath/WilloWispDelay.prefab").WaitForCompletion();
        private static readonly GameObject sporkBlastEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Explosion.prefab").WaitForCompletion();

        public const float sporkHemmerageBlastRadius = 32f;
        public const float sporkBaseBleedBlastRadius = 4f; // With 1 it is 8 meters total
        public const float sporkStackBleedBlastRadius = 4f;

        //public const float sporkDurationPerStack = 5f;
        //private const float bleedHealing = 3;

        public override void Init(ConfigFile configs)
        {
            if (!CheckItemEnabledConfig("Pigs Spork", "Red", configs)) // Can't have apostrophes
            {
                return;
            }
            item = CreateItemDef(
                "PIGSSPORK",
                "Pig's Spork",
                "Inflict bleed equal to missing health. When hit at low health explode and inflicting bleed will splash.",
                // Applying bleed will make the enemy splash bleed on adjacent enemies when you have the 200% bleed buff
                //"Bleed damage <style=cIsHealing>heals</style> for <style=cIsHealing>3</style> <style=cStack>(+3 per stack)</style> <style=cIsHealing>health</style>. Taking damage below <style=cIsHealth>25% health</style> will <style=cIsHealth>hemorrhage</style> all enemies within <style=cIsDamage>32m</style> <style=cStack>(+32m per stack)</style> and grant <style=cIsDamage>200%</style> chance to <style=cIsDamage>bleed</style> for <style=cIsDamage>12s</style> <style=cStack>(+12 per stack)</style>.",
                "Chance to inflict bleed equal to <style=cIsDamage>100%</style> <style=cStack>(+100% per stack)</style> of <style=cIsHealth>maximum health percetage missing</style>. Taking damage below <style=cIsHealth>25% health</style> will <style=cIsHealth>hemorrhage</style> all enemies within <style=cIsDamage>32m</style> <style=cStack>(+32m per stack)</style> and make <style=cIsDamage>bleed splash</style> enemies within <style=cIsDamage>8m</style> <style=cStack>(+4m per stack)</style> for <style=cIsDamage>15s</style>.",
                "There once was a pet named porky\nA cute and chubby pig\n\nBut the farmer broke his fork\nAnd used the spoon to dig\n\nSo he made a Sporky Spig\n",
                ItemTier.Tier3,
                UltAssets.PigsSporkSprite,
                UltAssets.PigsSporkPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Damage, ItemTag.LowHealth]
            );
        }

        protected override void Hooks()
        {
            // Heal on tick of bleed
            //On.RoR2.DotController.EvaluateDotStacksForType += DotController_EvaluateDotStacksForType;
            // Add Item heal on bleed behavior to target on bleed inflicted
            //On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
            // Remove heal from victim
            //On.RoR2.DotController.OnDotStackRemovedServer += DotController_OnDotStackRemovedServer;

            // Dirty Bit if health changes
            On.RoR2.CharacterBody.OnInventoryChanged += CharacterBody_OnInventoryChanged;
            // Add bleed chance
            On.RoR2.CharacterBody.RecalculateStats += CharacterBody_RecalculateStats;
            // Inflict splash check
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
            // Initial Explosion and buff when at low health
            On.RoR2.HealthComponent.UpdateLastHitTime += HealthComponent_UpdateLastHitTime;
        }

        protected void CharacterBody_OnInventoryChanged(On.RoR2.CharacterBody.orig_OnInventoryChanged orig, CharacterBody self)
        {
            orig(self);
            if (self && self.inventory)
            {
                _ = self.AddItemBehavior<DreamFuelBehaviour>(self.inventory.GetItemCountEffective(item));
            }
        }//*/

        // Speed at full health
        public class DreamFuelBehaviour : CharacterBody.ItemBehavior
        {
            public HealthComponent healthComponent;
            private float _currentHealth = 0;
            public float CurrentHealth
            {
                get { return _currentHealth; }
                set
                {
                    // If not already the same value
                    if (_currentHealth != value)
                    {
                        _currentHealth = value;

                        body.SetDirtyBit(1U);

                        Log.Debug("Spokr's Current Health: " + _currentHealth);

                        //_ = Util.PlaySound("Play_affix_void_bug_spawn", gameObject);
                    }
                }
            }

            // If player is at full health
            public void FixedUpdate()
            {
                if (healthComponent)
                {
                    CurrentHealth = healthComponent.combinedHealthFraction;
                }
            }

            public void Start()
            {
                healthComponent = GetComponent<HealthComponent>();
            }

            public void OnDestroy()
            {
                CurrentHealth = 0;
            }
        }

        private void CharacterBody_RecalculateStats(On.RoR2.CharacterBody.orig_RecalculateStats orig, CharacterBody self)
        {
            orig(self);

            if (self && self.inventory)
            {
                int grabCount = self.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    float lostHealthPercent = Mathf.Clamp(1f - self.healthComponent.combinedHealthFraction, 0f, 1f);
                    self.bleedChance += sporkBleedChancePerItem * grabCount * lostHealthPercent;
                    Log.Debug("New Bleed Chance: " + self.bleedChance);
                }
            }
        }

        // Initial Explosion and buff when at low health
        private void HealthComponent_UpdateLastHitTime(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker, bool delayedDamage, bool firstHitOfDelayedDamage)
        {
            if (NetworkServer.active && self && self.body && self.body.inventory)
            {
                CharacterBody body = self.body;
                int grabCount = body.inventory.GetItemCountEffective(item);
                if (grabCount > 0 && self.isHealthLow) // && !body.HasBuff(SporkBleedBuff.buff.buffIndex)
                {
                    // Bleed Blast
                    GameObject explostionObject = Object.Instantiate(willOWisp, body.corePosition, Quaternion.identity);
                    DelayBlast blast = explostionObject.GetComponent<DelayBlast>();
                    //GameObject FakePlayer = body.gameObject.InstantiateClone("Fake Player");

                    //UnityEngine.Object.Destroy(FakePlayer.GetComponent<CharacterBody>());
                    blast.position = body.corePosition;
                    blast.attacker = body.gameObject; // Making this attacker makes self damage not inflict enemies
                    blast.inflictor = body.gameObject;
                    blast.baseDamage = body.damage;
                    blast.baseForce = 1300f;
                    //blast.bonusForce = ;
                    blast.radius = sporkHemmerageBlastRadius * grabCount;
                    blast.maxTimer = 0.15f;
                    blast.falloffModel = BlastAttack.FalloffModel.None;
                    blast.damageColorIndex = DamageColorIndex.SuperBleed;
                    blast.damageType = DamageType.AOE | DamageType.SuperBleedOnCrit;
                    blast.crit = true;
                    blast.procCoefficient = 1f;

                    blast.explosionEffect = sporkBlastEffect;
                    blast.hasSpawnedDelayEffect = true;

                    blast.teamFilter = new TeamFilter()
                    {
                        teamIndexInternal = (int)body.teamComponent.teamIndex,
                        defaultTeam = TeamIndex.None,
                        teamIndex = body.teamComponent.teamIndex,
                        NetworkteamIndexInternal = (int)body.teamComponent.teamIndex
                    };

                    // Bleed Buff
                    body.AddTimedBuff(SporkBleedBuff.buff, sporkBaseDuration); //  * grabCount

                    _ = Util.PlaySound("Play_item_void_bleedOnHit_start", body.gameObject);
                }
            }
            orig(self, damageValue, damagePosition, damageIsSilent, attacker, delayedDamage, firstHitOfDelayedDamage);
        }//*/

        // Bleed Splash
        private void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            orig(ref inflictDotInfo);
            //Log.Debug("Dot Controller Spork: " + inflictDotInfo.dotIndex + " = " + DotController.DotIndex.Bleed + " | " + DotController.DotIndex.SuperBleed);
            if (inflictDotInfo.dotIndex is DotController.DotIndex.Bleed or DotController.DotIndex.SuperBleed)
            {
                CharacterBody inflictor = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
                if (inflictor && inflictor.inventory && inflictor.HasBuff(SporkBleedBuff.buff))
                {
                    CharacterBody victim = inflictDotInfo.victimObject ? inflictDotInfo.victimObject.GetComponent<CharacterBody>() : null;

                    //Log.Debug("Hey Spork... ... Has Buff? " + inflictor.HasBuff(SporkBleedBuff.buff));

                    if (victim)
                    {
                        _ = Util.PlaySound("Play_item_void_bleedOnHit_start", inflictor.gameObject);

                        int grabCount = inflictor.inventory.GetItemCountEffective(item);
                        grabCount = grabCount < 0 ? 0 : grabCount;

                        // Bleed Blast
                        GameObject explostionObject = Object.Instantiate(willOWisp, victim.corePosition, Quaternion.identity);
                        DelayBlast blast = explostionObject.GetComponent<DelayBlast>();
                        //GameObject FakePlayer = body.gameObject.InstantiateClone("Fake Player");

                        //UnityEngine.Object.Destroy(FakePlayer.GetComponent<CharacterBody>());
                        blast.position = victim.corePosition;
                        blast.attacker = victim.gameObject;
                        blast.inflictor = victim.gameObject; // To avoid an infinite loop?
                        blast.baseDamage = inflictor.damage;
                        blast.baseForce = 400f;
                        //blast.bonusForce = ;
                        blast.radius = sporkBaseBleedBlastRadius + sporkStackBleedBlastRadius * grabCount;
                        blast.maxTimer = .25f;
                        blast.falloffModel = BlastAttack.FalloffModel.None;
                        blast.damageColorIndex = DamageColorIndex.Bleed;
                        blast.damageType = DamageType.AOE | DamageType.BleedOnHit;
                        blast.crit = false;
                        blast.procCoefficient = .7f;

                        blast.explosionEffect = sporkBlastEffect;
                        blast.hasSpawnedDelayEffect = true;

                        blast.teamFilter = new TeamFilter()
                        {
                            teamIndexInternal = (int)inflictor.teamComponent.teamIndex,
                            defaultTeam = TeamIndex.None,
                            teamIndex = inflictor.teamComponent.teamIndex,
                            NetworkteamIndexInternal = (int)inflictor.teamComponent.teamIndex
                        };
                    }
                }
            }
        }

        /*/
        
        // Bleed Splash
        private void GlobalEventManager_OnHitEnemy(On.RoR2.GlobalEventManager.orig_OnHitEnemy orig, GlobalEventManager self, DamageInfo damageInfo, GameObject victim)
        {
            orig(self, damageInfo, victim);
            try
            {
                if (damageInfo.attacker)
                {
                    CharacterBody inflictor = damageInfo.attacker.GetComponent<CharacterBody>();
                    if (inflictor.inventory.GetItemCountEffective(item) > 0 && victim && victim.GetComponent<CharacterBody>())
                    {
                        CharacterBody sporkVictim = victim.GetComponent<CharacterBody>();

                        
                        //if (sporkVictim.HasBuff(RoR2Content.Buffs.Bleeding))
                        //{
                            // Has item and enemy is bleeding
                            //SporkBleedBehavior behavior = sporkVictim.AddItemBehavior<SporkBleedBehavior>(1);
                            //behavior.AddInflictor(inflictor);

                        //}
                    }
                }
            }
            catch
            {
                //Log.Warning("Spork On Hit Expected Error");
            }
        }

        // Add Heal from Bleeds
        private void DotController_EvaluateDotStacksForType(On.RoR2.DotController.orig_EvaluateDotStacksForType orig, DotController self, DotController.DotIndex dotIndex, float dt, out int remainingActive)
        {
            if (self && self.victimObject && self.victimBody &&
                dotIndex is DotController.DotIndex.Bleed or DotController.DotIndex.SuperBleed)
            {
                //Log.Debug("Evaluating...");
                SporkBleedBehavior behavior = self.victimObject.GetComponent<SporkBleedBehavior>();
                if (behavior)
                {
                    CharacterBody[] inflictors = behavior.GetInflictors();
                    foreach (CharacterBody body in inflictors)
                    {
                        int grabCount = body.inventory.GetItemCountEffective(item);
                        if (grabCount > 0)
                        {
                            //Log.Warning("Healing Inflictors ! ! ! " + body.name);
                            //_ = body.healthComponent.Heal(bleedHealing * grabCount, default, true);
                        }
                    }
                }
                //Log.Debug("Has inflictors?");
            }
            orig(self, dotIndex, dt, out remainingActive);
            //Log.Debug(" ? but How inflictors");
        }

        private void DotController_OnDotStackRemovedServer(On.RoR2.DotController.orig_OnDotStackRemovedServer orig, DotController self, DotController.DotStack dotStack)
        {
            orig(self, dotStack);
            //Log.Debug(" > > test bleed");
            DotController.DotIndex dotIndex = dotStack.dotIndex;
            //Log.Debug(" > > test bleed stop");
            if (dotIndex == DotController.DotIndex.Bleed && self.victimBody)
            {
                //Log.Debug(" | | < < Eats with spork?");
                if (self.victimBody.GetComponent<SporkBleedBehavior>() != null)
                {
                    //Log.Debug(" | | < < Has eatten with a spork");
                    _ = self.victimBody.AddItemBehavior<SporkBleedBehavior>(0);
                }
            }
        }

        // Used to keep track of who heals from bleed damage
        public class SporkBleedBehavior : CharacterBody.ItemBehavior
        {
            private readonly List<CharacterBody> _inflictors = [];

            public void AddInflictor(CharacterBody inflictor)
            {
                //Log.Debug("Adding " + inflictor.name + " to inflictors");
                if (!_inflictors.Contains(inflictor))
                {
                    _inflictors.Add(inflictor);
                }
            }

            public CharacterBody[] GetInflictors()
            {
                //Log.Debug("In Inflictor get method");
                return [.. _inflictors];
            }

            public void OnDestroy()
            {
                //Log.Warning(" , Spork Bleed ended...");
                _inflictors.Clear();
            }
        }

        //
        //private void HealthComponent_UpdateLastHitTime(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker)
        //{
        //    if (NetworkServer.active && self.body && damageValue > 0f && self.isHealthLow)
        //    {
        //        int grabCount = self.body.inventory.GetItemCount(item);
        //        if (grabCount > 0)
        //        {
        //            Log.Debug(" / / / Low spork, now drink BLOOD!");
        //            PigsSporkBehavior behavior = self.body.GetComponent<PigsSporkBehavior>();
        //            behavior.InLowHealth = true;
        //        }
        //    }
        //    orig(self, damageValue, damagePosition, damageIsSilent, attacker);
        //}

        //*/
    }
}