using RoR2;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.AddressableAssets;
using BepInEx.Configuration;

namespace UltitemsCyan.Items.Tier2
{
    // TODO: check if Item classes needs to be public
    public class XenonAmpoule : ItemBase
    {
        private static ItemDef item;

        public static GameObject TracerRailgun = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgun.prefab").WaitForCompletion();
        //public static GameObject TracerRailgunCryo = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunCryo.prefab").WaitForCompletion();
        public static GameObject TracerRailgunSuper = Addressables.LoadAssetAsync<GameObject>("RoR2/DLC1/Railgunner/TracerRailgunSuper.prefab").WaitForCompletion();

        //public static GameObject Tracer2 = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/OrbitalLaser/TracerAncientWisp.prefab").WaitForCompletion();
        //public static GameObject Tracer3 = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Mage/TracerMageLightningLaser.prefab").WaitForCompletion();
        //public static GameObject Tracer4 = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Mage/TracerMageIceLaser.prefab").WaitForCompletion();
        //public static GameObject Tracer5 = Addressables.LoadAssetAsync<GameObject>("RoR2/Junk/Commando/TracerBarrage2.prefab").WaitForCompletion();

        public const float baseDamage = 16f / 3f; // div 3f to account for weak point and crit
        public const float damagePerStack = 4f / 3f; // div 3f to account for weak point and crit
        public const float shortDamagePercent = 70f; // Percent of normal damage

        public const float shortLaserRadius = 1.6f;
        public const float laserRadius = 2.5f;
        public const float longLaserRadius = 3.2f;

        public const float forceFromCooldown = 550f;

        public const float shortCooldown = 20f;    // Less than or equal, small laser and has half damage
        public const float longCooldown = 60f;   // Greater than or equal, big laser multiple cooldown by cooldown / 60
        public const float maxCooldownMultipler = 4f;

        public const float subCooldown = .3f;

        /* Use muzzle location?
        	Vector3 position = base.transform.position;
			Ray aimRay = this.GetAimRay();
			Transform transform = this.FindActiveEquipmentDisplay();
			if (transform)
			{
				ChildLocator componentInChildren = transform.GetComponentInChildren<ChildLocator>();
				if (componentInChildren)
				{
					Transform transform2 = componentInChildren.FindChild("Muzzle");
					if (transform2)
					{
						aimRay.origin = transform2.position;
					}
				}
			}
         */



        public override void Init(ConfigFile configs)
        {
            const string itemName = "Xenon Ampoule";
            if (!CheckItemEnabledConfig(itemName, "Green", configs))
            {
                return;
            }
            item = CreateItemDef(
                "XENONAMPOULE",
                itemName,
                "Activating your Equipment also fires a laser. Damage and size scale with equipment duration.",
                "Activating your Equipment also fires a <style=cIsDamage>critting laser</style> for <style=cIsDamage>1600%</style> <style=cStack>(+400% per stack)</style> base damage. The damage scales with an equipment's cooldown.",
                "It's Purple because I messed up. Xenon is supposed to be more blue than hyrdogen, but I wanted an X name. Sorry.",
                ItemTier.Tier2,
                UltAssets.XenonAmpouleSprite,
                UltAssets.XenonAmpoulePrefab,
                [ItemTag.CanBeTemporary, ItemTag.Damage, ItemTag.EquipmentRelated]
            );
        }

        protected override void Hooks()
        {
            //EquipmentSlot.onServerEquipmentActivated += EquipmentSlot_onServerEquipmentActivated; // Doesn't work?
            //On.RoR2.EquipmentSlot.PerformEquipmentAction // For Equipment Activation (if used, has a chance of returning before eaching hooked code)
            On.RoR2.EquipmentSlot.OnEquipmentExecuted += EquipmentSlot_OnEquipmentExecuted; // For If the Equipment was fired
        }

        private void EquipmentSlot_OnEquipmentExecuted(On.RoR2.EquipmentSlot.orig_OnEquipmentExecuted orig, EquipmentSlot self)
        {
            orig(self);

            //Log.Debug("Xenon Perform Equipment Action");
            if (NetworkServer.active && self.characterBody && self.inventory)
            {
                //Log.Debug(" ? ? ? Xenon Perform Equipment Action Actually Activated?");
                CharacterBody activator = self.characterBody;
                int grabCount = activator.inventory.GetItemCountEffective(item);
                if (grabCount > 0)
                {
                    // Cooldown after reduction from items
                    //Log.Debug(" ! Xe Fire | Cooldown: " + self.cooldownTimer);

                    Ray aimRay = activator.inputBank.GetAimRay();
                    //float damage = activator.damage * (baseDamage + (damagePerStack * (grabCount - 1)));
                    float damage, radius, force;
                    GameObject tracer;

                    // Get Default Cooldown of Item
                    float cooldown = EquipmentCatalog.GetEquipmentDef(self.equipmentIndex).cooldown;

                    if (cooldown <= shortCooldown)
                    {
                        //Log.Debug("Short");
                        _ = Util.PlaySound("Play_railgunner_m2_fire", activator.gameObject);
                        tracer = TracerRailgun;
                        damage = (baseDamage + damagePerStack * (grabCount - 1)) * shortDamagePercent / 100f;//  * (self.cooldownTimer / 45f)
                        radius = shortLaserRadius;
                        force = forceFromCooldown;
                    }
                    else if (cooldown < longCooldown)
                    {
                        //Log.Debug("Normal");
                        _ = Util.PlaySound("Play_voidRaid_snipe_shoot_final", activator.gameObject);
                        tracer = TracerRailgunSuper;
                        damage = baseDamage + damagePerStack * (grabCount - 1);
                        radius = laserRadius;
                        force = forceFromCooldown * 2;
                    }
                    else
                    {
                        //Log.Debug("Long");
                        _ = Util.PlaySound("Play_voidRaid_snipe_shoot_final", activator.gameObject);
                        tracer = TracerRailgunSuper;
                        damage = (baseDamage + damagePerStack * (grabCount - 1)) * Mathf.Max(cooldown / longCooldown, maxCooldownMultipler);
                        radius = longLaserRadius;
                        force = forceFromCooldown * 3;
                    }

                    //Log.Debug((baseDamage + damagePerStack * (grabCount - 1)) + " * " + Mathf.Max(cooldown / longCooldown, maxCooldownMultipler) + " | " + damage);

                    // Create and Fire Laser
                    new BulletAttack
                    {
                        owner = activator.gameObject,
                        weapon = activator.gameObject,
                        origin = aimRay.origin,
                        aimVector = aimRay.direction,
                        minSpread = 0f,
                        maxSpread = 0f,
                        bulletCount = 1U,
                        procCoefficient = 2f,
                        damageType = DamageType.WeakPointHit,
                        damage = activator.damage * damage,
                        force = 1f,
                        falloffModel = BulletAttack.FalloffModel.None,
                        //muzzleName = MinigunState.muzzleName,
                        //hitEffectPrefab = ImpactRailgun,
                        tracerEffectPrefab = tracer,
                        isCrit = true, // true
                        HitEffectNormal = false,
                        radius = radius,
                        maxDistance = 2000f,
                        smartCollision = true,
                        stopperMask = LayerIndex.noDraw.mask
                    }.Fire();

                    // Mostly prevent gester of drown cheese, and flashing and sounds
                    if (self.subcooldownTimer < subCooldown)
                    {
                        self.subcooldownTimer = subCooldown;
                    }

                    activator.healthComponent.TakeDamageForce(aimRay.direction * -force, true, false);
                }
            }
            else
            {
                //Log.Warning("Xe Equipment not fired?");
            }
        }
    }
}