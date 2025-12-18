using BepInEx.Configuration;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using System;
using UnityEngine;

namespace UltitemsCyan.Items.Tier1
{
    // TODO: check if Item classes needs to be public
    // Notes
    // With 3 you take a maximum of 75.2%, just more than 75% to get low health threashold
    // With 10 you take a maximum of 47.6%, less than 50% requiring 3 hits to die
    // With 19 you take a maximum of 32.4%, less than 33% requiring 4 hits to die

    public class KoalaSticker : ItemBase
    {
        public static ItemDef item;
        private const float hyperbolicPercent = 11f;
        private const float minDamage = 5f;
        // 1 - 1 / (percent * n + 1)


        public override void Init(ConfigFile configs)
        {
            Log.Warning("-JYPrint Hello?!?! 1");
            const string itemName = "Koala Sticker";
            if (!CheckItemEnabledConfig(itemName, "White", configs))
            {
                return;
            }
            item = CreateItemDef(
                "KOALASTICKER",
                itemName,
                "Reduce the maximum damage you can take.",
                "Only lose a maximum of <style=cIsHealing>90%</style> <style=cStack>(-12% per stack)</style> of your <style=cIsHealing>health</style> from a hit. Does not reduce below <style=cIsHealing>5 health</style>.",
                "Like the bear but more consistant...   and more cute",
                ItemTier.Tier1,
                UltAssets.KoalaStickerSprite,
                UltAssets.KoalaStickerPrefab,
                [ItemTag.CanBeTemporary, ItemTag.Utility]
            );
            Log.Warning("-JYPrint Hello?!?! 1.5");
        }


        protected override void Hooks()
        {
            //Log.Warning("-JYPrint Koala Hooks Start");
            //IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
            //IOnTakeDamageServerReceiver.OnTakeDamageServer +=  ??? Use delegates like Too Many Items Mod
            GenericGameEvents.BeforeTakeDamage += (damageInfo, attackerInfo, victimInfo) =>
            {
                CharacterBody victimBody = victimInfo.body;
                

                if (victimBody && victimBody.master && victimBody.master.inventory &&
                        damageInfo.rejected == false && damageInfo.damageColorIndex != DamageColorIndex.DelayedDamage)
                {
                    // grab Count
                    int grabCount = victimBody.master.inventory.GetItemCountEffective(item);
                    if (grabCount > 0)
                    {
                        //Log.Debug("Koala Taken Damage for " + victimBody.GetUserName() + " with " + victimBody.healthComponent.fullCombinedHealth + "\t health");
                        //Log.Debug("Max Percent: " + 100 / (hyperbolicPercent / 100 * grabCount + 1) + " of " + victimBody.healthComponent.fullCombinedHealth);
                        // max Damage
                        float maxDamage = victimBody.healthComponent.fullCombinedHealth / (hyperbolicPercent / 100 * grabCount + 1);
                        if (maxDamage < minDamage)
                        {
                            maxDamage = minDamage;
                        }
                        //Log.Debug("Is " + td + "\t > " + maxDamage + "?");
                        if (damageInfo.damage > maxDamage)
                        {
                            Log.Warning("Koala BLOCK " + damageInfo.damage + " to " + maxDamage + " ! ! for " + victimBody.name);
                            EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, new EffectData
                            {
                                origin = damageInfo.position,
                                rotation = Util.QuaternionSafeLookRotation((damageInfo.force != Vector3.zero) ? damageInfo.force : UnityEngine.Random.onUnitSphere),
                                //color = new Color(10, 64, 95) // Koala Skin Colors Deson't Do Anything
                            }, true);
                            damageInfo.damage = maxDamage;
                        }
                    }
                }
            };
            //Log.Warning("-JYPrint Koala Hooks END");
        }
    }
}


/*


[Error  : Unity Log] NullReferenceException: Object reference not set to an instance of an object
Stack trace:
UltitemsCyan.Items.Tier1.KoalaSticker+<>c.<HealthComponent_TakeDamage>b__4_0 (RoR2.HealthComponent hc, RoR2.DamageInfo di, System.Single td) (at <c53460e9dbbb428aa9be181a42ce4fc4>:IL_000E)
MonoMod.Cil.RuntimeILReferenceBag+FastDelegateInvokers.Invoke[T1,T2,T3,TResult] (T1 arg1, T2 arg2, T3 arg3, MonoMod.Cil.RuntimeILReferenceBag+FastDelegateInvokers+Func`4[T1,T2,T3,TResult] del) (at <6733e342b5b549bba815373898724469>:IL_0000)
RoR2.HealthComponent.TakeDamage (RoR2.DamageInfo damageInfo) (at <1d532be543be416b9db3594e4b62447d>:IL_0D5B)
DMD<>?279537920.Trampoline<RoR2.HealthComponent::TakeDamage>?819928064 (RoR2.HealthComponent , RoR2.DamageInfo ) (at <4d732e2b12ed49c8ba95348f98cce03d>:IL_0020)
DebugToolkit.Hooks.NonLethatDamage (On.RoR2.HealthComponent+orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo) (at <cb07732d859f4712b74c5802a0c569c5>:IL_0022)
DMD<>?279537920.Hook<RoR2.HealthComponent::TakeDamage>?1313805312 (RoR2.HealthComponent , RoR2.DamageInfo ) (at <20b386dce9c44007b14487ee568833c3>:IL_000A)
DMD<>?279537920.Trampoline<RoR2.HealthComponent::TakeDamage>?1091579904 (RoR2.HealthComponent , RoR2.DamageInfo ) (at <84d8c5304fc64d84b8b27bfd05d909ec>:IL_0020)
UltitemsCyan.Items.Tier1.CremeBrulee.HealthComponent_TakeDamage (On.RoR2.HealthComponent+orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo) (at <c53460e9dbbb428aa9be181a42ce4fc4>:IL_013C)
DMD<>?279537920.Hook<RoR2.HealthComponent::TakeDamage>?1305795584 (RoR2.HealthComponent , RoR2.DamageInfo ) (at <a5435c830d0042358ad7d1c63095e9fd>:IL_0014)
DMD<>?279537920.Trampoline<RoR2.HealthComponent::TakeDamage>?636471296 (RoR2.HealthComponent , RoR2.DamageInfo ) (at <dfb7e45ac4594faf82a6f5cef85b6d1d>:IL_0020)
UltitemsCyan.Items.Tier3.SuesMandibles.HealthComponent_TakeDamage (On.RoR2.HealthComponent+orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo) (at <c53460e9dbbb428aa9be181a42ce4fc4>:IL_0001)
DMD<>?279537920.Hook<RoR2.HealthComponent::TakeDamage>?-328387840 (RoR2.HealthComponent , RoR2.DamageInfo ) (at <f6eb130cd99e4bf7bca9c7b066ad574f>:IL_0014)
DMD<>?279537920.Trampoline<RoR2.HealthComponent::TakeDamage>?250040320 (RoR2.HealthComponent , RoR2.DamageInfo ) (at <6cde896ddf524f97b8939229fa396df7>:IL_0020)
UltitemsCyan.Items.Void.DriedHam.HealthComponent_TakeDamage (On.RoR2.HealthComponent+orig_TakeDamage orig, RoR2.HealthComponent self, RoR2.DamageInfo damageInfo) (at <c53460e9dbbb428aa9be181a42ce4fc4>:IL_0126)
DMD<>?279537920.Hook<RoR2.HealthComponent::TakeDamage>?830010752 (RoR2.HealthComponent , RoR2.DamageInfo ) (at <99c3d5d23596499e92cde9a40c61780c>:IL_0014)
RoR2.BlastAttack.PerformDamageServer (RoR2.BlastAttack+BlastAttackDamageInfo& blastAttackDamageInfo) (at <1d532be543be416b9db3594e4b62447d>:IL_00A4)
RoR2.BlastAttack.HandleHits (RoR2.BlastAttack+HitPoint[] hitPoints) (at <1d532be543be416b9db3594e4b62447d>:IL_01B7)
RoR2.BlastAttack.Fire () (at <1d532be543be416b9db3594e4b62447d>:IL_0007)
RoR2.Projectile.ProjectileExplosion.DetonateServer () (at <1d532be543be416b9db3594e4b62447d>:IL_0177)
RoR2.Projectile.ProjectileExplosion.Detonate () (at <1d532be543be416b9db3594e4b62447d>:IL_0007)
RoR2.Projectile.ProjectileImpactExplosion.FixedUpdate () (at <1d532be543be416b9db3594e4b62447d>:IL_013F)



 */