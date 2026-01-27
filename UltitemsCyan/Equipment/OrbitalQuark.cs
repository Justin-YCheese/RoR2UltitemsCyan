using BepInEx.Configuration;
using RoR2;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace UltitemsCyan.Equipment
{
    // Alloyed: CharacterMotor Changed signature for ModifyGravity
    // There's a ModifyGravity?
    // There's a public class SlowFallZone : MonoBehaviour ?

    public class OrbitalQuark : EquipmentBase
    {
        public static EquipmentDef equipment;

        //private static readonly GameObject QuartzWard = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/WardOnLevel/WarbannerWard.prefab").WaitForCompletion();
        private static readonly GameObject QuartzWard = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/CrippleWard/CrippleWard.prefab").WaitForCompletion();

        public const float buffSpeed = 40f;
        public const float buffDampening = 0.99f;
        public const float buffFallSpeed = 2f;
        public const float buffMinFallSpeed = 0.12f;

        public const float wardSize = 25f;
        public const int maxOrbits = 3;
        public const float wardDuration = 30f;
        public const float wardAboveOffset = 5f;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Orbital Quark";
            if (!CheckItemEnabledConfig(itemName, "Equipment", configs))
            {
                return;
            }
            equipment = CreateItemDef(
                "ORBITALQUARK",
                itemName,
                "Spawn a zero gravity zone for 30 seconds.",
                "Create a <style=cIsUtility>zero gravity</style> space for players and enemies. Last <style=cIsUtility>30 seconds</style>. Grants <style=cIsUtility>40% movement speed</style> to players.",
                "In space... I can do at least 3 flips before landing.",
                60f,
                false,
                true,
                true,
                UltAssets.OrbitalQuarkSprite,
                UltAssets.OrbitalQuarkPrefab
            );
        }

        protected override void Hooks()
        {
            On.RoR2.EquipmentSlot.PerformEquipmentAction += EquipmentSlot_PerformEquipmentAction;
        }

        private bool EquipmentSlot_PerformEquipmentAction(On.RoR2.EquipmentSlot.orig_PerformEquipmentAction orig, EquipmentSlot self, EquipmentDef equipmentDef)
        {
            if (NetworkServer.active && !self.equipmentDisabled && equipmentDef == equipment)
            {
                CharacterBody activator = self.characterBody;

                // Add Get Obitial Token (Create Singleton if not set)
                OrbitalQuarkToken OrbitalToken = activator.gameObject.GetComponent<OrbitalQuarkToken>();
                if (!OrbitalToken)
                {
                    OrbitalToken = activator.gameObject.AddComponent<OrbitalQuarkToken>();
                }

                Vector3 position = activator.corePosition;
                position.y += wardAboveOffset;

                // Keep track of Orbits
                GameObject gravityZone = UnityEngine.Object.Instantiate(QuartzWard, position, Util.QuaternionSafeLookRotation(Vector3.left));

                //gravityZone.transform.position = position;
                gravityZone.transform.rotation = Util.QuaternionSafeLookRotation(Vector3.down);

                UnityEngine.Object.Destroy(gravityZone.GetComponent<Deployable>());

                /*Rigidbody rigidBody = gravityZone.GetComponent<Rigidbody>();
                rigidBody.detectCollisions = false;
                Log.Debug(" ###* ###* Has Collisions? " + gravityZone.GetComponent<Rigidbody>().detectCollisions + " | Network?" + NetworkServer.active);*/

                TeamFilter teamFilter = gravityZone.GetComponent<TeamFilter>();
                teamFilter.teamIndex = TeamIndex.None;

                BuffWard buffWard = gravityZone.GetComponent<BuffWard>();
                buffWard.radius = wardSize;
                buffWard.Networkradius = wardSize;
                buffWard.buffDef = Buffs.QuarkGravityBuff.buff;
                buffWard.buffDuration = 0.1f;
                buffWard.interval = 0.08f;
                buffWard.floorWard = false; //default: true
                buffWard.invertTeamFilter = true;
                buffWard.expires = true;
                buffWard.expireDuration = wardDuration;

                /*Light light = gravityZone.GetComponent<Light>();
                light.color = new Color(1f, 0.2f, 1f, 1f);
                //light.type = LightType.Disc; // Spot = 0, Directional = 1, Point = 2, Area = 3, Rectangle = 3, Disc = 4
                //light.shape = LightShape.Cone; // Cone, Pyramid, Box
                light.intensity = 100;
                light.range = 2.8f;
                //light.shadowStrength = 10f;*/

                OrbitalToken.ownedOrbits.Enqueue(gravityZone);
                NetworkServer.Spawn(gravityZone);

                try
                {
                    if (OrbitalToken.ownedOrbits.Count > maxOrbits)
                    {
                        GameObject oldOrbit = OrbitalToken.ownedOrbits.Dequeue();
                        EffectData effectData = new()
                        {
                            origin = oldOrbit.transform.position
                        };
                        effectData.SetNetworkedObjectReference(oldOrbit);
                        EffectManager.SpawnEffect(HealthComponent.AssetReferences.executeEffectPrefab, effectData, transmit: true);
                        UnityEngine.Object.Destroy(oldOrbit);
                        NetworkServer.Destroy(oldOrbit);
                    }
                }
                catch (NullReferenceException) { }

                return true;
            }
            else
            {
                return orig(self, equipmentDef);
            }
        }
    }

    public class OrbitalQuarkToken : MonoBehaviour
    {
        //public GameObject[] ownedBanners = new GameObject[0];
        public Queue<GameObject> ownedOrbits = [];
    }
}