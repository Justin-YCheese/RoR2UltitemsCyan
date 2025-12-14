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
            if (equipmentDef == equipment)
            {
                CharacterBody activator = self.characterBody;

                // Add Get Obitial Token (Create Singleton if not set)
                OrbitalQuarkToken OrbitalToken = activator.gameObject.GetComponent<OrbitalQuarkToken>();
                if (!OrbitalToken)
                {
                    _ = activator.gameObject.AddComponent<OrbitalQuarkToken>();
                    OrbitalToken = activator.gameObject.GetComponent<OrbitalQuarkToken>();
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

/*
using RoR2;
using RoR2.Audio;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using R2API;
using MSU;
using RoR2.ContentManagement;
using MSU.Config;

namespace SS2.Equipments
{
    public class GreaterWarbanner : SS2Equipment
    {
        private const string token = "SS2_EQUIP_GREATERWARBANNER_DESC";

        public override SS2AssetRequest AssetRequest => SS2Assets.LoadAssetAsync<EquipmentAssetCollection>("acGreaterWarbanner", SS2Bundle.Equipments);

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of Extra Regeneration. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100)]
        public static float extraRegeneration = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of Extra Crit Chance. (100 = 100%)")]
        [FormatToken(token, 1)]
        public static float extraCrit = 20f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Amount of Cooldown Reduction. (1 = 100%)")]
        [FormatToken(token, FormatTokenAttribute.OperationTypeEnum.MultiplyByN, 100, 2)]
        public static float cooldownReduction = 0.5f;

        [RiskOfOptionsConfigureField(SS2Config.ID_ITEM, configDescOverride = "Max active warbanners for each character.")]
        [FormatToken(token, 3)]
        public static int maxGreaterBanners = 1;

        private GameObject _warbannerObject;

        public override bool Execute(EquipmentSlot slot)
        {
            var GBToken = slot.characterBody.gameObject.GetComponent<GreaterBannerToken>();
            if (!GBToken)
            {
                slot.characterBody.gameObject.AddComponent<GreaterBannerToken>();
                GBToken = slot.characterBody.gameObject.GetComponent<GreaterBannerToken>();
            }
            //To do: make better placement system
            Vector3 position = slot.inputBank.aimOrigin - (slot.inputBank.aimDirection);
            GameObject bannerObject = UnityEngine.Object.Instantiate(_warbannerObject, position, Quaternion.identity);

            bannerObject.GetComponent<TeamFilter>().teamIndex = slot.teamComponent.teamIndex;
            NetworkServer.Spawn(bannerObject);

            if (GBToken.soundCooldown >= 5f)
            {
                var sound = NetworkSoundEventCatalog.FindNetworkSoundEventIndex("GreaterWarbanner");
                EffectManager.SimpleSoundEffect(sound, bannerObject.transform.position, true);
                GBToken.soundCooldown = 0f;
            }

            GBToken.ownedBanners.Add(bannerObject);

            if (GBToken.ownedBanners.Count > maxGreaterBanners)
            {
                var oldBanner = GBToken.ownedBanners[0];
                GBToken.ownedBanners.RemoveAt(0);
                EffectData effectData = new EffectData
                {
                    origin = oldBanner.transform.position
                };
                effectData.SetNetworkedObjectReference(oldBanner);
                EffectManager.SpawnEffect(HealthComponent.AssetReferences.executeEffectPrefab, effectData, transmit: true);

                UnityEngine.Object.Destroy(oldBanner);
                NetworkServer.Destroy(oldBanner);
            }

            return true;
        }

        public override void Initialize()
        {
            _warbannerObject = AssetCollection.FindAsset<GameObject>("GreaterWarbannerWard");
            RegisterTempVisualEffects();
            On.RoR2.GenericSkill.RunRecharge += FasterTickrateBannerHook;
        }

        public override bool IsAvailable(ContentPack contentPack)
        {
            return true;
        }

        public override void OnEquipmentLost(CharacterBody body)
        {
        }

        public override void OnEquipmentObtained(CharacterBody body)
        {
        }

        private void FasterTickrateBannerHook(On.RoR2.GenericSkill.orig_RunRecharge orig, GenericSkill self, float dt)
        {
            if (self)
            {
                if (self.characterBody)
                {
                    if (self.characterBody.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
                    {
                        dt *= (1f + (2f * cooldownReduction));
                    }
                }
            }
            orig(self, dt);
        }

        private void RegisterTempVisualEffects()
        {
            // TODO: MSU 2.0
            //*var effectInstance = SS2Assets.LoadAsset<GameObject>("GreaterBannerBuffEffect", SS2Bundle.Equipments); 

            //TempVisualEffectAPI.AddTemporaryVisualEffect(effectInstance.InstantiateClone("GreaterBannerBuffEffect", false), (CharacterBody body) => { return body.HasBuff(SS2Content.Buffs.BuffGreaterBanner); }, true, "MainHurtbox");
        }

        public class GreaterBannerToken : MonoBehaviour
{
    //public GameObject[] ownedBanners = new GameObject[0];
    public List<GameObject> ownedBanners = new List<GameObject>(0);

    public float soundCooldown = 5f;

    private void FixedUpdate()
    {
        soundCooldown += Time.fixedDeltaTime;
    }
}

// TODO: Replace class with a single hook on RecalculateSTatsAPI.GetstatCoefficients. This way we replace the monobehaviour with just a method
public sealed class GreatBannerBuffBehavior : BaseBuffBehaviour, IBodyStatArgModifier
{
    [BuffDefAssociation]
    private static BuffDef GetBuffDef() => SS2Content.Buffs.BuffGreaterBanner;
    public void ModifyStatArguments(RecalculateStatsAPI.StatHookEventArgs args)
    {
        if (characterBody.HasBuff(SS2Content.Buffs.BuffGreaterBanner))
        {
            args.critAdd += GreaterWarbanner.extraCrit;
            args.regenMultAdd += GreaterWarbanner.extraRegeneration;
        }
    }
}
    }
}
//*/