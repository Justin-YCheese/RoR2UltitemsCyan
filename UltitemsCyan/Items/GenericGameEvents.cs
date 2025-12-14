using RoR2;
using UnityEngine;

// Credit to TooManyItems from shirograhm for showing me a much better way of changing damage values taken 

namespace UltitemsCyan
{
    public class GenericGameEvents
    {
        public delegate void DamageAttackerVictimEventHandler(DamageInfo damageInfo, GenericCharacterInfo attackerInfo, GenericCharacterInfo victimInfo);
        //public delegate void DamageReportEventHandler(DamageReport damageReport);

        //public static event DamageAttackerVictimEventHandler OnHitEnemy;
        public static event DamageAttackerVictimEventHandler BeforeTakeDamage;
        //public static event DamageReportEventHandler OnTakeDamage;

        internal static void Init()
        {
            On.RoR2.HealthComponent.Awake += (orig, self) =>
            {
                //Log.Warning(" [[]] [[]] [[]] Adding GenericDamageEvent");
                _ = self.gameObject.AddComponent<GenericDamageEvent>();
                orig(self);
            };
        }

        public class GenericDamageEvent : MonoBehaviour, IOnIncomingDamageServerReceiver
        {
            public HealthComponent healthComponent;
            public CharacterBody victimBody;

            public void Start()
            {
                healthComponent = GetComponent<HealthComponent>();
                if (!healthComponent)
                {
                    Destroy(this);
                    return;
                }
                victimBody = healthComponent.body;
            }

            public void OnIncomingDamageServer(DamageInfo damageInfo)
            {
                GenericCharacterInfo attackerInfo = new();
                if (damageInfo.attacker) attackerInfo = new GenericCharacterInfo(damageInfo.attacker.GetComponent<CharacterBody>());
                GenericCharacterInfo victimInfo = new(victimBody);
                BeforeTakeDamage?.Invoke(damageInfo, attackerInfo, victimInfo);
            }
        }
    }
    public struct GenericCharacterInfo
    {
        public GameObject gameObject;
        public CharacterBody body;
        public CharacterMaster master;
        public TeamComponent teamComponent;
        public HealthComponent healthComponent;
        public Inventory inventory;
        public TeamIndex teamIndex;
        public Vector3 aimOrigin;

        public GenericCharacterInfo(CharacterBody body)
        {
            //Log.Warning(" [[]] [[]] [[]] Create GenericCharacterInfo");
            this.body = body;
            gameObject = body ? body.gameObject : null;
            master = body ? body.master : null;
            teamComponent = body ? body.teamComponent : null;
            healthComponent = body ? body.healthComponent : null;
            inventory = master ? master.inventory : null;
            teamIndex = teamComponent ? teamComponent.teamIndex : TeamIndex.Neutral;
            aimOrigin = body ? body.aimOrigin : Random.insideUnitSphere.normalized;
        }
    }
}
