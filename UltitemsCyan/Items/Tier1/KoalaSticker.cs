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
        }


        protected override void Hooks()
        {
            IL.RoR2.HealthComponent.TakeDamageProcess += HealthComponent_TakeDamageProcess;
        }

        private void HealthComponent_TakeDamageProcess(ILContext il)
        {
            ILCursor c = new(il); // Make new ILContext

            int num3 = -1;   //Initial Total Damage
            int num14 = -1; //New Total Damage

            // Inject code just before damage is subtracted from health
            // Go just before the "if (num12 > 0f && this.barrier > 0f)" line, which is equal to the following instructions

            //Log.Warning("Koala Sticker Take Damage");

            if (c.TryGotoNext(MoveType.Before,                              // TODO make cursor search more robust
                x => x.MatchLdloc(out num3),                                // 1130 ldloc.s V_6 (7)
                x => x.MatchStloc(out num14),                               // 1130 stloc.s V_7 (8)
                x => x.MatchLdloc(out num14),                               // 1130 ldloc.s V_7 (8)
                x => x.MatchLdcR4(0f),                                      // 1131 ldc.r4 0
                x => x.Match(OpCodes.Ble_Un_S),                             // 1132 ble.un.s 1200 (0D38) ldloc.s V_7 (7)
                x => x.MatchLdarg(0),                                       // 1133 ldarg.0
                x => x.MatchLdcI4(0),                                       // 1134 ldci4.0
                x => x.MatchStfld<HealthComponent>("isShieldRegenForced")   // 1135 ldfld float32 RoR2.HealthComponent::barrier
            ))
            {

                //Log.Debug(" * * * Start C Index: " + c.Index + " > " + c.ToString());
                //[Warning:UltitemsCyan] * **Start C Index: 1129 > // ILCursor: System.Void DMD<RoR2.HealthComponent::TakeDamage>?-456176384::RoR2.HealthComponent::TakeDamage(RoR2.HealthComponent,RoR2.DamageInfo), 1129, Next
                //IL_0e05: stfld System.Single RoR2.HealthComponent::adaptiveArmorValue
                //IL_0e0a: ldloc.s V_7

                //give_item koalasticker 100

                c.Index += 4;

                //Log.Debug(" * * * +4 Working Index: " + c.Index + " > " + c.ToString());
                //[Debug  :UltitemsCyan] * **+4 Working Index: 1133 > // ILCursor: System.Void DMD<RoR2.HealthComponent::TakeDamage>?-771449600::RoR2.HealthComponent::TakeDamage(RoR2.HealthComponent,RoR2.DamageInfo), 1133, None
                //IL_0e10: ldc.r4 0
                //IL_0e15: ble.un.s IL_0e21


                _ = c.Emit(OpCodes.Ldarg, 0);     // Load Health Component
                _ = c.Emit(OpCodes.Ldarg, 1);     // Load Damage Info (If Damage rejected, returned earlier)
                _ = c.Emit(OpCodes.Ldloc, num14);   // Load Total Damage

                // Run custom code
                _ = c.EmitDelegate<Func<HealthComponent, DamageInfo, float, float>>((hc, di, td) =>
                {
                    CharacterBody cb = hc.body;
                    if (cb)
                    {
                        //Log.Debug("Health: " + hc.fullCombinedHealth + "\t Body: " + cb.GetUserName() + "\t Damage: " + td);
                        if (cb.master && cb.master.inventory)
                        {
                            // grab Count
                            int gC = cb.master.inventory.GetItemCountEffective(item);
                            if (gC > 0)
                            {
                                //Log.Debug("Koala Taken Damage for " + cb.GetUserName() + " with " + hc.fullCombinedHealth + "\t health");
                                //Log.Debug("Max Percent: " + ((hyperbolicPercent / 100 * grabCount) + 1) + " of " + hc.fullCombinedHealth);
                                // max Damage
                                float mD = hc.fullCombinedHealth / (hyperbolicPercent / 100 * gC + 1);
                                //Util.ConvertAmplificationPercentageIntoReductionNormalized(hyperbolicPercent / 100 );
                                if (mD < minDamage)
                                {
                                    mD = minDamage;
                                }
                                //Log.Debug("Is " + td + "\t > " + maxDamage + "?");
                                if (td > mD)
                                {
                                    Log.Warning("Koala BLOCK ! ! for " + cb.name);
                                    EffectManager.SpawnEffect(HealthComponent.AssetReferences.bearEffectPrefab, new EffectData
                                    {
                                        origin = di.position,
                                        rotation = Util.QuaternionSafeLookRotation((di.force != Vector3.zero) ? di.force : UnityEngine.Random.onUnitSphere),
                                        //color = new Color(10, 64, 95) // Koala Skin Colors Deson't Do Anything
                                    }, true);
                                    return mD;
                                }
                            }
                        }
                    }
                    return td;
                });

                _ = c.Emit(OpCodes.Stloc, num14); // Store Total Damage
                //
                //}
                //else
                //{
                //    Log.Warning("Koala cannot find 'for (int k = 0; k < num15; k++){}'");
                //}
            }
            else
            {
                Log.Warning("Koala cannot find '(num12 > 0f && this.barrier > 0f)'");
            }
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