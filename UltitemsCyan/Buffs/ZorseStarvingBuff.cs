using RoR2;
using UltitemsCyan.Items.Void;
using UnityEngine;
using UnityEngine.AddressableAssets;
using static R2API.DotAPI;
using static RoR2.DotController;

namespace UltitemsCyan.Buffs
{
    public class ZorseStarvingBuff : BuffBase
    {
        public static BuffDef buff;
        public static DotIndex index;

        //public readonly GameObject PreFractureEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/PreFractureEffect.prefab").WaitForCompletion();

        //public readonly GameObject FractureEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/MegaCrabBlackCannonStuckProjectile2.prefab").WaitForCompletion(); // Cool but shakes screen
        //public readonly GameObject testEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidMegaCrab/VoidMegaCrabDeathBombExplosion.prefab").WaitForCompletion(); // 
        //public readonly GameObject testEffect1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabEscapeEffect.prefab").WaitForCompletion(); // Way too big
        //public readonly GameObject testEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidCrabSpawnEffect.prefab").WaitForCompletion(); // Big and purple
        //public readonly GameObject testEffect3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/VoidRaidSafeWardDisappearEffect.prefab").WaitForCompletion(); // Big and purple

        //public readonly GameObject testEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSuppressor/SuppressorClapEffect.prefab").WaitForCompletion(); // Small swirl effect
        //public readonly GameObject testEffect3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSuppressor/SuppressorDieEffect.prefab").WaitForCompletion(); // blue shells fly out then fall straight down
        //public readonly GameObject testEffect4 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSuppressor/SuppressorBreakFromShellFirstTimeEffect.prefab").WaitForCompletion(); // blue shells fly out then fall straight down
        //public readonly GameObject testEffect1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSuppressor/SuppressorItemSpawnEffect.prefab").WaitForCompletion(); // large ambiant rock effect
        //public readonly GameObject testEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSuppressor/SuppressorPreEradicateItemEffect.prefab").WaitForCompletion(); // small purple sphere
        //public readonly GameObject testEffect3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSuppressor/SuppressorRetreatToShellEffect.prefab").WaitForCompletion(); // rocks fly out then zoom back in
        //public readonly GameObject testEffect4 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidSurvivor/mdlVoidSurvivorPodParticleEffectBaseMesh.fbx").WaitForCompletion(); // doesn't work
        //public readonly GameObject testEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Birdshark/BirdsharkDeathEffect.prefab").WaitForCompletion(); // Impactful but feathers
        //public readonly GameObject testEffect1 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/PreFractureEffect.prefab").WaitForCompletion(); // doesn't work
        //public readonly GameObject testEffect2 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ChainLightningVoid/VoidLightningOrbEffect.prefab").WaitForCompletion(); // no visuals
        //public readonly GameObject testEffect3 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ElementalRingVoid/ElementalRingVoidImplodeEffect.prefab").WaitForCompletion(); // large but light ring effect
        //public readonly GameObject testEffect4 = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/MissileVoid/VoidImpactEffect.prefab").WaitForCompletion(); // small but impactful (rocket sound)

        public readonly GameObject FractureEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/BleedOnHitVoid/FractureImpactEffect.prefab").WaitForCompletion();
        public readonly GameObject LaserImpactEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/VoidRaidCrab/LaserImpactEffect.prefab").WaitForCompletion(); // Tiny
        //public readonly GameObject ElementalRingVoidImplodeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/ElementalRingVoid/ElementalRingVoidImplodeEffect.prefab").WaitForCompletion(); // large but light ring effect

        private readonly float duration = ZorsePill.duration;
        //private const float airSpeed = Chrysotope.airSpeed;

        public override void Init()
        {
            buff = DefineBuff("Zorse Starving Buff", false, true, UltAssets.ZorseStarveSprite, true);

            DotDef dotDef = new()
            {
                associatedBuff = buff,
                damageCoefficient = 1f,
                damageColorIndex = DamageColorIndex.Void,
                interval = duration,
                resetTimerOnAdd = true,
                //terminalTimedBuff
                //terminalTimedBuffDuration
            };
            //var customDotBehaviour = DotAPI.CustomDotBehaviour
            //var customDotVisual = DotAPI.CustomDotVisual.CreateDelegate

            //Log.Warning("1st Custom Count " + CustomDotCount + " | Count: " + DotIndex.Count);
            //RegisterDotDef(null);
            index = RegisterDotDef(dotDef);
            //Log.Debug("2nd Custom Count " + CustomDotCount + " | Count: " + DotIndex.Count);

            Hooks();
        }

        protected void Hooks()
        {
            On.RoR2.CharacterBody.OnBuffFinalStackLost += CharacterBody_OnBuffFinalStackLost;
        }

        private void CharacterBody_OnBuffFinalStackLost(On.RoR2.CharacterBody.orig_OnBuffFinalStackLost orig, CharacterBody self, BuffDef buffDef)
        {
            orig(self, buffDef);
            if (self && buffDef == buff) // && list.Count > 0
            {
                //Log.Warning("s s s Spawning Fracture Effect ! ! !");
                /*EffectManager.SpawnEffect(FractureEffect, new EffectData
                {
                    origin = self.corePosition,
                    color = new Color(0.2392f, 0.8196f, 0.917647f) // Cyan Lunar color
                }, true);*/
                Log.Warning(" zzor zzor OnBuffFinalStackLost: pre spawn zorse");
                EffectManager.SpawnEffect(FractureEffect, new EffectData
                {
                    origin = self.corePosition,
                    color = new Color(0.2392f, 0.8196f, 0.917647f) // Cyan Lunar color
                }, true);
                EffectManager.SpawnEffect(LaserImpactEffect, new EffectData
                {
                    origin = self.corePosition,
                    color = new Color(0.2392f, 0.8196f, 0.917647f) // Cyan Lunar color
                }, true);
                Log.Warning(" zzor zzor OnBuffFinalStackLost: POST spawn zorse");
                /*EffectManager.SpawnEffect(ElementalRingVoidImplodeEffect, new EffectData
                {
                    origin = self.corePosition,
                    color = new Color(0.2392f, 0.8196f, 0.917647f) // Cyan Lunar color
                }, true);*/
            }
            //Log.Debug("OnBuffFinal Exclude From Thorns? " + buffDef.flags.HasFlag(BuffDef.Flags.ExcludeFromNoxiousThorns));
            //Log.Debug("OnBuffFinal Netowrking? " + NetworkServer.active);
        }
    }
}