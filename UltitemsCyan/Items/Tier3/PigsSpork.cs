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

        public const float sporkBleedChance = 200f;
        public const float sporkBaseDuration = 12f;

        private static readonly GameObject willOWisp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ExplodeOnDeath/WilloWispDelay.prefab").WaitForCompletion();
        private static readonly GameObject sporkBlastEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/BleedOnHitAndExplode/BleedOnHitAndExplode_Explosion.prefab").WaitForCompletion();
        public const float sporkBlastRadius = 32f;

        //public const float sporkDurationPerStack = 5f;
        private const float bleedHealing = 3;

        public override void Init(ConfigFile configs)
        {
            if (!CheckItemEnabledConfig("Pigs Spork", "Red", configs)) // Can't have apostrophes
            {
                return;
            }
            item = CreateItemDef(
                "PIGSSPORK",
                "Pig's Spork",
                "Bleeds heal you. When hit at low health explode and gain 200% chance to bleed enemies.",
                // Applying bleed will make the enemy splash bleed on adjacent enemies when you have the 200% bleed buff
                "Bleed damage <style=cIsHealing>heals</style> for <style=cIsHealing>3</style> <style=cStack>(+3 per stack)</style> <style=cIsHealing>health</style>. Taking damage below <style=cIsHealth>25% health</style> will <style=cIsHealth>hemorrhage</style> all enemies within <style=cIsDamage>32m</style> <style=cStack>(+32m per stack)</style> and grant <style=cIsDamage>200%</style> chance to <style=cIsDamage>bleed</style> for <style=cIsDamage>12s</style> <style=cStack>(+12 per stack)</style>.",
                "There once was a pet named porky\nA cute and chubby pig\n\nBut the farmer broke his fork\nAnd used the spoon to dig\n\nSo he made a Sporky Spig\n",
                ItemTier.Tier3,
                UltAssets.PigsSporkSprite,
                UltAssets.PigsSporkPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Damage, ItemTag.Healing, ItemTag.LowHealth]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.DotController.EvaluateDotStacksForType += DotController_EvaluateDotStacksForType;
            On.RoR2.DotController.InflictDot_refInflictDotInfo += DotController_InflictDot_refInflictDotInfo;
            On.RoR2.DotController.OnDotStackRemovedServer += DotController_OnDotStackRemovedServer;
            On.RoR2.GlobalEventManager.OnHitEnemy += GlobalEventManager_OnHitEnemy;
            On.RoR2.HealthComponent.UpdateLastHitTime += HealthComponent_UpdateLastHitTime;
        }

        // Initial Explosion when at low health
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
                    blast.attacker = attacker;
                    blast.inflictor = body.gameObject;
                    blast.baseDamage = body.damage;
                    blast.baseForce = 1000f;
                    //blast.bonusForce = ;
                    blast.radius = sporkBlastRadius * grabCount;
                    blast.maxTimer = 0.1f;
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
                    body.AddTimedBuff(SporkBleedBuff.buff, sporkBaseDuration * grabCount);

                    _ = Util.PlaySound("Play_item_void_bleedOnHit_start", body.gameObject);
                }
            }
            orig(self, damageValue, damagePosition, damageIsSilent, attacker, delayedDamage, firstHitOfDelayedDamage);
        }//*/

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
                        if (sporkVictim.HasBuff(RoR2Content.Buffs.Bleeding))
                        {
                            // Has item and enemy is bleeding
                            SporkBleedBehavior behavior = sporkVictim.AddItemBehavior<SporkBleedBehavior>(1);
                            behavior.AddInflictor(inflictor);
                            if (inflictor.HasBuff(SporkBleedBuff.buff))
                            {
                                _ = Util.PlaySound("Play_item_void_bleedOnHit_start", inflictor.gameObject);
                            }
                        }
                    }
                }
            }
            catch
            {
                //Log.Warning("Spork On Hit Expected Error");
            }
        }

        // Add Inflictor to Victim
        // TODO supposed to be when hit and while bleeding, not if bleeding was inflicted
        private void DotController_InflictDot_refInflictDotInfo(On.RoR2.DotController.orig_InflictDot_refInflictDotInfo orig, ref InflictDotInfo inflictDotInfo)
        {
            orig(ref inflictDotInfo);
            //Log.Debug("Dot Controller Spork: " + inflictDotInfo.dotIndex + " = " + DotController.DotIndex.Bleed + " | " + DotController.DotIndex.SuperBleed);
            if (inflictDotInfo.dotIndex is DotController.DotIndex.Bleed or DotController.DotIndex.SuperBleed)
            {
                CharacterBody inflictor = inflictDotInfo.attackerObject.GetComponent<CharacterBody>();
                if (inflictor.inventory.GetItemCountEffective(item) > 0)
                {
                    CharacterBody victim = inflictDotInfo.victimObject ? inflictDotInfo.victimObject.GetComponent<CharacterBody>() : null;

                    SporkBleedBehavior behavior = victim.AddItemBehavior<SporkBleedBehavior>(1);
                    behavior.AddInflictor(inflictor);
                }
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
                            _ = body.healthComponent.Heal(bleedHealing * grabCount, default, true);
                        }
                    }
                }
                //Log.Debug("Has inflictors?");
            }
            orig(self, dotIndex, dt, out remainingActive);
            //Log.Debug(" ? but How inflictors");
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

        /*/
        private void HealthComponent_UpdateLastHitTime(On.RoR2.HealthComponent.orig_UpdateLastHitTime orig, HealthComponent self, float damageValue, Vector3 damagePosition, bool damageIsSilent, GameObject attacker)
        {
            if (NetworkServer.active && self.body && damageValue > 0f && self.isHealthLow)
            {
                int grabCount = self.body.inventory.GetItemCount(item);
                if (grabCount > 0)
                {
                    Log.Debug(" / / / Low spork, now drink BLOOD!");
                    PigsSporkBehavior behavior = self.body.GetComponent<PigsSporkBehavior>();
                    behavior.InLowHealth = true;
                }
            }
            orig(self, damageValue, damagePosition, damageIsSilent, attacker);
        }//*/
    }
}