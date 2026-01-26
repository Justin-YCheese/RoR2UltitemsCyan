using BepInEx.Configuration;
using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UltitemsCyan.Equipment
{
    // Can't affect character motor or give buffs with equipment

    // TODO: the array that used to store your various equipment has changed from equipmentStateSlots[slot] to _equipmentStateSlots[slot][set].
    public class YieldSign : EquipmentBase
    {
        // Inflict Slowdown on self?
        public static EquipmentDef equipment;

        public const float cooldown = 10f;
        public const float subCooldown = 0.1f;

        public const float boostMultiplier = 3f;
        public const float boostHorizontalMultiplier = 1.75f;
        public const float boostMinMultiplier = 3f;
        public const float boostMaxMultiplier = 8f;

        public const float stopMultiplier = -.3f;
        public const float stopHorizontalMultiplier = 1.5f;
        public const float stopMinMultiplier = 0.4f;
        public const float stopMaxMultiplier = 0.8f;

        private const float yieldDamage = 3f; //300%
        private const float hitForce = 2500f;
        private const int radius = 8;

        private static readonly GameObject willOWisp = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/ExplodeOnDeath/WilloWispDelay.prefab").WaitForCompletion();
        private static readonly GameObject explosionGolem = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Golem/ExplosionGolem.prefab").WaitForCompletion();
        //AncientWispPillar ?

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Yield Sign";
            if (!CheckItemEnabledConfig(itemName, "Equipment", configs))
            {
                return;
            }
            equipment = CreateItemDef(
                "YIELDSIGN",
                itemName,
                "Alternate between multiplying speed and canceling it. Hit nearby enemies each time.",
                "Alternate between multiplying current <style=cIsUtility>speed by 300%</style> and <style=cIsUtility>canceling</style> it. Stun nearby enemies for <style=cIsDamage>300% damage</style>.",
                "Stop and go, the best of both worlds right?",
                cooldown,
                false,
                true,
                false,
                UltAssets.YieldSignSprite,
                UltAssets.YieldSignPrefab
            );
        }

        protected override void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
            On.RoR2.EquipmentSlot.RpcOnClientEquipmentActivationRecieved += EquipmentSlot_RpcOnClientEquipmentActivationRecieved;
        }

        //
        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active && self.equipmentDisabled && equipmentDef == equipment)
            {
                //Log.Warning("YIELD! Preform Equipment");
                //Log.Debug(" ))) --- ))) EquipmentSlot_PerformEquipmentAction ID: " + equipment.equipmentIndex);
                self.characterBody.inventory.SetEquipmentIndex(YieldSignStop.equipment.equipmentIndex, true);
                return true;
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }
        //*/

        private void EquipmentSlot_RpcOnClientEquipmentActivationRecieved(On.RoR2.EquipmentSlot.orig_RpcOnClientEquipmentActivationRecieved orig, EquipmentSlot self)
        {
            orig(self);
            if (self.equipmentIndex == equipment.equipmentIndex && self.characterBody && self.characterBody.characterMotor)
            {
                Log.Debug("RPC Equipment | Net? " + NetworkServer.active);
                if (NetworkServer.active)
                {
                    // Stop Multipliers because item switches on server first
                    YieldStopActivation(self);
                }
                else
                {
                    YieldBoostActivation(self);
                }
                //VelocityMultiplier(ref self.characterBody.characterMotor.velocity, boostMultiplier, boostHorizontalMultiplier, boostMaxMultiplier, boostMinMultiplier, self.characterBody.moveSpeed);
                
                //Log.Debug(" ))) --- ))) RPC ID: " + equipment.equipmentIndex);
            }
        }

        public static void YieldBoostActivation(EquipmentSlot self)
        {
            VelocityMultiplier(ref self.characterBody.characterMotor.velocity, boostMultiplier, boostHorizontalMultiplier, boostMaxMultiplier, boostMinMultiplier, self.characterBody.moveSpeed);
            YieldAttack(self.characterBody);
        }

        public static void YieldStopActivation(EquipmentSlot self)
        {
            VelocityMultiplier(ref self.characterBody.characterMotor.velocity, stopMultiplier, stopHorizontalMultiplier, stopMaxMultiplier, stopMinMultiplier, self.characterBody.moveSpeed);
            YieldAttack(self.characterBody);
        }

        public static void VelocityMultiplier(ref Vector3 velocity, float multiplier, float horizontalMultiplier, float maxMultiplier, float minMultiplier, float moveSpeed)
        {
            if (velocity == Vector3.zero)
            {
                velocity = Vector3.up;
            }

            velocity *= multiplier;
            velocity.x *= horizontalMultiplier;
            velocity.z *= horizontalMultiplier;

            float maxSpeed = moveSpeed * maxMultiplier; // / forceMultiplier
            float minSpeed = moveSpeed * minMultiplier; // / forceMultiplier
            //Log.Debug("Velocity exceeded bounds? | " + velocity.magnitude + " >< " + moveSpeed + " * " + maxMultiplier + " | " + velocity);// / 36
            if (velocity.magnitude > maxSpeed)
            {
                velocity = velocity.normalized * maxSpeed;
                //Log.Warning("New Max Velocity | " + velocity + " mag: " + velocity.magnitude);
            }
            else if (velocity.magnitude < minSpeed)
            {
                velocity = velocity.normalized * minSpeed;
                //Log.Warning("new min Velocity | " + velocity + " mag: " + velocity.magnitude);
            }
        }

        public static void YieldAttack(CharacterBody body)
        {
            // No more cleanse Body? CleanseSystem.CleanseBody
            body.characterMotor.disableAirControlUntilCollision = false;
            body.characterMotor.Motor.ForceUnground();

            GameObject explostionObject = Object.Instantiate(willOWisp, body.transform.position, Quaternion.identity);
            DelayBlast blast = explostionObject.GetComponent<DelayBlast>();

            blast.position = body.transform.position;
            blast.attacker = body.gameObject;
            blast.inflictor = body.gameObject;
            blast.baseDamage = yieldDamage * body.damage;
            blast.baseForce = hitForce;
            //blast.bonusForce = ;
            blast.radius = radius;
            blast.maxTimer = 0f;
            blast.falloffModel = BlastAttack.FalloffModel.None;
            blast.damageColorIndex = DamageColorIndex.Fragile;
            blast.damageType = DamageType.AOE | DamageType.Stun1s;
            blast.procCoefficient = 1f;

            blast.explosionEffect = explosionGolem;
            blast.hasSpawnedDelayEffect = true;

            blast.teamFilter = new TeamFilter()
            {
                teamIndexInternal = (int)body.teamComponent.teamIndex,
                defaultTeam = TeamIndex.None,
                teamIndex = body.teamComponent.teamIndex,
                NetworkteamIndexInternal = (int)body.teamComponent.teamIndex
            };

            _ = Util.PlaySound("Play_bellBody_attackLand", body.gameObject);
            _ = Util.PlaySound("Play_bellBody_impact", body.gameObject);
        }
    }
}

/*
 *      Failed tries
 * 
 * CharacterMotor.velocity  (Fails client, changing server velocity does nothing to client, client velocity not sent to server)
 * CharacterDirection       (Fails client, direction missing y direction)
 * ItemBehavior             (Fails client, When adding behavior to client body, server body is missing behavior)
 *                          (Adding on either server or client doesn't matter because either on fails client or one cannot be detected)
 * CharacterMotor.Jump      (Fails client, Does nothing for clients)
 * Health.TryForce          (Missing y speed, Does affect client)
 * Custom prePosition       (Janky, also disableAirControlUntilCollision never actually worked for client from server)
 * Previous Position        (Client's previous Position and current position are the same on the server. Probably also janky)
 * Rigidbody                (Fails client, ApplyForce doesn't do anything to either host or client)
 * AddBuff / SetBuffCount   (Causes an error)
 * OnBuffFirstStackGained   (doesn't run on client)
 * ClientRpc                (Doesn't do anything?)
 * 
 *      Sucess
 * 
 * OnBuffFinalStackLost     (Runs on client after timed buff ends)
 * EquipmentSlot_RpcOnClientEquipmentActivationRecieved (It was so simple...)
 */