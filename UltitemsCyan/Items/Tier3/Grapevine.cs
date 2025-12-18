using RoR2;
using UnityEngine;
using static System.Math;
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

        //Play_acid_larva_impact

        private const float baseGrapeDropChance = 40f;
        //private const float stackGrapeDropChance = 25f;

        // For Slippery Buff
        public const float grapeBlockChance = 70f;
        public const int maxGrapes = 20;

        public override void Init(ConfigFile configs)
        {
            Log.Warning("-JYPrint Hello?!?! 3");
            const string itemName = "Grapevine";
            if (!CheckItemEnabledConfig(itemName, "Red", configs))
            {
                return;
            }
            item = CreateItemDef(
                "GRAPEVINE",
                itemName,
                "Chance on kill to drop grapes that block damage.",
                "<style=cIsHealing>40%</style> chance on kill to grow <style=cIsHealing>1</style> <style=cStack>(+1 per stack)</style> grape. <style=cIsHealing>70%</style> to <style=cIsHealing>block</style> incoming damage per grape. Block chance is <style=cIsUtility>unaffected by luck</style>.",
                "If you close your eyes, you can pretend their eyeballs",
                ItemTier.Tier3,
                UltAssets.GrapevineSprite,
                UltAssets.GrapevinePrefab,
                [ItemTag.CanBeTemporary, ItemTag.Damage]
            );
        }

        protected override void Hooks()
        {
            On.RoR2.GlobalEventManager.OnCharacterDeath += GlobalEventManager_OnCharacterDeath;
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
                    if (Util.CheckRoll(baseGrapeDropChance, killer.master.luck))
                    {
                        //Log.Warning("Dropping grape from " + victim.name);
                        _ = Util.PlaySound("Play_clayGrenadier_impact", victim.gameObject);
                        SpawnOrb(victim.transform.position, victim.transform.rotation, TeamComponent.GetObjectTeam(killer.gameObject), grabCount);
                    }
                }
            }
        }

        public static void SpawnOrb(Vector3 position, Quaternion rotation, TeamIndex teamIndex, int itemCount)
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
            GrapeComponent.amount = itemCount;
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
                //if (body && body.GetBuffCount(buffDef) < maxGrapes)
                //{
                //    body.AddBuff(buffDef);
                //}
                int addBuffs = Min(amount, maxGrapes - body.GetBuffCount(buffDef));
                //Log.Debug("Grape On Trigger Happened! amount: " + addBuffs);
                for (int i = 0; i < addBuffs; i++)
                {
                    body.AddBuff(buffDef);
                }
                _ = Util.PlaySound("Play_acid_larva_impact", body.gameObject);
                Destroy(baseObject);
            }
        }

        //private BuffDef buffDef = JunkContent.Buffs.BodyArmor;
        private readonly BuffDef buffDef = Buffs.SlipperyGrapeBuff.buff;
        public int amount;

        public GameObject baseObject;
        public TeamFilter teamFilter;
        public GameObject pickupEffect;

        private bool alive = true;
    }
}
