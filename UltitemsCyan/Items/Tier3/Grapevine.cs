using RoR2;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;
using BepInEx.Configuration;

namespace UltitemsCyan.Items.Tier3
{

    // TODO: Make better sound and visuals
    public class Grapevine : ItemBase
    {
        public static ItemDef item;
        private readonly static GameObject GrapeOrbPrefab = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Tooth/HealPack.prefab").WaitForCompletion();
        private readonly static GameObject GrapeEffect = Addressables.LoadAssetAsync<GameObject>("RoR2/Base/Common/VFX/HealthOrbEffect.prefab").WaitForCompletion();

        private const float baseGrapeDropChance = 50f;
        private const float stackGrapeDropChance = 25f;

        // For Slippery Buff
        public const float grapeBlockChance = 80f;
        public const int maxGrapes = 20;

        public override void Init(ConfigFile configs)
        {
            const string itemName = "Grapevine";
            if (!CheckItemEnabledConfig(itemName, "Red", configs))
            {
                return;
            }
            item = CreateItemDef(
                "GRAPEVINE",
                itemName,
                "Chance on kill to drop grapes that block damage.",
                "<style=cIsHealing>50%</style> <style=cStack>(+25% per stack)</style> chance on kill to grow a grape. <style=cIsHealing>85%</style> to <style=cIsHealing>block</style> incomming damage per grape. Block chance is <style=cIsUtility>unaffected by luck</style>.",
                "If you close your eyes, you can pretend their eyeballs",
                ItemTier.Tier3,
                UltAssets.GrapevineSprite,
                UltAssets.GrapevinePrefab,
                [ItemTag.CanBeTemporary, ItemTag.Damage]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath; ;
        }

        private void GlobalEventManager_OnCharacterDeath(On.RoR2.GlobalEventManager.orig_OnCharacterDeath orig, GlobalEventManager self, DamageReport damageReport)
        {
            orig(self, damageReport);
            if (self && damageReport.attacker && damageReport.attackerBody && damageReport.attackerBody.inventory)
            {
                CharacterBody killer = damageReport.attackerBody;
                CharacterBody victim = damageReport.victimBody;
                int grabCount = killer.inventory.GetItemCountEffective(item);
                //int buffCount = killer.GetBuffCount(Buffs.OverclockedBuff.buff);
                if (grabCount > 0)
                {
                    if (Util.CheckRoll(baseGrapeDropChance + (stackGrapeDropChance - 1) * grabCount, killer.master.luck))
                    {
                        //Log.Warning("Dropping grape from " + victim.name);
                        //RoR2.BuffPickup.Instantiate(item);
                        //Util.PlaySound("Play_hermitCrab_idle_VO", victim.gameObject);
                        //Util.PlaySound("Play_hermitCrab_idle_VO", victim.gameObject);
                        SpawnOrb(victim.transform.position, victim.transform.rotation, TeamComponent.GetObjectTeam(killer.gameObject));
                    }
                }
            }
        }

        public static void SpawnOrb(Vector3 position, Quaternion rotation, TeamIndex teamIndex)
        {
            GameObject orb = Object.Instantiate(GrapeOrbPrefab);
            if (orb)
            {
                //Log.Debug("Grape Orb is loaded");
            }

            orb.transform.position = position;
            orb.transform.rotation = rotation;
            orb.GetComponent<TeamFilter>().teamIndex = teamIndex;

            // * * Additions * * //
            VelocityRandomOnStart trajectory = orb.GetComponent<VelocityRandomOnStart>();
            trajectory.maxSpeed = 20;
            trajectory.minSpeed = 15;
            trajectory.directionMode = VelocityRandomOnStart.DirectionMode.Cone;
            trajectory.coneAngle = 1;


            HealthPickup healthComponent = orb.GetComponentInChildren<HealthPickup>();
            //Log.Debug("health Component? " + healthComponent.alive);
            healthComponent.alive = false;

            //BuffPickup
            GrapePickup GrapeComponent = healthComponent.gameObject.AddComponent<GrapePickup>();
            GrapeComponent.baseObject = orb;
            GrapeComponent.teamFilter = orb.GetComponent<TeamFilter>();
            GrapeComponent.pickupEffect = GrapeEffect;

            orb.GetComponent<Rigidbody>().useGravity = true;
            orb.transform.localScale = Vector3.one * 2.5f;

            //Log.Debug("Spawning orb at: " + orb.transform.position);
            NetworkServer.Spawn(orb);
        }
        //*/
    }

    public class GrapePickup : MonoBehaviour
    {
        private readonly int maxGrapes = Grapevine.maxGrapes;

#pragma warning disable IDE0051 // Remove unused private members
        private void OnTriggerStay(Collider other)
#pragma warning restore IDE0051 // Remove unused private members
        {
            if (NetworkServer.active && alive && TeamComponent.GetObjectTeam(other.gameObject) == teamFilter.teamIndex)
            {
                CharacterBody body = other.GetComponent<CharacterBody>();
                if (body && body.GetBuffCount(buffDef) < maxGrapes)
                {
                    body.AddBuff(buffDef);
                }
                Destroy(baseObject);
            }
        }

        //private BuffDef buffDef = JunkContent.Buffs.BodyArmor;
        private readonly BuffDef buffDef = Buffs.SlipperyGrapeBuff.buff;

        public GameObject baseObject;
        public TeamFilter teamFilter;
        public GameObject pickupEffect;

        private bool alive = true;
    }
}
